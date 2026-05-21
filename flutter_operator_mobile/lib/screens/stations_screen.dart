import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/identity.dart';
import '../providers/station.dart';
import '../providers/stations.dart';
import '../services/api_client.dart';
import '../widgets/app_icons.dart';
import '../widgets/station_card.dart';
import 'account_screen.dart';
import 'all_sessions_screen.dart';
import 'login_screen.dart';

/// How often the stations list polls the server while this screen is visible.
/// Lightweight `GET /api/v1/stations` call.
const _pollEvery = Duration(seconds: 5);

class StationsScreen extends StatefulWidget {
  const StationsScreen({super.key});

  @override
  State<StationsScreen> createState() => _StationsScreenState();
}

class _StationsScreenState extends State<StationsScreen>
    with WidgetsBindingObserver {
  Timer? _poll;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<StationsProvider>().refresh();
      _startPolling();
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _poll?.cancel();
    super.dispose();
  }

  /// Auto-pause polling when the tab is backgrounded so we don't burn battery
  /// or pile up requests when the operator isn't looking.
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
    _poll = Timer.periodic(_pollEvery, (_) {
      if (!mounted) return;
      final p = context.read<StationsProvider>();
      if (!p.isLoading) p.refresh();
    });
  }

  Future<void> _logout() async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Sign out?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.of(ctx).pop(false),
              child: const Text('Cancel')),
          FilledButton(
              onPressed: () => Navigator.of(ctx).pop(true),
              child: const Text('Sign out')),
        ],
      ),
    );
    if (ok != true || !mounted) return;
    await context.read<IdentityProvider>().logout();
    context.read<StationsProvider>().clear();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (_) => false,
    );
  }

  Future<void> _createStation() async {
    final result = await showDialog<Map<String, String>>(
      context: context,
      builder: (ctx) => const _CreateStationDialog(),
    );
    if (result == null || !mounted) return;
    try {
      final api = context.read<ApiClient>();
      final st = await api.createStation(
          displayName: result['displayName']!, password: result['password']!);
      if (!mounted) return;
      _toast(
          'Station "${st['displayName']}" created. ID: ${st['stationId']}');
      await context.read<StationsProvider>().refresh();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(e.messages().join('\n'), isError: true);
    }
  }

  void _toast(String msg, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Text(msg),
      backgroundColor: isError ? Colors.red : null,
    ));
  }

  @override
  Widget build(BuildContext context) {
    final identity = context.watch<IdentityProvider>();
    final stations = context.watch<StationsProvider>();
    final connectedCount = stations.stations.where((s) => s.isConnected).length;
    final activeSessionCount =
        stations.stations.where((s) => s.hasSession).length;
    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            AppIcons.img(AppIcons.station, size: 24),
            const SizedBox(width: 8),
            const Text('Stations'),
          ],
        ),
        bottom: stations.isLoading
            ? const PreferredSize(
                preferredSize: Size.fromHeight(3),
                child: LinearProgressIndicator(minHeight: 3),
              )
            : null,
      ),
      drawer: _AppDrawer(
        username: identity.username,
        onLogout: _logout,
      ),
      body: Column(
        children: [
          _StatBar(
            total: stations.stations.length,
            connected: connectedCount,
            active: activeSessionCount,
          ),
          Expanded(
            child: RefreshIndicator(
              onRefresh: stations.refresh,
              child: _list(stations),
            ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _createStation,
        icon: AppIcons.img(AppIcons.station, size: 22),
        label: const Text('New station'),
      ),
    );
  }

  Widget _list(StationsProvider stations) {
    if (stations.isLoading && !stations.hasLoadedOnce) {
      return const Center(child: CircularProgressIndicator());
    }
    if (stations.error != null && stations.stations.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: [
          const SizedBox(height: 80),
          Center(
            child: Column(
              children: [
                AppIcons.img(AppIcons.stationDisconnect, size: 80),
                const SizedBox(height: 12),
                Text('Could not load stations\n${stations.error}',
                    textAlign: TextAlign.center),
                const SizedBox(height: 12),
                FilledButton(
                    onPressed: stations.refresh,
                    child: const Text('Retry')),
              ],
            ),
          ),
        ],
      );
    }
    if (stations.stations.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: [
          const SizedBox(height: 80),
          Center(
            child: Column(
              children: [
                AppIcons.img(AppIcons.station, size: 96),
                const SizedBox(height: 12),
                const Text(
                    'No stations yet.\nTap "New station" to add the first one.',
                    textAlign: TextAlign.center),
              ],
            ),
          ),
        ],
      );
    }
    return ListView.builder(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.only(bottom: 80),
      itemCount: stations.stations.length,
      itemBuilder: (_, i) {
        final station = stations.stations.elementAt(i);
        return ChangeNotifierProvider<Station>.value(
          value: station,
          child: const StationCard(),
        );
      },
    );
  }
}

class _StatBar extends StatelessWidget {
  const _StatBar(
      {required this.total, required this.connected, required this.active});
  final int total;
  final int connected;
  final int active;
  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      color: Colors.lightGreen.shade50,
      child: Row(
        children: [
          Expanded(
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  _chip(AppIcons.station, '$total total'),
                  const SizedBox(width: 8),
                  _chip(AppIcons.stationIdle, '$connected online',
                      tint: Colors.green),
                  const SizedBox(width: 8),
                  _chip(AppIcons.stationSession, '$active in session',
                      tint: Colors.deepOrange),
                ],
              ),
            ),
          ),
          const SizedBox(width: 8),
          const _LiveBadge(),
        ],
      ),
    );
  }

  Widget _chip(String asset, String label, {Color? tint}) {
    return Chip(
      avatar: AppIcons.img(asset, size: 20),
      label: Text(label,
          style:
              TextStyle(color: tint, fontWeight: FontWeight.w600, fontSize: 12)),
      backgroundColor: Colors.white,
      visualDensity: VisualDensity.compact,
    );
  }
}

class _AppDrawer extends StatelessWidget {
  const _AppDrawer({required this.username, required this.onLogout});
  final String? username;
  final VoidCallback onLogout;

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            DrawerHeader(
              decoration:
                  BoxDecoration(color: Colors.lightGreen.shade50),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Image.asset(AppIcons.logo, height: 50),
                  if (username != null)
                    Row(
                      children: [
                        AppIcons.img(AppIcons.menuGear, size: 18),
                        const SizedBox(width: 4),
                        Text(username!,
                            style: const TextStyle(fontSize: 14)),
                      ],
                    ),
                ],
              ),
            ),
            _item(context,
                asset: AppIcons.station,
                label: 'Stations',
                isActive: true,
                onTap: () => Navigator.pop(context)),
            _item(context,
                asset: AppIcons.statistics,
                label: 'All sessions',
                onTap: () {
              Navigator.pop(context);
              Navigator.of(context).push(MaterialPageRoute(
                  builder: (_) => const AllSessionsScreen()));
            }),
            _item(context,
                asset: AppIcons.menuGear,
                label: 'Account',
                onTap: () {
              Navigator.pop(context);
              Navigator.of(context).push(MaterialPageRoute(
                  builder: (_) => const AccountScreen()));
            }),
            const Divider(),
            _item(context,
                asset: AppIcons.support,
                label: 'Support',
                onTap: () {
              Navigator.pop(context);
              showAboutDialog(
                context: context,
                applicationName: 'PoD Operator Mobile',
                applicationVersion: '1.0.0',
                applicationIcon: Image.asset(AppIcons.logo, width: 100),
                children: const [
                  Text(
                      'Daily-ops operator app for PlayOnDemand. Manages station sessions, API keys, and station settings via the REST surface of Pod.Web.Center.'),
                ],
              );
            }),
            const Spacer(),
            const Divider(),
            ListTile(
              leading: AppIcons.img(AppIcons.logout, size: 28),
              title: const Text('Sign out'),
              onTap: () {
                Navigator.pop(context);
                onLogout();
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _item(BuildContext context,
      {required String asset,
      required String label,
      required VoidCallback onTap,
      bool isActive = false}) {
    return ListTile(
      leading: AppIcons.img(asset, size: 28),
      title: Text(label,
          style: TextStyle(
              fontWeight: isActive ? FontWeight.bold : FontWeight.normal)),
      selected: isActive,
      onTap: onTap,
    );
  }
}

/// Tiny "Live · 5s" pulse to show the screen is auto-refreshing.
class _LiveBadge extends StatefulWidget {
  const _LiveBadge();
  @override
  State<_LiveBadge> createState() => _LiveBadgeState();
}

class _LiveBadgeState extends State<_LiveBadge>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl;

  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
        vsync: this, duration: const Duration(seconds: 1))
      ..repeat(reverse: true);
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        FadeTransition(
          opacity: _ctrl,
          child: Container(
            width: 8,
            height: 8,
            decoration: const BoxDecoration(
                color: Colors.redAccent, shape: BoxShape.circle),
          ),
        ),
        const SizedBox(width: 4),
        Text('Live · ${_pollEvery.inSeconds}s',
            style:
                const TextStyle(fontSize: 11, color: Colors.black54)),
      ],
    );
  }
}

/// Dialog to create a new station. Server requires displayName + password.
class _CreateStationDialog extends StatefulWidget {
  const _CreateStationDialog();

  @override
  State<_CreateStationDialog> createState() => _CreateStationDialogState();
}

class _CreateStationDialogState extends State<_CreateStationDialog> {
  final _nameCtrl = TextEditingController();
  final _passCtrl = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _obscure = true;

  @override
  void dispose() {
    _nameCtrl.dispose();
    _passCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Row(
        children: [
          AppIcons.img(AppIcons.station, size: 28),
          const SizedBox(width: 8),
          const Text('New station'),
        ],
      ),
      content: SizedBox(
        width: 380,
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: _nameCtrl,
                decoration: const InputDecoration(
                  labelText: 'Display name',
                  hintText: 'Booth-North-1',
                  border: OutlineInputBorder(),
                ),
                validator: (v) {
                  if (v == null || v.trim().length < 3) return 'Min 3 chars';
                  if (v.length > 30) return 'Max 30 chars';
                  return null;
                },
                autofocus: true,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _passCtrl,
                obscureText: _obscure,
                decoration: InputDecoration(
                  labelText: 'Station password',
                  helperText:
                      'Used by the kiosk to authenticate via gRPC metadata.',
                  border: const OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: Icon(
                        _obscure ? Icons.visibility : Icons.visibility_off),
                    onPressed: () => setState(() => _obscure = !_obscure),
                  ),
                ),
                validator: (v) {
                  if (v == null || v.length < 10) return 'Min 10 chars';
                  if (v.length > 80) return 'Max 80 chars';
                  return null;
                },
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel')),
        FilledButton.icon(
          onPressed: () {
            if (_formKey.currentState!.validate()) {
              Navigator.of(context).pop({
                'displayName': _nameCtrl.text.trim(),
                'password': _passCtrl.text,
              });
            }
          },
          icon: const Icon(Icons.add),
          label: const Text('Create'),
        ),
      ],
    );
  }
}

