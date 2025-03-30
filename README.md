# Butter

.NET bindings to Flutter.

### Local testing

Run the example app using the Butter tool:

```
cp example
dart run ../bin/butter.dart run -d butter
```

### TODO:

Initial:
1. Tool and Flutter SDK can be out of sync. Make butter use its own Flutter tool
1. Clean should remove obj/bin/ephemeral folders
1. Plugins
    1. Dart plugins
    1. C++ plugins
    1. Plugin registration
    1. Messaging
1. Update README
1. Docs using https://docs.page
    1. Get started
        1. How to install VS Code + .NET
    1. Changelog
    1. How to create a plugin
    1. How to find pub.dev plugins
    1. How to debug C# code


After:
1. Doctor command
1. Upgrade command
1. Add-to-app
    1. Win32
    1. WPF
    1. WinForms
1. Pigeon
1. TODO: Test that templates copied from Flutter are up-to-date. Ensure not missing new templates.
1. Local engine
1. Support x64 and arm64
1. Docs
    1. Add-to-app
    1. How to bind to native code
    1. 
    1. How to deploy Butter app

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
