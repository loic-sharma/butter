using Windows.Win32;
using Windows.Win32.Foundation;

namespace Butter;

public class Program
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
