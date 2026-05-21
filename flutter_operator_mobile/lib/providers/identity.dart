import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../models/auth_data.dart';
import '../models/result.dart';
import '../services/api_client.dart';

/// Owns the operator's auth state. Wraps the legacy `IdentityProvider`'s
/// behavior into a null-safe `ChangeNotifier` on top of the shared
/// `ApiClient`. The token-refresh timer is the v1.x feature that the simple
/// `flutter_operator/` reference SPA didn't have.
class IdentityProvider with ChangeNotifier {
  static const _prefUsername = 'username';
  static const _prefAccessToken = 'accessToken';
  static const _prefAccessTokenValidUntil = 'accessTokenValidUntil';
  static const _prefRefreshToken = 'refreshToken';

  final ApiClient _api;
  AuthData? _authData;
  Timer? _refreshTimer;

  IdentityProvider(this._api);

  AuthData? get authData => _authData;
  bool get isAuthenticated => _authData != null;
  String? get username => _authData?.username;

  // ----- auth -------------------------------------------------------------

  /// Try to revive a session from `SharedPreferences`. If the persisted
  /// access token is already past its expiry, refresh it. Returns true if
  /// we ended up with a valid `AuthData`.
  Future<bool> tryAutoLogin() async {
    try {
      final stored = await _load();
      if (stored == null) return false;
      AuthData data = stored;
      if (data.needsRefresh) {
        final refreshed = await _refresh(data);
        if (!refreshed.isSuccess) return false;
        data = refreshed.value!;
        await _save(data);
      }
      _onLoggedIn(data);
      return true;
    } catch (e) {
      debugPrint('Auto-login failed: $e');
      return false;
    }
  }

  Future<Result<bool>> login(String username, String password) async {
    try {
      final requestedAtUtc = DateTime.now().toUtc();
      final r = await _api.login(username, password);
      final accessToken =
          (r['accessToken'] as Map<String, dynamic>)['token'] as String;
      final expiresIn =
          (r['accessToken'] as Map<String, dynamic>)['expiresIn'] as int;
      final refreshToken = r['refreshToken'] as String;

      final data = AuthData(
        username: username,
        accessToken: accessToken,
        accessTokenValidUntil:
            requestedAtUtc.add(Duration(seconds: expiresIn)),
        refreshToken: refreshToken,
      );
      await _save(data);
      _onLoggedIn(data);
      return Result.ok(true);
    } on ApiException catch (e) {
      return Result.error(e.messages());
    } catch (e) {
      return Result.errorFromException(e);
    }
  }

  Future<void> logout() async {
    _refreshTimer?.cancel();
    _refreshTimer = null;
    final hadSession = _authData != null;
    _authData = null;
    _api.setToken(null);
    await _clear();
    if (hadSession) {
      try {
        await _api.logout();
      } catch (e) {
        // Best-effort; we've cleared locally either way.
        debugPrint('Server-side logout call failed: $e');
      }
    }
    notifyListeners();
  }

  // ----- internals --------------------------------------------------------

  void _onLoggedIn(AuthData data) {
    _authData = data;
    _api.setToken(data.accessToken);
    _scheduleRefresh();
    notifyListeners();
  }

  void _scheduleRefresh() {
    _refreshTimer?.cancel();
    final data = _authData;
    if (data == null) return;
    final delta = data.safeValidUntil.difference(DateTime.now().toUtc());
    // If already-due, fire after 1s to avoid pinning the CPU on errors.
    final delay = delta.isNegative ? const Duration(seconds: 1) : delta;
    _refreshTimer = Timer(delay, _refreshTick);
  }

  Future<void> _refreshTick() async {
    final data = _authData;
    if (data == null) return;
    final refreshed = await _refresh(data);
    if (refreshed.isSuccess) {
      await _save(refreshed.value!);
      _onLoggedIn(refreshed.value!);
    } else if (_authData?.needsRefresh ?? false) {
      // Couldn't refresh and the token is now expired: drop session.
      await logout();
    } else {
      // Token still has some life - retry shortly.
      _refreshTimer = Timer(const Duration(seconds: 5), _refreshTick);
    }
  }

  Future<Result<AuthData>> _refresh(AuthData data) async {
    try {
      final requestedAtUtc = DateTime.now().toUtc();
      // The refreshtoken endpoint accepts the current access token in the
      // Authorization header (server allows the about-to-expire token here).
      _api.setToken(data.accessToken);
      final r = await _api.refreshToken(data.refreshToken);
      final token = r['token'] as String;
      final expiresIn = r['expiresIn'] as int;
      return Result.ok(data.copyWith(
        accessToken: token,
        accessTokenValidUntil:
            requestedAtUtc.add(Duration(seconds: expiresIn)),
      ));
    } catch (e) {
      return Result.errorFromException(e);
    }
  }

  // ----- persistence ------------------------------------------------------

  Future<AuthData?> _load() async {
    final p = await SharedPreferences.getInstance();
    if (!p.containsKey(_prefUsername)) return null;
    try {
      return AuthData(
        username: p.getString(_prefUsername)!,
        accessToken: p.getString(_prefAccessToken)!,
        accessTokenValidUntil:
            DateTime.parse(p.getString(_prefAccessTokenValidUntil)!),
        refreshToken: p.getString(_prefRefreshToken)!,
      );
    } catch (_) {
      return null;
    }
  }

  Future<void> _save(AuthData d) async {
    final p = await SharedPreferences.getInstance();
    await p.setString(_prefUsername, d.username);
    await p.setString(_prefAccessToken, d.accessToken);
    await p.setString(
        _prefAccessTokenValidUntil, d.accessTokenValidUntil.toIso8601String());
    await p.setString(_prefRefreshToken, d.refreshToken);
  }

  Future<void> _clear() async {
    final p = await SharedPreferences.getInstance();
    await p.remove(_prefUsername);
    await p.remove(_prefAccessToken);
    await p.remove(_prefAccessTokenValidUntil);
    await p.remove(_prefRefreshToken);
  }

  @override
  void dispose() {
    _refreshTimer?.cancel();
    super.dispose();
  }
}
