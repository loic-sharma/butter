import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Method Channel Demo',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: const MyHomePage(title: 'Method Channel Demo'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title});

  final String title;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  static const MethodChannel channel = MethodChannel('butter.method_channel');

  String _response = 'No response yet.';
  final TextEditingController _messageController = TextEditingController();
  final GlobalKey<ScaffoldMessengerState> _scaffoldMessengerKey = GlobalKey<ScaffoldMessengerState>();

  Future<void> _sendMessage() async {
    final String message = _messageController.text;
    if (message.isEmpty) {
      setState(() => _response = 'Please enter a message.');
      return;
    }

    try {
      final String? reply = await channel.invokeMethod(
        'sendMessage',
        <String, dynamic> { 'message': message },
      );
      if (!mounted) {
        return;
      }

      setState(() => _response = reply ?? 'Null.');
    } on PlatformException catch (e) {
      setState(() => _response = 'Exception: ${e.message}');
    }
  }

  @override
  void initState() {
    super.initState();

    // Respond to messages send from native.
    channel.setMethodCallHandler((MethodCall call) async {
      switch (call.method) {
        case 'showSnackBar':
          final String message = call.arguments as String;
          if (!mounted) {
            return false;
          }
          _scaffoldMessengerKey.currentState?.showSnackBar(
            SnackBar(
              content: Text('Message from Native: $message'),
              duration: const Duration(seconds: 2),
            ),
          );
          return true;
        default:
          throw MissingPluginException('Method not implemented: ${call.method}');
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return ScaffoldMessenger(
      key: _scaffoldMessengerKey,
      child: Scaffold(
        appBar: AppBar(
          title: Text(widget.title),
        ),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: <Widget>[
                TextField(
                  controller: _messageController,
                  decoration: const InputDecoration(
                    labelText: 'Enter message to native',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 20),
                ElevatedButton(
                  onPressed: _sendMessage,
                  child: const Text('Send to Native'),
                ),
                const SizedBox(height: 20),
                Text(
                  'Response from Native:',
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                const SizedBox(height: 10),
                Text(
                  _response,
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
