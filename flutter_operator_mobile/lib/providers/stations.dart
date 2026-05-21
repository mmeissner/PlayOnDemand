import 'dart:collection';

import 'package:flutter/foundation.dart';

import '../services/api_client.dart';
import 'station.dart';

/// Owns the station collection. Re-fetches via `GET /api/v1/stations`; per-
/// station mutating actions live on the `Station` nodes themselves and emit
/// their own change notifications.
class StationsProvider with ChangeNotifier {
  final ApiClient _api;
  final Map<String, Station> _items = {};

  bool _isLoading = false;
  bool _hasLoadedOnce = false;
  String? _error;

  StationsProvider(this._api);

  UnmodifiableListView<Station> get stations =>
      UnmodifiableListView(_items.values);
  bool get isLoading => _isLoading;
  bool get hasLoadedOnce => _hasLoadedOnce;
  String? get error => _error;

  Station? byId(String stationId) => _items[stationId];

  Future<void> refresh() async {
    if (_isLoading) return;
    _isLoading = true;
    _error = null;
    notifyListeners();
    try {
      final list = await _api.listStations();
      final seen = <String>{};
      for (final raw in list) {
        if (raw is! Map<String, dynamic>) continue;
        final id = raw['stationId'] as String;
        seen.add(id);
        final existing = _items[id];
        if (existing == null) {
          _items[id] = Station.fromJson(_api, raw);
        } else {
          existing.updateFromJson(raw);
        }
      }
      _items.removeWhere((id, _) => !seen.contains(id));
      _hasLoadedOnce = true;
    } on ApiException catch (e) {
      _error = e.messages().join('; ');
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  void clear() {
    _items.clear();
    _hasLoadedOnce = false;
    _error = null;
    notifyListeners();
  }
}
