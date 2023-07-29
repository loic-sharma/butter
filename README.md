# Butter

### Local testing

1. Build engine
2. Copy the locall built engine to this repo's directory:

   ```
   cp C:\Code\f\engine\src\out\host_debug_unopt\flutter_windows.dll C:\Code\butter\
   ```

3. Create a test app and run it once using local engine

   ```
   flutter create my_app
   cd my_app
   flutter run -d windows --local-engine host_debug_unopt
   ```

4. Now run using Butter:

   ```
   dotnet build C:\Code\butter ; dotnet C:\Code\butter\bin\Debug\net6.0-windows8.0\Butter.dll
   ```

### Useful resources

1. https://github.com/LiveOrNot/FlutterSharp/blob/8b24bdf14465c090b53ecc04c0c2c2598ae7aff3/FlutterSharp/Integrations/FlutterInterop.cs
1. https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
2. https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
