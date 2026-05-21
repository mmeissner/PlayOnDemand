import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../providers/station.dart';
import '../services/api_client.dart';
import '../widgets/app_icons.dart';

/// Detail-screen poll cadence. Fast enough to catch a kiosk-side accept of a
/// pending login intention (`Requested -> Delivered -> Started`) within a tick.
const _detailPoll = Duration(seconds: 3);
/// Sessions / API-key list polling — change less often, so poll slower.
const _listPoll = Duration(seconds: 8);

/// Full per-station view with four tabs covering everything the REST API
/// can do for one station:
///   - Overview   : live state + current session card + start/stop/extend
///   - Sessions   : recent session history (scrollable)
///   - API Keys   : list / mint (with one-shot Secret reveal) / revoke
///   - Settings   : rename, change control mode, edit QR code, rotate password
class StationDetailScreen extends StatefulWidget {
  const StationDetailScreen({super.key});

  @override
  State<StationDetailScreen> createState() => _StationDetailScreenState();
}

class _StationDetailScreenState extends State<StationDetailScreen>
    with SingleTickerProviderStateMixin, WidgetsBindingObserver {
  late final TabController _tabs;
  Timer? _poll;

  @override
  void initState() {
    super.initState();
    _tabs = TabController(length: 4, vsync: this);
    WidgetsBinding.instance.addObserver(this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<Station>().refresh();
      _startPolling();
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _poll?.cancel();
    _tabs.dispose();
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
    _poll = Timer.periodic(_detailPoll, (_) {
      if (!mounted) return;
      final s = context.read<Station>();
      if (!s.isBusy) s.refresh();
    });
  }

  @override
  Widget build(BuildContext context) {
    final station = context.watch<Station>();
    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            Flexible(
              child: Text(station.name,
                  overflow: TextOverflow.ellipsis),
            ),
            const SizedBox(width: 8),
            const _LiveDot(),
          ],
        ),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(50),
          child: Column(
            children: [
              if (station.isBusy)
                const LinearProgressIndicator(minHeight: 2),
              TabBar(
                controller: _tabs,
                tabs: const [
                  Tab(icon: Icon(Icons.dashboard), text: 'Overview'),
                  Tab(icon: Icon(Icons.history), text: 'Sessions'),
                  Tab(icon: Icon(Icons.key), text: 'API keys'),
                  Tab(icon: Icon(Icons.settings), text: 'Settings'),
                ],
              ),
            ],
          ),
        ),
      ),
      body: TabBarView(
        controller: _tabs,
        children: const [
          _OverviewTab(),
          _SessionsTab(),
          _ApiKeysTab(),
          _SettingsTab(),
        ],
      ),
    );
  }
}

class _LiveDot extends StatefulWidget {
  const _LiveDot();
  @override
  State<_LiveDot> createState() => _LiveDotState();
}

class _LiveDotState extends State<_LiveDot>
    with SingleTickerProviderStateMixin {
  late final AnimationController _c = AnimationController(
      vsync: this, duration: const Duration(milliseconds: 1200))
    ..repeat(reverse: true);
  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FadeTransition(
      opacity: _c,
      child: Container(
        width: 8,
        height: 8,
        decoration: const BoxDecoration(
            color: Colors.redAccent, shape: BoxShape.circle),
      ),
    );
  }
}

// ============================================================================
// Overview tab
// ============================================================================

class _OverviewTab extends StatelessWidget {
  const _OverviewTab();

  @override
  Widget build(BuildContext context) {
    final station = context.watch<Station>();
    return RefreshIndicator(
      onRefresh: () => station.refresh(),
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16),
        children: [
          _StationStateCard(station: station),
          const SizedBox(height: 16),
          _CurrentSessionCard(station: station),
          const SizedBox(height: 16),
          _ActionsCard(station: station),
        ],
      ),
    );
  }
}

class _StationStateCard extends StatelessWidget {
  const _StationStateCard({required this.station});
  final Station station;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                AppIcons.img(
                    AppIcons.stationStateAsset(
                        isConnected: station.isConnected,
                        hasSession: station.hasSession),
                    size: 56),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(station.name,
                          style: Theme.of(context).textTheme.titleLarge),
                      const SizedBox(height: 4),
                      Text(station.id,
                          style: TextStyle(
                              fontSize: 12, color: Colors.grey[600])),
                    ],
                  ),
                ),
              ],
            ),
            const Divider(height: 24),
            Row(
              children: [
                AppIcons.img(AppIcons.controlModeAsset(station.controlMode),
                    size: 28),
                const SizedBox(width: 8),
                Text(StationControlMode.toLabel(
                    StationControlMode.fromString(station.controlMode))),
                const Spacer(),
                Chip(
                  avatar: Icon(
                    station.isConnected
                        ? Icons.cloud_done
                        : Icons.cloud_off,
                    color: station.isConnected
                        ? Colors.green
                        : Colors.grey,
                    size: 18,
                  ),
                  label: Text(station.networkState),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _CurrentSessionCard extends StatefulWidget {
  const _CurrentSessionCard({required this.station});
  final Station station;

  @override
  State<_CurrentSessionCard> createState() => _CurrentSessionCardState();
}

class _CurrentSessionCardState extends State<_CurrentSessionCard> {
  late final Stream<DateTime> _tick =
      Stream.periodic(const Duration(seconds: 1), (_) => DateTime.now().toUtc());

  @override
  Widget build(BuildContext context) {
    final s = widget.station.session;
    if (s == null) {
      return Card(
        child: ListTile(
          leading: AppIcons.img(AppIcons.stationIdle, size: 36),
          title: const Text('No active session'),
          subtitle: const Text('Use Start below to send a login intention.'),
        ),
      );
    }
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                AppIcons.img(AppIcons.stationSession, size: 36),
                const SizedBox(width: 12),
                Text('Session: ${s.state}',
                    style: Theme.of(context).textTheme.titleMedium),
              ],
            ),
            const SizedBox(height: 12),
            if (s.reference != null && s.reference!.isNotEmpty)
              _kv('Reference', s.reference!),
            _kv('Session id', s.sessionId),
            if (s.startedOnUtc != null)
              _kv(
                  'Started',
                  DateFormat.yMd()
                      .add_Hms()
                      .format(s.startedOnUtc!.toLocal())),
            if (s.maxDurationLimit != null)
              _kv('Max duration', _fmtDuration(s.maxDurationLimit!)),
            if (s.startedOnUtc != null && s.maxDurationLimit != null)
              StreamBuilder<DateTime>(
                stream: _tick,
                builder: (_, __) =>
                    _kv('Remaining', _remainingText(s)),
              ),
          ],
        ),
      ),
    );
  }

  Widget _kv(String k, String v) => Padding(
        padding: const EdgeInsets.symmetric(vertical: 2),
        child: Row(
          children: [
            SizedBox(
                width: 120,
                child:
                    Text(k, style: const TextStyle(color: Colors.black54))),
            Expanded(
              child: Text(v,
                  style: const TextStyle(fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      );

  static String _remainingText(StationSession s) {
    final end = s.startedOnUtc!.add(s.maxDurationLimit!);
    final remaining = end.difference(DateTime.now().toUtc());
    if (remaining.isNegative) return 'overdue by ${_fmtDuration(-remaining)}';
    return _fmtDuration(remaining);
  }

  static String _fmtDuration(Duration d) {
    final h = d.inHours;
    final m = d.inMinutes % 60;
    final s = d.inSeconds % 60;
    return [
      if (h > 0) '${h}h',
      if (m > 0 || h > 0) '${m}m',
      '${s}s',
    ].join(' ');
  }
}

class _ActionsCard extends StatelessWidget {
  const _ActionsCard({required this.station});
  final Station station;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Wrap(
          spacing: 12,
          runSpacing: 12,
          children: [
            if (station.hasSession) ...[
              _action(context,
                  asset: AppIcons.updateSession,
                  label: 'Extend',
                  onTap: () => _extend(context)),
              _action(context,
                  asset: AppIcons.stop,
                  label: 'Stop',
                  onTap: () => _stop(context)),
            ] else if (station.isConnected)
              _action(context,
                  asset: AppIcons.play,
                  label: 'Start session',
                  onTap: () => _start(context)),
            if (!station.isConnected)
              _disabled(context,
                  asset: AppIcons.stationDisconnect,
                  label: 'Station offline'),
          ],
        ),
      ),
    );
  }

  Widget _action(BuildContext context,
      {required String asset,
      required String label,
      required VoidCallback onTap}) {
    return ElevatedButton.icon(
      onPressed: station.isBusy ? null : onTap,
      icon: AppIcons.img(asset, size: 28),
      label: Text(label),
      style:
          ElevatedButton.styleFrom(padding: const EdgeInsets.all(16)),
    );
  }

  Widget _disabled(BuildContext context,
      {required String asset, required String label}) {
    return Opacity(
      opacity: 0.5,
      child: ElevatedButton.icon(
        onPressed: null,
        icon: AppIcons.img(asset, size: 28),
        label: Text(label),
      ),
    );
  }

  Future<void> _start(BuildContext context) async {
    final duration = await _pickDuration(context, title: 'Start session for');
    if (duration == null) return;
    if (!context.mounted) return;
    final err =
        await context.read<Station>().tryStartSession(duration: duration);
    if (err != null && context.mounted) _toast(context, err, isError: true);
  }

  Future<void> _extend(BuildContext context) async {
    final duration =
        await _pickDuration(context, title: 'Extend session by');
    if (duration == null) return;
    if (!context.mounted) return;
    final err =
        await context.read<Station>().tryUpdateSession(duration: duration);
    if (err != null && context.mounted) _toast(context, err, isError: true);
  }

  Future<void> _stop(BuildContext context) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Stop session?'),
        content:
            const Text('This will end the current session on this station.'),
        actions: [
          TextButton(
              onPressed: () => Navigator.of(ctx).pop(false),
              child: const Text('Cancel')),
          FilledButton(
              onPressed: () => Navigator.of(ctx).pop(true),
              child: const Text('Stop')),
        ],
      ),
    );
    if (confirm != true) return;
    if (!context.mounted) return;
    final err = await context.read<Station>().tryStopSession();
    if (err != null && context.mounted) _toast(context, err, isError: true);
  }

  Future<Duration?> _pickDuration(BuildContext context,
      {required String title}) async {
    return showDialog<Duration>(
      context: context,
      builder: (ctx) => SimpleDialog(
        title: Text(title),
        children: const [
          _DurationOption(label: '15 minutes', minutes: 15),
          _DurationOption(label: '30 minutes', minutes: 30),
          _DurationOption(label: '1 hour', minutes: 60),
          _DurationOption(label: '2 hours', minutes: 120),
          _DurationOption(label: '4 hours', minutes: 240),
        ],
      ),
    );
  }
}

class _DurationOption extends StatelessWidget {
  const _DurationOption({required this.label, required this.minutes});
  final String label;
  final int minutes;

  @override
  Widget build(BuildContext context) {
    return SimpleDialogOption(
      onPressed: () =>
          Navigator.of(context).pop(Duration(minutes: minutes)),
      child: Text(label),
    );
  }
}

// ============================================================================
// Sessions tab
// ============================================================================

class _SessionsTab extends StatefulWidget {
  const _SessionsTab();

  @override
  State<_SessionsTab> createState() => _SessionsTabState();
}

class _SessionsTabState extends State<_SessionsTab>
    with AutomaticKeepAliveClientMixin {
  List<Map<String, dynamic>>? _items;
  String? _err;
  bool _busy = false;
  Timer? _poll;

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
    _poll = Timer.periodic(_listPoll, (_) {
      if (mounted && !_busy) _load(silent: true);
    });
  }

  @override
  void dispose() {
    _poll?.cancel();
    super.dispose();
  }

  Future<void> _load({bool silent = false}) async {
    if (!silent) {
      setState(() {
        _busy = true;
        _err = null;
      });
    }
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      final raw = await api.getStationSessions(station.id);
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

  @override
  Widget build(BuildContext context) {
    super.build(context);
    if (_busy && _items == null) {
      return const Center(child: CircularProgressIndicator());
    }
    return RefreshIndicator(
      onRefresh: () => _load(),
      child: _err != null && (_items == null || _items!.isEmpty)
          ? ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              children: [
                const SizedBox(height: 80),
                Center(
                  child: Column(
                    children: [
                      AppIcons.img(AppIcons.stationDisconnect, size: 80),
                      const SizedBox(height: 12),
                      Text('Could not load sessions\n$_err',
                          textAlign: TextAlign.center),
                      const SizedBox(height: 12),
                      FilledButton(
                          onPressed: () => _load(),
                          child: const Text('Retry')),
                    ],
                  ),
                ),
              ],
            )
          : ListView.builder(
              physics: const AlwaysScrollableScrollPhysics(),
              itemCount: (_items?.length ?? 0) + 1,
              itemBuilder: (_, i) {
                if (i == 0) {
                  return Padding(
                    padding: const EdgeInsets.all(12.0),
                    child: Row(
                      children: [
                        AppIcons.img(AppIcons.statistics, size: 28),
                        const SizedBox(width: 8),
                        Text(
                          '${_items?.length ?? 0} session(s) recorded',
                          style: Theme.of(context).textTheme.titleSmall,
                        ),
                      ],
                    ),
                  );
                }
                return _SessionTile(s: _items![i - 1]);
              },
            ),
    );
  }
}

class _SessionTile extends StatelessWidget {
  const _SessionTile({required this.s});
  final Map<String, dynamic> s;

  @override
  Widget build(BuildContext context) {
    final state = (s['latestState'] as String?) ?? '';
    final startedUtc = s['startedUtc'] as String?;
    final endedUtc = s['endedUtc'] as String?;
    final stoppedBy = s['stoppedBy'] as String?;
    final reference = s['reference'] as String?;
    final requestedBy = s['requestedBy'] as String?;
    final isEnded = state == 'Ended' || state == 'Canceled';
    final hue = isEnded ? Colors.grey : Colors.green;
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      child: ExpansionTile(
        leading: AppIcons.img(
            isEnded ? AppIcons.stop : AppIcons.stationSession,
            size: 32),
        title: Text(reference?.isNotEmpty == true ? reference! : state),
        subtitle: Text(_subtitle(startedUtc, endedUtc)),
        trailing: Chip(
            label: Text(state, style: const TextStyle(fontSize: 11)),
            visualDensity: VisualDensity.compact,
            backgroundColor: hue.withAlpha(40)),
        children: [
          Padding(
            padding:
                const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _kv('Session id', s['sessionId']?.toString() ?? ''),
                if (requestedBy != null) _kv('Requested by', requestedBy),
                if (stoppedBy != null) _kv('Stopped by', stoppedBy),
                if (startedUtc != null) _kv('Started UTC', startedUtc),
                if (endedUtc != null) _kv('Ended UTC', endedUtc),
              ],
            ),
          ),
        ],
      ),
    );
  }

  static String _subtitle(String? startedUtc, String? endedUtc) {
    final s = startedUtc != null ? _fmt(startedUtc) : '?';
    if (endedUtc == null) return 'Started $s';
    return '$s → ${_fmt(endedUtc)}';
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

// ============================================================================
// API Keys tab
// ============================================================================

class _ApiKeysTab extends StatefulWidget {
  const _ApiKeysTab();

  @override
  State<_ApiKeysTab> createState() => _ApiKeysTabState();
}

class _ApiKeysTabState extends State<_ApiKeysTab>
    with AutomaticKeepAliveClientMixin {
  List<Map<String, dynamic>>? _items;
  String? _err;
  bool _busy = false;

  /// Holds the freshly-minted secret reveal cards. Server only returns the
  /// `secret` field on the mint call; the list endpoint elides it.
  final List<Map<String, dynamic>> _justMinted = [];

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  Future<void> _load() async {
    setState(() {
      _busy = true;
      _err = null;
    });
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      final raw = await api.listApiKeys(station.id);
      final list =
          raw.whereType<Map<String, dynamic>>().toList(growable: false);
      list.sort((a, b) {
        final ax = a['createOnUtc'] as String? ?? '';
        final bx = b['createOnUtc'] as String? ?? '';
        return bx.compareTo(ax);
      });
      if (!mounted) return;
      setState(() {
        _items = list;
        _busy = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _err = e is ApiException ? e.messages().join('\n') : e.toString();
        _busy = false;
      });
    }
  }

  Future<void> _mint() async {
    final name = await _promptKeyName();
    if (name == null || !mounted) return;
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      final result = await api.mintApiKey(station.id, name);
      if (!mounted) return;
      setState(() {
        _justMinted.insert(0, result);
      });
      _toast(context, 'API key "${result['name']}" minted. Copy the secret now!');
      await _load();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(context, e.messages().join('\n'), isError: true);
    }
  }

  Future<String?> _promptKeyName() async {
    final ctrl = TextEditingController();
    final formKey = GlobalKey<FormState>();
    return showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Mint new API key'),
        content: Form(
          key: formKey,
          child: TextFormField(
            controller: ctrl,
            decoration: const InputDecoration(
              labelText: 'Key name',
              hintText: 'Station01-CoinAcceptor',
              prefixIcon: Icon(Icons.label_outline),
            ),
            validator: (v) {
              if (v == null || v.trim().isEmpty) return 'Required';
              if (v.length > 60) return 'Max 60 chars';
              return null;
            },
            autofocus: true,
          ),
        ),
        actions: [
          TextButton(
              onPressed: () => Navigator.of(ctx).pop(),
              child: const Text('Cancel')),
          FilledButton(
            onPressed: () {
              if (formKey.currentState!.validate()) {
                Navigator.of(ctx).pop(ctrl.text.trim());
              }
            },
            child: const Text('Mint'),
          ),
        ],
      ),
    );
  }

  Future<void> _revoke(Map<String, dynamic> key) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Revoke "${key['name']}"?'),
        content: const Text(
            'Any process using this api key will be locked out. This cannot be undone.'),
        actions: [
          TextButton(
              onPressed: () => Navigator.of(ctx).pop(false),
              child: const Text('Cancel')),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Revoke'),
          ),
        ],
      ),
    );
    if (confirm != true || !mounted) return;
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      await api.deleteApiKey(station.id, key['publicKey'] as String);
      if (!mounted) return;
      _toast(context, 'Revoked.');
      await _load();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(context, e.messages().join('\n'), isError: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return Scaffold(
      body: _busy && _items == null
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _load,
              child: ListView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(12),
                children: [
                  if (_justMinted.isNotEmpty)
                    ..._justMinted.map((k) => _MintedSecretCard(
                          mintedKey: k,
                          onDismiss: () =>
                              setState(() => _justMinted.remove(k)),
                        )),
                  if (_err != null)
                    Padding(
                      padding: const EdgeInsets.all(8.0),
                      child: Text('Could not load keys: $_err',
                          style: const TextStyle(color: Colors.red)),
                    ),
                  if (_items != null && _items!.isEmpty)
                    Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Center(
                        child: Column(
                          children: [
                            AppIcons.img(AppIcons.wrench, size: 60),
                            const SizedBox(height: 12),
                            const Text(
                                'No API keys yet. Mint one with the + button.'),
                          ],
                        ),
                      ),
                    ),
                  ...?_items?.map((k) => _ApiKeyTile(
                        apiKey: k,
                        onRevoke: () => _revoke(k),
                      )),
                ],
              ),
            ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _mint,
        icon: const Icon(Icons.add),
        label: const Text('Mint key'),
      ),
    );
  }
}

class _MintedSecretCard extends StatelessWidget {
  const _MintedSecretCard(
      {required this.mintedKey, required this.onDismiss});
  final Map<String, dynamic> mintedKey;
  final VoidCallback onDismiss;

  @override
  Widget build(BuildContext context) {
    final secret = mintedKey['secret'] as String? ?? '';
    final publicKey = mintedKey['publicKey'] as String? ?? '';
    final name = mintedKey['name'] as String? ?? '';
    return Card(
      color: Colors.amber.shade50,
      shape: RoundedRectangleBorder(
        side: const BorderSide(color: Colors.amber, width: 2),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.warning, color: Colors.orange),
                const SizedBox(width: 8),
                Expanded(
                  child: Text('Copy the secret for "$name" now!',
                      style: const TextStyle(
                          fontWeight: FontWeight.bold)),
                ),
                IconButton(
                    icon: const Icon(Icons.close), onPressed: onDismiss),
              ],
            ),
            const SizedBox(height: 8),
            const Text(
                'The Secret is only shown on creation. After dismissing this card you cannot retrieve it again.'),
            const SizedBox(height: 12),
            _copyRow(context, 'Public key', publicKey),
            _copyRow(context, 'Secret', secret, mono: true),
          ],
        ),
      ),
    );
  }

  Widget _copyRow(BuildContext context, String label, String value,
      {bool mono = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Row(
        children: [
          SizedBox(
              width: 90,
              child:
                  Text(label, style: const TextStyle(color: Colors.black54))),
          Expanded(
            child: SelectableText(
              value,
              style: TextStyle(
                  fontFamily: mono ? 'monospace' : null,
                  fontSize: 13,
                  fontWeight: FontWeight.bold),
            ),
          ),
          IconButton(
            tooltip: 'Copy',
            icon: const Icon(Icons.copy, size: 18),
            onPressed: () async {
              await Clipboard.setData(ClipboardData(text: value));
              if (context.mounted) _toast(context, '$label copied');
            },
          ),
        ],
      ),
    );
  }
}

class _ApiKeyTile extends StatelessWidget {
  const _ApiKeyTile({required this.apiKey, required this.onRevoke});
  final Map<String, dynamic> apiKey;
  final VoidCallback onRevoke;

  @override
  Widget build(BuildContext context) {
    final name = apiKey['name'] as String? ?? '?';
    final pub = apiKey['publicKey'] as String? ?? '';
    final created = apiKey['createOnUtc'] as String?;
    return Card(
      child: ListTile(
        leading: AppIcons.img(AppIcons.server, size: 32),
        title: Text(name),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SelectableText(pub,
                style: const TextStyle(
                    fontFamily: 'monospace', fontSize: 11)),
            if (created != null)
              Text('Created ${_fmt(created)}',
                  style:
                      const TextStyle(fontSize: 11, color: Colors.black54)),
          ],
        ),
        trailing: IconButton(
          tooltip: 'Revoke',
          icon: const Icon(Icons.delete_outline, color: Colors.red),
          onPressed: onRevoke,
        ),
        isThreeLine: true,
      ),
    );
  }

  static String _fmt(String iso) {
    final dt = DateTime.tryParse(iso)?.toLocal();
    if (dt == null) return iso;
    return DateFormat.yMd().add_Hm().format(dt);
  }
}

// ============================================================================
// Settings tab
// ============================================================================

class _SettingsTab extends StatefulWidget {
  const _SettingsTab();

  @override
  State<_SettingsTab> createState() => _SettingsTabState();
}

class _SettingsTabState extends State<_SettingsTab>
    with AutomaticKeepAliveClientMixin {
  Map<String, dynamic>? _settings;
  String? _err;
  bool _busy = false;

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  Future<void> _load() async {
    setState(() {
      _busy = true;
      _err = null;
    });
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      final s = await api.getStationSettings(station.id);
      if (!mounted) return;
      setState(() {
        _settings = s;
        _busy = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _err = e is ApiException ? e.messages().join('\n') : e.toString();
        _busy = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    if (_busy && _settings == null) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_err != null && _settings == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              AppIcons.img(AppIcons.stationDisconnect, size: 60),
              const SizedBox(height: 12),
              Text('Could not load settings\n$_err',
                  textAlign: TextAlign.center),
              const SizedBox(height: 12),
              FilledButton(onPressed: _load, child: const Text('Retry')),
            ],
          ),
        ),
      );
    }
    final s = _settings!;
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(12),
        children: [
          _RenameCard(initialName: s['displayName'] as String? ?? '',
              onSaved: _load),
          const SizedBox(height: 12),
          _ModeCard(
              currentMode: _decodeMode(s['controlMode']),
              onSaved: _load),
          const SizedBox(height: 12),
          _QrCodeCard(
              initialQr: s['qrCode'] as String?, onSaved: _load),
          const SizedBox(height: 12),
          const _PasswordRotationCard(),
        ],
      ),
    );
  }

  static int _decodeMode(Object? raw) {
    if (raw is int) return raw;
    if (raw is String) return StationControlMode.fromString(raw);
    return StationControlMode.local;
  }
}

class _RenameCard extends StatefulWidget {
  const _RenameCard({required this.initialName, required this.onSaved});
  final String initialName;
  final VoidCallback onSaved;
  @override
  State<_RenameCard> createState() => _RenameCardState();
}

class _RenameCardState extends State<_RenameCard> {
  late final TextEditingController _ctrl =
      TextEditingController(text: widget.initialName);
  final _formKey = GlobalKey<FormState>();
  bool _busy = false;

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _busy = true);
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      // Settings endpoint requires {displayName, mode}; we send current mode.
      final mode = StationControlMode.fromString(station.controlMode);
      await api.updateStationSettings(station.id,
          displayName: _ctrl.text.trim(), mode: mode);
      if (!mounted) return;
      _toast(context, 'Renamed.');
      await station.refresh();
      widget.onSaved();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(context, e.messages().join('\n'), isError: true);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  AppIcons.img(AppIcons.gear, size: 28),
                  const SizedBox(width: 8),
                  Text('Display name',
                      style: Theme.of(context).textTheme.titleMedium),
                ],
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _ctrl,
                decoration: const InputDecoration(border: OutlineInputBorder()),
                validator: (v) {
                  if (v == null || v.trim().length < 3) return 'Min 3 chars';
                  if (v.length > 15) return 'Max 15 chars';
                  return null;
                },
              ),
              const SizedBox(height: 12),
              Align(
                alignment: Alignment.centerRight,
                child: FilledButton.icon(
                  onPressed: _busy ? null : _save,
                  icon: _busy
                      ? const SizedBox(
                          width: 16,
                          height: 16,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white))
                      : const Icon(Icons.save),
                  label: const Text('Save name'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ModeCard extends StatefulWidget {
  const _ModeCard({required this.currentMode, required this.onSaved});
  final int currentMode;
  final VoidCallback onSaved;
  @override
  State<_ModeCard> createState() => _ModeCardState();
}

class _ModeCardState extends State<_ModeCard> {
  late int _selected = widget.currentMode;
  bool _busy = false;

  Future<void> _save() async {
    setState(() => _busy = true);
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      await api.updateStationMode(station.id, _selected);
      if (!mounted) return;
      _toast(context, 'Control mode changed.');
      await station.refresh();
      widget.onSaved();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(context, e.messages().join('\n'), isError: true);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                AppIcons.img(AppIcons.controlModeAsset(
                    _selected == StationControlMode.remote
                        ? 'Remote'
                        : _selected == StationControlMode.remoteWithQrCode
                            ? 'RemoteWithQrCode'
                            : 'Local'),
                    size: 28),
                const SizedBox(width: 8),
                Text('Control mode',
                    style: Theme.of(context).textTheme.titleMedium),
              ],
            ),
            const SizedBox(height: 8),
            _radio(StationControlMode.local, 'Local',
                'Operator runs sessions on the kiosk-side dashboard.',
                AppIcons.modeLocal),
            _radio(StationControlMode.remote, 'Remote',
                'Sessions are driven exclusively from this operator console.',
                AppIcons.modeRemote),
            _radio(
                StationControlMode.remoteWithQrCode,
                'Remote + QR code',
                'Customer scans a QR with their phone to initiate sessions.',
                AppIcons.modeQrCode),
            const SizedBox(height: 8),
            Align(
              alignment: Alignment.centerRight,
              child: FilledButton.icon(
                onPressed: _busy ? null : _save,
                icon: _busy
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(
                            strokeWidth: 2, color: Colors.white))
                    : const Icon(Icons.save),
                label: const Text('Save mode'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _radio(int value, String title, String subtitle, String icon) {
    return RadioListTile<int>(
      value: value,
      groupValue: _selected,
      onChanged: (v) => setState(() => _selected = v ?? _selected),
      secondary: AppIcons.img(icon, size: 32),
      title: Text(title),
      subtitle: Text(subtitle),
      dense: true,
      contentPadding: EdgeInsets.zero,
    );
  }
}

class _QrCodeCard extends StatefulWidget {
  const _QrCodeCard({required this.initialQr, required this.onSaved});
  final String? initialQr;
  final VoidCallback onSaved;
  @override
  State<_QrCodeCard> createState() => _QrCodeCardState();
}

class _QrCodeCardState extends State<_QrCodeCard> {
  late final TextEditingController _ctrl =
      TextEditingController(text: widget.initialQr ?? '');
  bool _busy = false;

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() => _busy = true);
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      await api.updateStationQrCode(
          station.id, _ctrl.text.trim().isEmpty ? null : _ctrl.text.trim());
      if (!mounted) return;
      _toast(context, 'QR code updated.');
      widget.onSaved();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(context, e.messages().join('\n'), isError: true);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                AppIcons.img(AppIcons.modeQrCode, size: 28),
                const SizedBox(width: 8),
                Text('QR-code URL',
                    style: Theme.of(context).textTheme.titleMedium),
              ],
            ),
            const SizedBox(height: 8),
            const Text(
                'URL embedded in the customer-facing QR code (used in Remote-with-QR mode).'),
            const SizedBox(height: 12),
            TextField(
              controller: _ctrl,
              decoration: const InputDecoration(
                hintText: 'https://api.example.com/Station/{stationId}/Session...',
                border: OutlineInputBorder(),
              ),
              maxLines: 2,
            ),
            const SizedBox(height: 12),
            Align(
              alignment: Alignment.centerRight,
              child: FilledButton.icon(
                onPressed: _busy ? null : _save,
                icon: _busy
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(
                            strokeWidth: 2, color: Colors.white))
                    : const Icon(Icons.save),
                label: const Text('Save QR'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _PasswordRotationCard extends StatefulWidget {
  const _PasswordRotationCard();
  @override
  State<_PasswordRotationCard> createState() => _PasswordRotationCardState();
}

class _PasswordRotationCardState extends State<_PasswordRotationCard> {
  final _ctrl = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _busy = false;
  bool _obscure = true;

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  Future<void> _rotate() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _busy = true);
    try {
      final station = context.read<Station>();
      final api = context.read<ApiClient>();
      await api.resetStationPassword(station.id, _ctrl.text);
      if (!mounted) return;
      _toast(context, 'Station password rotated. Update the kiosk!');
      _ctrl.clear();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(context, e.messages().join('\n'), isError: true);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      color: Colors.red.shade50,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  AppIcons.img(AppIcons.wrench, size: 28),
                  const SizedBox(width: 8),
                  Text('Rotate station password',
                      style: Theme.of(context).textTheme.titleMedium),
                ],
              ),
              const SizedBox(height: 8),
              const Text(
                  'The kiosk authenticates gRPC calls with the (StationId, Password) pair. Rotating here invalidates the kiosk until you update its config.'),
              const SizedBox(height: 12),
              TextFormField(
                controller: _ctrl,
                obscureText: _obscure,
                decoration: InputDecoration(
                  labelText: 'New password',
                  border: const OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: Icon(
                        _obscure ? Icons.visibility : Icons.visibility_off),
                    onPressed: () => setState(() => _obscure = !_obscure),
                  ),
                ),
                validator: (v) {
                  if (v == null || v.length < 10) {
                    return 'Min 10 chars';
                  }
                  if (v.length > 80) return 'Max 80 chars';
                  return null;
                },
              ),
              const SizedBox(height: 12),
              Align(
                alignment: Alignment.centerRight,
                child: FilledButton.icon(
                  style: FilledButton.styleFrom(
                      backgroundColor: Colors.red),
                  onPressed: _busy ? null : _rotate,
                  icon: _busy
                      ? const SizedBox(
                          width: 16,
                          height: 16,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white))
                      : const Icon(Icons.lock_reset),
                  label: const Text('Rotate'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// ============================================================================
// shared helpers
// ============================================================================

void _toast(BuildContext context, String msg, {bool isError = false}) {
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(msg),
      backgroundColor: isError ? Colors.red : null,
      behavior: SnackBarBehavior.floating,
    ),
  );
}
