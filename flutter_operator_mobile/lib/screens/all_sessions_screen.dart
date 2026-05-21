import 'dart:async';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../providers/stations.dart';
import '../services/api_client.dart';
import '../widgets/app_icons.dart';

const _allSessionsPoll = Duration(seconds: 8);

/// Admin-facing overview of every session across every station. Surfaces the
/// `/api/v1/stations/sessions` endpoint. Supports filter by state.
class AllSessionsScreen extends StatefulWidget {
  const AllSessionsScreen({super.key});

  @override
  State<AllSessionsScreen> createState() => _AllSessionsScreenState();
}

class _AllSessionsScreenState extends State<AllSessionsScreen>
    with WidgetsBindingObserver {
  List<Map<String, dynamic>>? _items;
  String? _err;
  bool _busy = false;
  String _filterState = 'All';
  Timer? _poll;

  static const _states = ['All', 'Active', 'Ended'];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _load();
      _startPolling();
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _poll?.cancel();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      _startPolling();
    } else {
      _poll?.cancel();
    }
  }

  void _startPolling() {
    _poll?.cancel();
    _poll = Timer.periodic(_allSessionsPoll, (_) {
      if (mounted && !_busy) _load(silent: true);
    });
  }

  Future<void> _load({bool silent = false}) async {
    if (!silent) {
      setState(() {
        _busy = true;
        _err = null;
      });
    }
    try {
      final api = context.read<ApiClient>();
      final raw = await api.listAllSessions();
      final list =
          raw.whereType<Map<String, dynamic>>().toList(growable: false);
      list.sort((a, b) {
        final ax = a['startedUtc'] as String? ?? '';
        final bx = b['startedUtc'] as String? ?? '';
        return bx.compareTo(ax);
      });
      if (!mounted) return;
      setState(() {
        _items = list;
        _busy = false;
        _err = null;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _err = e is ApiException ? e.messages().join('\n') : e.toString();
        _busy = false;
      });
    }
  }

  bool _passesFilter(Map<String, dynamic> s) {
    if (_filterState == 'All') return true;
    final st = s['latestState'] as String? ?? '';
    if (_filterState == 'Active') {
      return st != 'Ended' && st != 'Canceled';
    }
    if (_filterState == 'Ended') {
      return st == 'Ended' || st == 'Canceled';
    }
    return true;
  }

  @override
  Widget build(BuildContext context) {
    final stations = context.watch<StationsProvider>();
    final stationNamesById = {
      for (final st in stations.stations) st.id: st.name,
    };

    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            AppIcons.img(AppIcons.statistics, size: 24),
            const SizedBox(width: 8),
            const Text('All sessions'),
          ],
        ),
        bottom: _busy
            ? const PreferredSize(
                preferredSize: Size.fromHeight(3),
                child: LinearProgressIndicator(minHeight: 3),
              )
            : null,
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            child: Row(
              children: [
                const Text('Show:'),
                const SizedBox(width: 8),
                ..._states.map((s) => Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 4),
                      child: ChoiceChip(
                        label: Text(s),
                        selected: _filterState == s,
                        onSelected: (_) => setState(() => _filterState = s),
                      ),
                    )),
              ],
            ),
          ),
          Expanded(child: _body(stationNamesById)),
        ],
      ),
    );
  }

  Widget _body(Map<String, String> stationNamesById) {
    if (_busy && _items == null) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_err != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            AppIcons.img(AppIcons.stationDisconnect, size: 80),
            const SizedBox(height: 12),
            Text('Could not load\n$_err', textAlign: TextAlign.center),
            const SizedBox(height: 12),
            FilledButton(
                onPressed: () => _load(), child: const Text('Retry')),
          ],
        ),
      );
    }
    final all = _items ?? const [];
    final filtered = all.where(_passesFilter).toList();
    if (filtered.isEmpty) {
      return RefreshIndicator(
        onRefresh: () => _load(),
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          children: [
            const SizedBox(height: 80),
            Center(
              child: Column(
                children: [
                  AppIcons.img(AppIcons.statistics, size: 80),
                  const SizedBox(height: 12),
                  Text('No sessions match "$_filterState".'),
                ],
              ),
            ),
          ],
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: () => _load(),
      child: ListView.builder(
        physics: const AlwaysScrollableScrollPhysics(),
        itemCount: filtered.length + 1,
        itemBuilder: (_, i) {
          if (i == 0) {
            return Padding(
              padding: const EdgeInsets.all(12.0),
              child: Text(
                '${filtered.length} of ${all.length} sessions',
                style: Theme.of(context).textTheme.titleSmall,
              ),
            );
          }
          final s = filtered[i - 1];
          final stationName =
              stationNamesById[s['stationId']] ?? s['stationId'] ?? '';
          return _SessionRow(s: s, stationName: stationName as String);
        },
      ),
    );
  }
}

class _SessionRow extends StatelessWidget {
  const _SessionRow({required this.s, required this.stationName});
  final Map<String, dynamic> s;
  final String stationName;

  @override
  Widget build(BuildContext context) {
    final state = (s['latestState'] as String?) ?? '';
    final startedUtc = s['startedUtc'] as String?;
    final endedUtc = s['endedUtc'] as String?;
    final stoppedBy = s['stoppedBy'] as String?;
    final reference = s['reference'] as String?;
    final isEnded = state == 'Ended' || state == 'Canceled';
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      child: ExpansionTile(
        leading: AppIcons.img(
            isEnded ? AppIcons.stop : AppIcons.stationSession,
            size: 32),
        title: Text(stationName),
        subtitle: Text(reference?.isNotEmpty == true ? reference! : state),
        trailing: Chip(
          label: Text(state, style: const TextStyle(fontSize: 11)),
          visualDensity: VisualDensity.compact,
          backgroundColor:
              isEnded ? Colors.grey.shade200 : Colors.lightGreen.shade100,
        ),
        children: [
          Padding(
            padding:
                const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _kv('Session id', s['sessionId']?.toString() ?? ''),
                if (s['requestedBy'] != null)
                  _kv('Requested by', s['requestedBy'].toString()),
                if (stoppedBy != null) _kv('Stopped by', stoppedBy),
                if (startedUtc != null)
                  _kv('Started', _fmt(startedUtc)),
                if (endedUtc != null) _kv('Ended', _fmt(endedUtc)),
              ],
            ),
          ),
        ],
      ),
    );
  }

  static String _fmt(String iso) {
    final dt = DateTime.tryParse(iso)?.toLocal();
    if (dt == null) return iso;
    return DateFormat.yMd().add_Hm().format(dt);
  }

  Widget _kv(String k, String v) => Padding(
        padding: const EdgeInsets.symmetric(vertical: 2),
        child: Row(
          children: [
            SizedBox(
                width: 110,
                child:
                    Text(k, style: const TextStyle(color: Colors.black54))),
            Expanded(child: SelectableText(v)),
          ],
        ),
      );
}
