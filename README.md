# Butter

### Local testing

1. Build engine
2. Copy the locally built engine to the `dotnet/Butter.Windows` directory:

   ```
   cp C:\Code\f\engine\src\out\host_debug_unopt\flutter_windows.dll C:\Code\butter\dotnet\Butter.Windows
   ```

3. Create a test app and run it once using local engine

   ```
   flutter create my_app
   cd my_app
   flutter run -d windows --local-engine host_debug_unopt
   ```

4. Now run using Butter:

   ```
   dotnet build C:\Code\butter ; dotnet C:\Code\butter\dotnet\Butter.Windows\bin\Debug\net6.0-windows8.0\Butter.Windows.dll
   ```

### Useful resources

1. https://github.com/LiveOrNot/FlutterSharp/blob/8b24bdf14465c090b53ecc04c0c2c2598ae7aff3/FlutterSharp/Integrations/FlutterInterop.cs
2. https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
3. https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
4. https://github.com/sony/flutter-elinux