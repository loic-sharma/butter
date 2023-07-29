using Windows.Win32;
using Windows.Win32.Foundation;

namespace Butter;

// https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
// https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
internal class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    var cwd = Directory.GetCurrentDirectory();
    using var engine = FlutterEngine.Create(new FlutterEngineOptions
    {
      AotLibraryPath = Path.Join(cwd, "build", "windows", "app.so"),
      IcuDataPath = Path.Join(cwd, "windows", "flutter", "ephemeral", "icudtl.dat"),
      AssetsPath = Path.Join(cwd, "build", "flutter_assets"),
      DartArgs = args,
    });

    FlutterWindow.RegisterWindowClass();
    using var window = FlutterWindow.Create(
      engine,
      "Butter application",
      RECT.FromXYWH(0, 0, 900, 672));

    engine.OnNextFrame(window.Show);

    while (PInvoke.GetMessage(out var message, default, 0, 0))
    {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }
}
