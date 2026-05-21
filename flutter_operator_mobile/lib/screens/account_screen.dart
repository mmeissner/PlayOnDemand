import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/identity.dart';
import '../services/api_client.dart';
import '../widgets/app_icons.dart';

class AccountScreen extends StatefulWidget {
  const AccountScreen({super.key});

  @override
  State<AccountScreen> createState() => _AccountScreenState();
}

class _AccountScreenState extends State<AccountScreen> {
  final _currentCtrl = TextEditingController();
  final _newCtrl = TextEditingController();
  final _confirmCtrl = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _busy = false;
  bool _obscureCurrent = true;
  bool _obscureNew = true;

  @override
  void dispose() {
    _currentCtrl.dispose();
    _newCtrl.dispose();
    _confirmCtrl.dispose();
    super.dispose();
  }

  Future<void> _change() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _busy = true);
    try {
      final api = context.read<ApiClient>();
      await api.changeAccountPassword(_currentCtrl.text, _newCtrl.text);
      if (!mounted) return;
      _toast('Password changed. You stay signed in on this device.');
      _currentCtrl.clear();
      _newCtrl.clear();
      _confirmCtrl.clear();
    } on ApiException catch (e) {
      if (!mounted) return;
      _toast(e.messages().join('\n'), isError: true);
    } finally {
      if (mounted) setState(() => _busy = false);
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
    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            AppIcons.img(AppIcons.menuGear, size: 22),
            const SizedBox(width: 8),
            const Text('Account'),
          ],
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: ListTile(
              leading: AppIcons.img(AppIcons.menuGear, size: 36),
              title: const Text('Signed in as'),
              subtitle: Text(identity.username ?? '?'),
            ),
          ),
          const SizedBox(height: 12),
          Card(
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
                        Text('Change password',
                            style:
                                Theme.of(context).textTheme.titleMedium),
                      ],
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _currentCtrl,
                      obscureText: _obscureCurrent,
                      decoration: InputDecoration(
                        labelText: 'Current password',
                        border: const OutlineInputBorder(),
                        suffixIcon: IconButton(
                          icon: Icon(_obscureCurrent
                              ? Icons.visibility
                              : Icons.visibility_off),
                          onPressed: () => setState(
                              () => _obscureCurrent = !_obscureCurrent),
                        ),
                      ),
                      validator: (v) => (v == null || v.isEmpty)
                          ? 'Required'
                          : null,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _newCtrl,
                      obscureText: _obscureNew,
                      decoration: InputDecoration(
                        labelText: 'New password',
                        border: const OutlineInputBorder(),
                        suffixIcon: IconButton(
                          icon: Icon(_obscureNew
                              ? Icons.visibility
                              : Icons.visibility_off),
                          onPressed: () =>
                              setState(() => _obscureNew = !_obscureNew),
                        ),
                      ),
                      validator: (v) {
                        if (v == null || v.length < 10) return 'Min 10 chars';
                        if (v.length > 80) return 'Max 80 chars';
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _confirmCtrl,
                      obscureText: _obscureNew,
                      decoration: const InputDecoration(
                        labelText: 'Confirm new password',
                        border: OutlineInputBorder(),
                      ),
                      validator: (v) => v != _newCtrl.text
                          ? "Passwords don't match"
                          : null,
                    ),
                    const SizedBox(height: 16),
                    Align(
                      alignment: Alignment.centerRight,
                      child: FilledButton.icon(
                        onPressed: _busy ? null : _change,
                        icon: _busy
                            ? const SizedBox(
                                width: 16,
                                height: 16,
                                child: CircularProgressIndicator(
                                    strokeWidth: 2, color: Colors.white))
                            : const Icon(Icons.lock_reset),
                        label: const Text('Change password'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
          const SizedBox(height: 12),
          const _AboutCard(),
        ],
      ),
    );
  }
}

class _AboutCard extends StatelessWidget {
  const _AboutCard();

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
                AppIcons.img(AppIcons.support, size: 28),
                const SizedBox(width: 8),
                Text('About',
                    style: Theme.of(context).textTheme.titleMedium),
              ],
            ),
            const SizedBox(height: 8),
            const Text('PoD Operator Mobile · v1.0.0'),
            const SizedBox(height: 4),
            const Text(
                'Modern Dart 3 port of the legacy leap_play_x_app. Manages PoD station sessions and credentials.'),
            const SizedBox(height: 8),
            Center(
                child: Image.asset(AppIcons.logo,
                    width: 180, fit: BoxFit.contain)),
          ],
        ),
      ),
    );
  }
}
