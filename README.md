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
1. Clean should remove obj/bin/ephemeral folders
1. Plugins
    1. Messaging
        1. Encodable value
        1. Standard method codec
        1. Standard codec
        1. Method call / result
    1. Plugin template
    1. Plugin registration
    1. Messaging
        1. Standard message codec
    1. Dart plugins
    1. C++ plugins
1. Update README
1. Docs using https://docs.page
    1. Get started
        1. How to install VS Code + .NET
    1. Changelog
    1. How to create a plugin
    1. How to find pub.dev plugins
    1. How to debug C# code
    1. CLI [tab](https://use.docs.page/navigation#tab-navigation)
        1. Build command
        1. Create command
        1. Etc...
1. Clean up Butter.Windows.csproj. Move stuff to ephemeral
1. Do a pass on runner app. Make sure it supports everything Flutter Windows does.
1. Do a pass on TODOs.
1. Check .NET public APIs. Make stuff internal. Add comments.

After:
1. Doctor command
1. Upgrade command
1. Add-to-app
    1. Win32
    1. WPF
    1. WinForms
1. Pigeon
1. Tests
    1. Integration tests
    1. Test that templates copied from Flutter are up-to-date. Ensure not missing new templates.
1. Local engine
1. Support x64 and arm64
1. Docs
    1. Add-to-app
    1. How to bind to native code
    1. How to deploy Butter app
    1. .NET API reference
1. NativeAOT
1. Games: Godot and Unity

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
