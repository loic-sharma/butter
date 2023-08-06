# Butter

### Local testing

1. Run the example app using the Butter tool:

   ```
   cp example
   dart run ../bin/butter.dart run -d butter
   ```


1. Build engine
2. Copy the locally built engine to the `example/butter/Flutter/ephemeral` directory:

   ```
   cp C:\Code\f\engine\src\out\host_debug_unopt\flutter_windows.dll C:\Code\butter\example\butter\Flutter\ephemeral
   ```

3. Run the example app once using the local engine:

   ```
   cd example
   flutter run -d windows --local-engine host_debug_unopt
   ```

4. Now run the example app using Butter:

   ```
   dotnet run --project butter/Runner
   ```

### TODO:

1. Build
   1. Build Dart app
   1. Copy C# files into `butter/Flutter/ephemeral`
   1. Add ephemeral directory to .gitignore, remove tracked files
1. Create template
1. Remove `Microsoft.Windows.CsWin32` dependency from user's app

### Useful resources

1. https://github.com/LiveOrNot/FlutterSharp/blob/8b24bdf14465c090b53ecc04c0c2c2598ae7aff3/FlutterSharp/Integrations/FlutterInterop.cs
2. https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
3. https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
4. https://github.com/sony/flutter-elinux