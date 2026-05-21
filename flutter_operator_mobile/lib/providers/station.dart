import 'package:flutter/foundation.dart';

import '../services/api_client.dart';

/// Per-station mutable state. Mirrors the StationCurrentState shape the server
/// returns. The session sub-record is materialised only when a session is
/// currently running on this station.
class Station with ChangeNotifier {
  final ApiClient _api;
  final String id;

  String _name;
  String _controlMode;
  String _networkState;
  StationSession? _session;
  bool _isBusy = false;

  String get name => _name;
  String get controlMode => _controlMode;
  String get networkState => _networkState;
  StationSession? get session => _session;
  bool get isBusy => _isBusy;
  bool get isConnected => _networkState == 'Connected';
  bool get hasSession => _session != null;

  Station.fromJson(this._api, Map<String, dynamic> json)
      : id = json['stationId'] as String,
        _name = (json['displayName'] as String?) ?? '',
        _controlMode = (json['controlMode'] as String?) ?? 'Local',
        _networkState =
            (json['networkState'] as String?) ?? 'Disconnected',
        _session = _readSession(json);

  void updateFromJson(Map<String, dynamic> json) {
    _name = (json['displayName'] as String?) ?? _name;
    _controlMode = (json['controlMode'] as String?) ?? _controlMode;
    _networkState = (json['networkState'] as String?) ?? _networkState;
    _session = _readSession(json);
    notifyListeners();
  }

  static StationSession? _readSession(Map<String, dynamic> json) {
    final s = json['session'];
    if (s is! Map<String, dynamic>) return null;
    return StationSession.fromJson(s);
  }

  void _setBusy(bool v) {
    _isBusy = v;
    notifyListeners();
  }

  // ----- actions -----------------------------------------------------------

  Future<String?> tryStartSession({Duration? duration}) async {
    if (_isBusy) return null;
    _setBusy(true);
    try {
      await _api.startSession(id, duration: duration);
      await _refresh();
      return null;
    } on ApiException catch (e) {
      return e.messages().join('; ');
    } catch (e) {
      return e.toString();
    } finally {
      _setBusy(false);
    }
  }

  Future<String?> tryStopSession() async {
    if (_isBusy) return null;
    _setBusy(true);
    try {
      await _api.stopSession(id);
      await _refresh();
      return null;
    } on ApiException catch (e) {
      return e.messages().join('; ');
    } catch (e) {
      return e.toString();
    } finally {
      _setBusy(false);
    }
  }

  Future<String?> tryUpdateSession({required Duration duration}) async {
    if (_isBusy) return null;
    _setBusy(true);
    try {
      await _api.updateSession(id, duration: duration);
      await _refresh();
      return null;
    } on ApiException catch (e) {
      return e.messages().join('; ');
    } catch (e) {
      return e.toString();
    } finally {
      _setBusy(false);
    }
  }

  Future<void> refresh() async {
    _setBusy(true);
    try {
      await _refresh();
    } finally {
      _setBusy(false);
    }
  }

  Future<void> _refresh() async {
    final json = await _api.getStation(id);
    updateFromJson(json);
  }
}

/// Lean projection of the server's session payload, only the fields the
/// operator UI actually displays.
class StationSession {
  final String sessionId;
  final String state;
  final DateTime? startedOnUtc;
  final Duration? startDuration;
  final Duration? maxDurationLimit;
  final String? reference;

  const StationSession({
    required this.sessionId,
    required this.state,
    this.startedOnUtc,
    this.startDuration,
    this.maxDurationLimit,
    this.reference,
  });

  factory StationSession.fromJson(Map<String, dynamic> j) => StationSession(
        sessionId: j['sessionId'] as String,
        state: (j['state'] as String?) ?? 'Unknown',
        startedOnUtc: _parseDate(j['startedOnUtc']),
        startDuration: _parseTimeSpan(j['startDuration']),
        maxDurationLimit: _parseTimeSpan(j['maxDurationLimit']),
        reference: j['reference'] as String?,
      );

  static DateTime? _parseDate(Object? v) =>
      v is String ? DateTime.tryParse(v) : null;

  static Duration? _parseTimeSpan(Object? v) {
    if (v is! String) return null;
    final parts = v.split(':');
    if (parts.length != 3) return null;
    final h = int.tryParse(parts[0]) ?? 0;
    final m = int.tryParse(parts[1]) ?? 0;
    final secParts = parts[2].split('.');
    final s = int.tryParse(secParts[0]) ?? 0;
    return Duration(hours: h, minutes: m, seconds: s);
  }
}
