/// Persisted JWT bundle. The accessToken is what every authenticated REST call
/// carries via `Authorization: Bearer ...`; the refreshToken trades for a new
/// accessToken before the current one expires.
///
/// `accessTokenValidUntil` is stored in UTC because the device's local time-
/// zone can change between launches.
class AuthData {
  final String username;
  final String accessToken;
  final DateTime accessTokenValidUntil;
  final String refreshToken;

  const AuthData({
    required this.username,
    required this.accessToken,
    required this.accessTokenValidUntil,
    required this.refreshToken,
  });

  /// Treat as "expired" 30s before the server thinks so, so we never present
  /// an about-to-expire token on the wire.
  DateTime get safeValidUntil =>
      accessTokenValidUntil.subtract(const Duration(seconds: 30));

  bool get needsRefresh => DateTime.now().toUtc().isAfter(safeValidUntil);

  AuthData copyWith({
    String? username,
    String? accessToken,
    DateTime? accessTokenValidUntil,
    String? refreshToken,
  }) =>
      AuthData(
        username: username ?? this.username,
        accessToken: accessToken ?? this.accessToken,
        accessTokenValidUntil:
            accessTokenValidUntil ?? this.accessTokenValidUntil,
        refreshToken: refreshToken ?? this.refreshToken,
      );
}
