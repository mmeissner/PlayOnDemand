import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

// The base URL for the Pod.Web.Center server. When this Flutter app is served
// by the nginx sidecar in docker-compose, the same host serves both the SPA
// and the API (nginx proxies /api/* to the server container). Empty string ->
// relative URLs against the current origin.
const String apiBase = String.fromEnvironment('POD_API_BASE', defaultValue: '');

void main() {
  runApp(const PodOperatorApp());
}

class PodOperatorApp extends StatelessWidget {
  const PodOperatorApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PoD Operator',
      theme: ThemeData(colorSchemeSeed: Colors.lightGreen, useMaterial3: true),
      home: const LoginScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}

// ---------------------------------------------------------------------------
// Login
// ---------------------------------------------------------------------------

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _user = TextEditingController(text: 'superuser');
  final _pass = TextEditingController();
  String? _error;
  bool _busy = false;

  Future<void> _doLogin() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final resp = await http.post(
        Uri.parse('$apiBase/api/v1/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'username': _user.text, 'password': _pass.text}),
      );
      if (resp.statusCode == 200) {
        final body = jsonDecode(resp.body) as Map<String, dynamic>;
        final token = (body['accessToken'] as Map<String, dynamic>)['token'] as String;
        if (!mounted) return;
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(builder: (_) => StationsScreen(token: token, username: _user.text)),
        );
      } else {
        final body = jsonDecode(resp.body) as Map<String, dynamic>;
        setState(() => _error = body.values.expand((v) => v as List).join('; '));
      }
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('PlayOnDemand operator login')),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 360),
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                TextField(
                  controller: _user,
                  decoration: const InputDecoration(labelText: 'Username'),
                  autofocus: true,
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _pass,
                  decoration: const InputDecoration(labelText: 'Password'),
                  obscureText: true,
                  onSubmitted: (_) => _doLogin(),
                ),
                const SizedBox(height: 16),
                if (_error != null)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 12),
                    child: Text(_error!, style: const TextStyle(color: Colors.red)),
                  ),
                FilledButton(
                  onPressed: _busy ? null : _doLogin,
                  child: _busy
                      ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text('Sign in'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// Stations list (Identifies the operator's stations + their current state)
// ---------------------------------------------------------------------------

class StationsScreen extends StatefulWidget {
  final String token;
  final String username;
  const StationsScreen({super.key, required this.token, required this.username});

  @override
  State<StationsScreen> createState() => _StationsScreenState();
}

class _StationsScreenState extends State<StationsScreen> {
  late Future<List<dynamic>> _stations;

  @override
  void initState() {
    super.initState();
    _stations = _load();
  }

  Future<List<dynamic>> _load() async {
    final resp = await http.get(
      Uri.parse('$apiBase/api/v1/Stations'),
      headers: {'Authorization': 'Bearer ${widget.token}'},
    );
    if (resp.statusCode != 200) {
      throw Exception('GET /api/v1/Stations -> ${resp.statusCode} ${resp.body}');
    }
    return jsonDecode(resp.body) as List<dynamic>;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Stations - ${widget.username}'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () => setState(() => _stations = _load()),
          ),
        ],
      ),
      body: FutureBuilder<List<dynamic>>(
        future: _stations,
        builder: (context, snap) {
          if (snap.connectionState != ConnectionState.done) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Padding(
              padding: const EdgeInsets.all(24),
              child: Text(snap.error.toString(), style: const TextStyle(color: Colors.red)),
            );
          }
          final stations = snap.data!;
          if (stations.isEmpty) {
            return const Center(child: Text('No stations yet.'));
          }
          return ListView.separated(
            itemCount: stations.length,
            separatorBuilder: (_, __) => const Divider(height: 1),
            itemBuilder: (context, i) {
              final s = stations[i] as Map<String, dynamic>;
              return ListTile(
                title: Text(s['displayName']?.toString() ?? '(unnamed)'),
                subtitle: Text('id: ${s['stationId']}  mode: ${s['controlMode']}  net: ${s['networkState']}'),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => Navigator.of(context).push(MaterialPageRoute(
                  builder: (_) => StationDetailScreen(token: widget.token, station: s),
                )),
              );
            },
          );
        },
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// Station detail + API key management
// ---------------------------------------------------------------------------

class StationDetailScreen extends StatefulWidget {
  final String token;
  final Map<String, dynamic> station;
  const StationDetailScreen({super.key, required this.token, required this.station});

  @override
  State<StationDetailScreen> createState() => _StationDetailScreenState();
}

class _StationDetailScreenState extends State<StationDetailScreen> {
  late Future<List<dynamic>> _apiKeys;
  String? _justMintedSecret;

  String get _stationId => widget.station['stationId'].toString();

  @override
  void initState() {
    super.initState();
    _apiKeys = _loadKeys();
  }

  Future<List<dynamic>> _loadKeys() async {
    final resp = await http.get(
      Uri.parse('$apiBase/api/v1/Stations/$_stationId/apikeys'),
      headers: {'Authorization': 'Bearer ${widget.token}'},
    );
    if (resp.statusCode != 200) {
      throw Exception('GET apikeys -> ${resp.statusCode} ${resp.body}');
    }
    return jsonDecode(resp.body) as List<dynamic>;
  }

  Future<void> _mintKey(String keyName) async {
    final resp = await http.put(
      Uri.parse('$apiBase/api/v1/Stations/$_stationId/apikeys?keyName=$keyName'),
      headers: {'Authorization': 'Bearer ${widget.token}'},
    );
    if (resp.statusCode != 200) {
      throw Exception('PUT apikeys -> ${resp.statusCode} ${resp.body}');
    }
    final body = jsonDecode(resp.body) as Map<String, dynamic>;
    setState(() {
      _justMintedSecret = body['secret']?.toString();
      _apiKeys = _loadKeys();
    });
  }

  Future<void> _promptMint() async {
    final controller = TextEditingController();
    final name = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('New API key'),
        content: TextField(
          controller: controller,
          autofocus: true,
          decoration: const InputDecoration(labelText: 'Name (e.g. booth-01-primary)'),
          onSubmitted: (v) => Navigator.of(ctx).pop(v),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.of(ctx).pop(), child: const Text('Cancel')),
          FilledButton(
            onPressed: () => Navigator.of(ctx).pop(controller.text),
            child: const Text('Mint'),
          ),
        ],
      ),
    );
    if (name == null || name.trim().isEmpty) return;
    try {
      await _mintKey(name.trim());
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = widget.station;
    return Scaffold(
      appBar: AppBar(title: Text(s['displayName']?.toString() ?? 'Station')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _kv('Station ID', _stationId),
          _kv('Display name', s['displayName']?.toString() ?? ''),
          _kv('Control mode', s['controlMode']?.toString() ?? ''),
          _kv('Network state', s['networkState']?.toString() ?? ''),
          const Divider(height: 32),
          Row(
            children: [
              Expanded(child: Text('API keys', style: Theme.of(context).textTheme.titleMedium)),
              FilledButton.icon(
                icon: const Icon(Icons.add),
                label: const Text('Mint'),
                onPressed: _promptMint,
              ),
            ],
          ),
          if (_justMintedSecret != null)
            Card(
              color: Colors.amber.shade100,
              margin: const EdgeInsets.symmetric(vertical: 12),
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Save this Secret NOW.', style: TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(height: 4),
                    const Text('This is the only time the server will reveal it.'),
                    const SizedBox(height: 8),
                    SelectableText(
                      _justMintedSecret!,
                      style: const TextStyle(fontFamily: 'monospace', fontSize: 14),
                    ),
                  ],
                ),
              ),
            ),
          FutureBuilder<List<dynamic>>(
            future: _apiKeys,
            builder: (context, snap) {
              if (snap.connectionState != ConnectionState.done) {
                return const Padding(
                  padding: EdgeInsets.symmetric(vertical: 24),
                  child: Center(child: CircularProgressIndicator()),
                );
              }
              if (snap.hasError) {
                return Padding(
                  padding: const EdgeInsets.all(12),
                  child: Text(snap.error.toString(), style: const TextStyle(color: Colors.red)),
                );
              }
              final keys = snap.data!;
              if (keys.isEmpty) {
                return const Padding(
                  padding: EdgeInsets.all(24),
                  child: Text('No keys yet. Mint one to provision a kiosk.', textAlign: TextAlign.center),
                );
              }
              return Column(
                children: keys.map((k) {
                  final m = k as Map<String, dynamic>;
                  return ListTile(
                    leading: const Icon(Icons.key),
                    title: Text(m['name']?.toString() ?? ''),
                    subtitle: Text('public: ${m['publicKey']}\ncreated: ${m['createOnUtc']}'),
                    isThreeLine: true,
                  );
                }).toList(),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _kv(String k, String v) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(width: 120, child: Text(k, style: const TextStyle(fontWeight: FontWeight.bold))),
          Expanded(child: SelectableText(v)),
        ],
      ),
    );
  }
}
