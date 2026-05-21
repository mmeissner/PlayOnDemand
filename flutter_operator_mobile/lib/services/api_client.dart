import 'dart:convert';
import 'package:http/http.dart' as http;

/// The base URL for the Pod.Web.Center server. When served by the nginx
/// sidecar in docker-compose, the same host serves both the SPA and the API
/// (nginx proxies /api/* to the server container). An empty string means
/// relative URLs against the current origin.
const String apiBase =
    String.fromEnvironment('POD_API_BASE', defaultValue: '');

/// Wraps the full operator REST surface. Each method maps 1:1 to a swagger
/// endpoint; non-2xx responses throw `ApiException` so the caller can pluck
/// human-readable messages back out via `e.messages()`.
class ApiClient {
  String? _accessToken;

  void setToken(String? token) => _accessToken = token;

  Map<String, String> _jsonHeaders({bool auth = false}) {
    final h = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    };
    if (auth && _accessToken != null) {
      h['Authorization'] = 'Bearer $_accessToken';
    }
    return h;
  }

  Uri _u(String path, [Map<String, String>? query]) {
    final base = Uri.parse('$apiBase$path');
    if (query == null || query.isEmpty) return base;
    return base.replace(queryParameters: {...base.queryParameters, ...query});
  }

  // -----------------------------------------------------------------------
  // auth + accounts
  // -----------------------------------------------------------------------

  Future<Map<String, dynamic>> login(String username, String password) async {
    final r = await http.post(_u('/api/v1/auth/login'),
        headers: _jsonHeaders(),
        body: jsonEncode({'username': username, 'password': password}));
    return _decode(r);
  }

  Future<Map<String, dynamic>> refreshToken(String refreshToken) async {
    final r = await http.post(_u('/api/v1/auth/refreshtoken'),
        headers: _jsonHeaders(auth: true),
        body: jsonEncode({'refreshToken': refreshToken}));
    return _decode(r);
  }

  Future<void> logout() async {
    final r = await http.post(_u('/api/v1/auth/logout'),
        headers: _jsonHeaders(auth: true));
    if (r.statusCode >= 400) throw ApiException(r.statusCode, r.body);
  }

  Future<Map<String, dynamic>> changeAccountPassword(
      String currentPassword, String newPassword) async {
    final r = await http.post(_u('/api/v1/accounts/password/change'),
        headers: _jsonHeaders(auth: true),
        body: jsonEncode({
          'currentPassword': currentPassword,
          'newPassword': newPassword,
        }));
    return _decode(r);
  }

  Future<void> registerAccount({
    required String username,
    required String email,
    required String password,
  }) async {
    final r = await http.post(_u('/api/v1/accounts/register'),
        headers: _jsonHeaders(),
        body: jsonEncode({
          'username': username,
          'eMailAddress': email,
          'password': password,
        }));
    if (r.statusCode >= 400) throw ApiException(r.statusCode, r.body);
  }

  // -----------------------------------------------------------------------
  // stations - list / detail / create
  // -----------------------------------------------------------------------

  Future<List<dynamic>> listStations() async {
    final r = await http.get(_u('/api/v1/stations'),
        headers: _jsonHeaders(auth: true));
    return _decode(r) as List<dynamic>;
  }

  Future<Map<String, dynamic>> getStation(String stationId) async {
    final r = await http.get(_u('/api/v1/stations/$stationId'),
        headers: _jsonHeaders(auth: true));
    return _decode(r);
  }

  /// Server uses PUT for create (idempotent-by-displayName).
  Future<Map<String, dynamic>> createStation({
    required String displayName,
    required String password,
  }) async {
    final r = await http.put(_u('/api/v1/stations'),
        headers: _jsonHeaders(auth: true),
        body: jsonEncode({
          'displayName': displayName,
          'password': password,
        }));
    return _decode(r);
  }

  // -----------------------------------------------------------------------
  // station settings
  // -----------------------------------------------------------------------

  Future<Map<String, dynamic>> getStationSettings(String stationId) async {
    final r = await http.get(_u('/api/v1/stations/$stationId/settings'),
        headers: _jsonHeaders(auth: true));
    return _decode(r);
  }

  Future<Map<String, dynamic>> updateStationSettings(
    String stationId, {
    required String displayName,
    required int mode,
    String? qrCode,
  }) async {
    final body = <String, dynamic>{
      'displayName': displayName,
      'mode': mode,
    };
    if (qrCode != null) body['qrCode'] = qrCode;
    final r = await http.post(_u('/api/v1/stations/$stationId/settings'),
        headers: _jsonHeaders(auth: true), body: jsonEncode(body));
    return _decode(r);
  }

  Future<Map<String, dynamic>> updateStationMode(
      String stationId, int mode) async {
    final r = await http.post(_u('/api/v1/stations/$stationId/settings/mode'),
        headers: _jsonHeaders(auth: true), body: jsonEncode({'mode': mode}));
    return _decode(r);
  }

  Future<Map<String, dynamic>> updateStationQrCode(
      String stationId, String? qrCode) async {
    final r = await http.post(
        _u('/api/v1/stations/$stationId/settings/qrcode'),
        headers: _jsonHeaders(auth: true),
        body: jsonEncode({'qrCode': qrCode}));
    return _decode(r);
  }

  Future<void> resetStationPassword(
      String stationId, String newPassword) async {
    final r = await http.post(
        _u('/api/v1/stations/$stationId/settings/password'),
        headers: _jsonHeaders(auth: true),
        body: jsonEncode({'password': newPassword}));
    if (r.statusCode >= 400) throw ApiException(r.statusCode, r.body);
  }

  // -----------------------------------------------------------------------
  // station api keys
  // -----------------------------------------------------------------------

  Future<List<dynamic>> listApiKeys(String stationId) async {
    final r = await http.get(_u('/api/v1/stations/$stationId/apikeys'),
        headers: _jsonHeaders(auth: true));
    return _decode(r) as List<dynamic>;
  }

  /// Mint a new API key for the station. The server returns the freshly
  /// generated `secret` exactly once - subsequent list-calls elide it.
  Future<Map<String, dynamic>> mintApiKey(
      String stationId, String keyName) async {
    final r = await http.put(
        _u('/api/v1/stations/$stationId/apikeys', {'keyName': keyName}),
        headers: _jsonHeaders(auth: true));
    return _decode(r);
  }

  Future<void> deleteApiKey(String stationId, String publicKey) async {
    final r = await http.delete(
        _u('/api/v1/stations/$stationId/apikeys/$publicKey'),
        headers: _jsonHeaders(auth: true));
    if (r.statusCode >= 400) throw ApiException(r.statusCode, r.body);
  }

  // -----------------------------------------------------------------------
  // sessions
  // -----------------------------------------------------------------------

  Future<List<dynamic>> getStationSessions(String stationId) async {
    final r = await http.get(_u('/api/v1/stations/$stationId/sessions'),
        headers: _jsonHeaders(auth: true));
    return _decode(r) as List<dynamic>;
  }

  Future<List<dynamic>> listAllSessions() async {
    final r = await http.get(_u('/api/v1/stations/sessions'),
        headers: _jsonHeaders(auth: true));
    return _decode(r) as List<dynamic>;
  }

  /// Server contract is PUT for "send login intention".
  Future<Map<String, dynamic>> startSession(
    String stationId, {
    String? reference,
    Duration? duration,
  }) async {
    final body = <String, dynamic>{};
    if (reference != null) body['reference'] = reference;
    if (duration != null) body['duration'] = durationToTimeSpan(duration);
    final r = await http.put(_u('/api/v1/stations/$stationId/sessions'),
        headers: _jsonHeaders(auth: true), body: jsonEncode(body));
    return _decode(r);
  }

  Future<Map<String, dynamic>> stopSession(String stationId) async {
    final r = await http.post(
        _u('/api/v1/stations/$stationId/sessions/current/stop'),
        headers: _jsonHeaders(auth: true));
    return _decode(r);
  }

  Future<Map<String, dynamic>> updateSession(
    String stationId, {
    required Duration duration,
    String? reference,
  }) async {
    final body = <String, dynamic>{'duration': durationToTimeSpan(duration)};
    if (reference != null) body['reference'] = reference;
    final r = await http.post(
        _u('/api/v1/stations/$stationId/sessions/current/update'),
        headers: _jsonHeaders(auth: true),
        body: jsonEncode(body));
    return _decode(r);
  }

  // -----------------------------------------------------------------------
  // helpers
  // -----------------------------------------------------------------------

  /// .NET expects an ISO-8601 TimeSpan ("00:30:00"), not a number of ms.
  static String durationToTimeSpan(Duration d) {
    final h = d.inHours.toString().padLeft(2, '0');
    final m = (d.inMinutes % 60).toString().padLeft(2, '0');
    final s = (d.inSeconds % 60).toString().padLeft(2, '0');
    return '$h:$m:$s';
  }

  dynamic _decode(http.Response r) {
    if (r.statusCode < 200 || r.statusCode >= 300) {
      throw ApiException(r.statusCode, r.body);
    }
    if (r.body.isEmpty) return <String, dynamic>{};
    return jsonDecode(r.body);
  }
}

class ApiException implements Exception {
  final int statusCode;
  final String body;
  ApiException(this.statusCode, this.body);

  /// Try to surface the server's IResult<T> error dictionary as a flat list
  /// of human-readable messages. Falls back to the raw body.
  List<String> messages() {
    try {
      final decoded = jsonDecode(body);
      if (decoded is Map<String, dynamic>) {
        final errs = decoded['errors'] ?? decoded['Errors'] ?? decoded;
        if (errs is Map<String, dynamic>) {
          return errs.values
              .expand<dynamic>((v) => v is List ? v : [v])
              .map((e) => e.toString())
              .toList();
        }
        if (errs is List) return errs.map((e) => e.toString()).toList();
      }
    } catch (_) {}
    return [body.isEmpty ? 'HTTP $statusCode' : body];
  }

  @override
  String toString() => 'ApiException($statusCode): ${messages().join("; ")}';
}

// ---------------------------------------------------------------------------
// shared enum decoders
// ---------------------------------------------------------------------------

/// Server returns control mode as both string (in GET responses, e.g. "Local")
/// and as int code (in POST/PUT request bodies). Wire format from
/// Pod.Enums.StationControlMode:
///   0 = Undefined (invalid - server rejects)
///   1 = Local
///   2 = Remote
///   3 = RemoteWithQrCode
class StationControlMode {
  static const local = 1;
  static const remote = 2;
  static const remoteWithQrCode = 3;

  static int fromString(String s) {
    switch (s) {
      case 'Local':
        return local;
      case 'Remote':
        return remote;
      case 'RemoteWithQrCode':
        return remoteWithQrCode;
      default:
        return local;
    }
  }

  static String toLabel(int code) {
    switch (code) {
      case remote:
        return 'Remote';
      case remoteWithQrCode:
        return 'Remote (QR code)';
      case local:
      default:
        return 'Local';
    }
  }

  static String toApiString(int code) {
    switch (code) {
      case remote:
        return 'Remote';
      case remoteWithQrCode:
        return 'RemoteWithQrCode';
      case local:
      default:
        return 'Local';
    }
  }
}
