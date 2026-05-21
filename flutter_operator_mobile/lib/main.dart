import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'providers/identity.dart';
import 'providers/stations.dart';
import 'screens/splash_screen.dart';
import 'services/api_client.dart';

void main() => runApp(const PodOperatorMobileApp());

class PodOperatorMobileApp extends StatelessWidget {
  const PodOperatorMobileApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        // Single shared HTTP client. IdentityProvider sets its token; the
        // StationsProvider and per-station Station widgets read through it
        // for every authenticated call.
        Provider<ApiClient>(create: (_) => ApiClient()),
        ChangeNotifierProvider<IdentityProvider>(
          create: (ctx) => IdentityProvider(ctx.read<ApiClient>()),
        ),
        ChangeNotifierProvider<StationsProvider>(
          create: (ctx) => StationsProvider(ctx.read<ApiClient>()),
        ),
      ],
      child: MaterialApp(
        title: 'PoD Operator Mobile',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorSchemeSeed: Colors.lightGreen,
          useMaterial3: true,
        ),
        home: const SplashScreen(),
      ),
    );
  }
}
