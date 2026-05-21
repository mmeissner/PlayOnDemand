import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/identity.dart';
import '../widgets/app_icons.dart';
import 'login_screen.dart';
import 'stations_screen.dart';

/// First screen after boot. Asks the IdentityProvider whether it can revive a
/// stored session, then hands off to either the stations list (authenticated)
/// or the login screen.
class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _resolve());
  }

  Future<void> _resolve() async {
    final identity = context.read<IdentityProvider>();
    final ok = await identity.tryAutoLogin();
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) => ok ? const StationsScreen() : const LoginScreen(),
      ),
    );
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
          Container(color: Colors.black.withAlpha(80)),
          Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Image.asset(
                  AppIcons.logo,
                  width: 260,
                ),
                const SizedBox(height: 24),
                const CircularProgressIndicator(),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
