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