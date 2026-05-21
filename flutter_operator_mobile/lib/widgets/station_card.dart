import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/station.dart';
import '../screens/station_detail_screen.dart';
import 'app_icons.dart';

/// Per-station card on the stations-list screen. Tap → station detail tabs.
/// The buttons here are kept short - the per-tab "Overview" surfaces the
/// same actions with bigger controls, so this is just a quick-action surface.
class StationCard extends StatelessWidget {
  const StationCard({super.key});

  @override
  Widget build(BuildContext context) {
    final station = context.watch<Station>();
    return Card(
      elevation: 3,
      margin: const EdgeInsets.symmetric(vertical: 6, horizontal: 12),
      child: InkWell(
        onTap: () => Navigator.of(context).push(
          MaterialPageRoute(
            builder: (_) => ChangeNotifierProvider<Station>.value(
              value: station,
              child: const StationDetailScreen(),
            ),
          ),
        ),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  AppIcons.img(
                      AppIcons.stationStateAsset(
                          isConnected: station.isConnected,
                          hasSession: station.hasSession),
                      size: 44),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(station.name,
                            style: Theme.of(context).textTheme.titleLarge),
                        const SizedBox(height: 2),
                        Row(
                          children: [
                            AppIcons.img(
                                AppIcons.controlModeAsset(
                                    station.controlMode),
                                size: 18),
                            const SizedBox(width: 4),
                            Text(station.controlMode,
                                style: const TextStyle(
                                    fontSize: 12, color: Colors.black54)),
                          ],
                        ),
                      ],
                    ),
                  ),
                  if (station.session != null)
                    Chip(
                      label: Text(station.session!.state,
                          style: const TextStyle(fontSize: 11)),
                      backgroundColor: Colors.lightGreen.shade100,
                      visualDensity: VisualDensity.compact,
                    ),
                ],
              ),
              const SizedBox(height: 6),
              if (station.isBusy)
                const Padding(
                  padding: EdgeInsets.all(8.0),
                  child: Center(
                      child: SizedBox(
                          width: 24,
                          height: 24,
                          child:
                              CircularProgressIndicator(strokeWidth: 2))),
                )
              else
                _quickActions(context, station),
            ],
          ),
        ),
      ),
    );
  }

  Widget _quickActions(BuildContext context, Station station) {
    final disconnected = !station.isConnected;
    if (disconnected) {
      return const Row(
        children: [
          SizedBox(width: 4),
          Icon(Icons.cloud_off, size: 14, color: Colors.grey),
          SizedBox(width: 4),
          Text('Offline', style: TextStyle(fontSize: 12)),
        ],
      );
    }
    final hasSession = station.session != null;
    // Single row: action icons on the left, Details button on the right.
    // (Earlier had Wrap + Spacer, but Spacer isn't a Flex parent so Wrap
    // bounced Details to a second line, leaving empty space under the card.)
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (!hasSession)
              _btn(
                tooltip: 'Start session',
                asset: AppIcons.play,
                onTap: () => _start(context),
              )
            else ...[
              _btn(
                tooltip: 'Extend session',
                asset: AppIcons.updateSession,
                onTap: () => _extend(context),
              ),
              _btn(
                tooltip: 'Stop session',
                asset: AppIcons.stop,
                onTap: () => _stop(context),
              ),
            ],
          ],
        ),
        TextButton.icon(
          icon: AppIcons.img(AppIcons.gear, size: 16),
          label: const Text('Details'),
          onPressed: () => Navigator.of(context).push(
            MaterialPageRoute(
              builder: (_) => ChangeNotifierProvider<Station>.value(
                value: station,
                child: const StationDetailScreen(),
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _btn({
    required String tooltip,
    required String asset,
    required VoidCallback onTap,
  }) {
    return IconButton(
      tooltip: tooltip,
      icon: AppIcons.img(asset, size: 28),
      visualDensity: VisualDensity.compact,
      onPressed: onTap,
    );
  }

  Future<void> _start(BuildContext context) async {
    final duration = await _pickDuration(context, 'Start session for');
    if (duration == null) return;
    if (!context.mounted) return;
    final err =
        await context.read<Station>().tryStartSession(duration: duration);
    if (err != null && context.mounted) _err(context, err);
  }

  Future<void> _extend(BuildContext context) async {
    final duration = await _pickDuration(context, 'Extend session by');
    if (duration == null) return;
    if (!context.mounted) return;
    final err =
        await context.read<Station>().tryUpdateSession(duration: duration);
    if (err != null && context.mounted) _err(context, err);
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
    if (confirm != true || !context.mounted) return;
    final err = await context.read<Station>().tryStopSession();
    if (err != null && context.mounted) _err(context, err);
  }

  Future<Duration?> _pickDuration(BuildContext context, String title) async {
    return showDialog<Duration>(
      context: context,
      builder: (ctx) => SimpleDialog(
        title: Text(title),
        children: [
          for (final m in const [15, 30, 60, 120, 240])
            SimpleDialogOption(
              onPressed: () =>
                  Navigator.of(ctx).pop(Duration(minutes: m)),
              child: Text(_minLabel(m)),
            ),
        ],
      ),
    );
  }

  static String _minLabel(int m) {
    if (m >= 60) return '${m ~/ 60} hour${m == 60 ? "" : "s"}';
    return '$m minutes';
  }

  void _err(BuildContext context, String msg) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Text(msg),
      backgroundColor: Colors.red,
    ));
  }
}
