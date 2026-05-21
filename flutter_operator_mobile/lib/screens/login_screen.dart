import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/identity.dart';
import '../widgets/app_icons.dart';
import 'stations_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _userCtl = TextEditingController();
  final _passCtl = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _userCtl.dispose();
    _passCtl.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    final identity = context.read<IdentityProvider>();
    final result = await identity.login(_userCtl.text, _passCtl.text);
    if (!mounted) return;
    if (result.isSuccess) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const StationsScreen()),
      );
    } else {
      setState(() {
        _error = result.errors.join('\n');
        _busy = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          Positioned.fill(
            child: Image.asset(
              AppIcons.mountainsBackdrop,
              fit: BoxFit.cover,
            ),
          ),
          Container(color: Colors.black.withAlpha(120)),
          SafeArea(
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 420),
                  child: Card(
                    elevation: 8,
                    child: Padding(
                      padding: const EdgeInsets.all(24),
                      child: Form(
                        key: _formKey,
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Image.asset(
                              AppIcons.logo,
                              width: 220,
                            ),
                            const SizedBox(height: 16),
                            Text(
                              'Operator sign-in',
                              style:
                                  Theme.of(context).textTheme.titleMedium,
                            ),
                            const SizedBox(height: 16),
                            TextFormField(
                              controller: _userCtl,
                              decoration: const InputDecoration(
                                labelText: 'Username',
                                prefixIcon: Icon(Icons.person),
                              ),
                              textInputAction: TextInputAction.next,
                              autofillHints: const [AutofillHints.username],
                              validator: (v) =>
                                  (v == null || v.trim().isEmpty)
                                      ? 'Required'
                                      : null,
                            ),
                            const SizedBox(height: 12),
                            TextFormField(
                              controller: _passCtl,
                              obscureText: true,
                              decoration: const InputDecoration(
                                labelText: 'Password',
                                prefixIcon: Icon(Icons.lock),
                              ),
                              textInputAction: TextInputAction.done,
                              autofillHints: const [AutofillHints.password],
                              onFieldSubmitted: (_) =>
                                  _busy ? null : _submit(),
                              validator: (v) =>
                                  (v == null || v.isEmpty) ? 'Required' : null,
                            ),
                            if (_error != null) ...[
                              const SizedBox(height: 12),
                              Text(
                                _error!,
                                style: const TextStyle(color: Colors.red),
                              ),
                            ],
                            const SizedBox(height: 20),
                            SizedBox(
                              width: double.infinity,
                              child: FilledButton.icon(
                                icon: _busy
                                    ? const SizedBox(
                                        width: 18,
                                        height: 18,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                          color: Colors.white,
                                        ),
                                      )
                                    : const Icon(Icons.login),
                                label: Text(_busy ? 'Signing in…' : 'Sign in'),
                                onPressed: _busy ? null : _submit,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
