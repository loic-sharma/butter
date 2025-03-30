# Butter

### Local testing

Run the example app using the Butter tool:

```
cp example
dart run ../bin/butter.dart run -d butter
```

### TODO:

1. Add entrypoint scripts
1. Tool and Flutter SDK can be out of sync.
1. Clean should remove obj/bin/ephemeral folders
1. Create template
1. TODO: Test that templates copied from Flutter are up-todate. Ensure not missing new templates.
1. Support x64 and arm64
1. Plugins
  1. Dart plugins
  1. C++ plugins
  1. Plugin registration
  1. Messaging
  1. Pigeon
1. Figure out if template works if Butter supports multiple target platforms
   1. Maybe `Butter.Windows` should be `Butter` and `Butter.Windows.Bindings` should be `Butter.Windows`?
1. Local engine

### Useful resources

1. https://github.com/LiveOrNot/FlutterSharp/blob/8b24bdf14465c090b53ecc04c0c2c2598ae7aff3/FlutterSharp/Integrations/FlutterInterop.cs
2. https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
3. https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
4. https://github.com/sony/flutter-elinux

### Running assemble manually

In the `example/` dir, run:

```ps1
dart-stable run ../bin/butter.dart `
  --verbose `
  assemble `
  --no-version-check `
  --output=build `
  -dTargetPlatform=windows-x64 `
  -dBuildMode=debug `
  -dTargetFile="lib/main.dart" `
  debug_bundle_butter_assets
```
