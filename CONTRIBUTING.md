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
