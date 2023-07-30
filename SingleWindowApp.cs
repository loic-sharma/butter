using Windows.Win32;

namespace Butter;

internal class SingleWindowAppBuilder
{
  private const int DefaultWidth = 900;
  private const int DefaultHeight = 672;

  private readonly string[] _args;
  private string _title = "Butter app";
  private Frame _frame = Frame.FromXYWH(0, 0, DefaultWidth, DefaultHeight);

  public SingleWindowAppBuilder(string[] args)
  {
    _args = args;
  }

  public SingleWindowAppBuilder UseTitle(string title)
  {
    _title = title;
    return this;
  }

  public SingleWindowAppBuilder UseFrame(
    int x = 0,
    int y = 0,
    int width = DefaultWidth,
    int height = DefaultHeight)
  {
    _frame = Frame.FromXYWH(x, y, width, height);
    return this;
  }

  public SingleWindowApp Build()
  {
    var cwd = Directory.GetCurrentDirectory();
    using var engine = FlutterEngine.Create(new FlutterEngineOptions
    {
      AotLibraryPath = Path.Join(cwd, "build", "windows", "app.so"),
      IcuDataPath = Path.Join(cwd, "windows", "flutter", "ephemeral", "icudtl.dat"),
      AssetsPath = Path.Join(cwd, "build", "flutter_assets"),
      DartArgs = _args,
    });

    FlutterWindow.RegisterWindowClass();
    var window = FlutterWindow.Create(
      engine,
      _title,
      _frame);

    engine.OnNextFrame(window.Show);

    return new SingleWindowApp(window);
  }
}

internal class SingleWindowApp : IDisposable
{
  public static SingleWindowAppBuilder CreateBuilder(string[] args)
  {
    return new SingleWindowAppBuilder(args);
  }

  internal SingleWindowApp(FlutterWindow window)
  {
    Window = window;
  }

  public FlutterWindow Window { get; }

  public void Run()
  {
    while (PInvoke.GetMessage(out var message, default, 0, 0))
    {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }

  public void Dispose() => Window.Dispose();
}
