using Windows.Win32;

namespace Butter.Windows;

public class MainWindowAppBuilder
{
  private const int DefaultX = 10;
  private const int DefaultY = 10;
  private const int DefaultWidth = 1280;
  private const int DefaultHeight = 720;

  private readonly string[] _args;

  private string _title = "Butter app";

  private Frame _frame = Frame.FromXYWH(
    DefaultX,
    DefaultY,
    DefaultWidth,
    DefaultHeight);

  public MainWindowAppBuilder(string[] args)
  {
    _args = args;
  }

  public MainWindowAppBuilder UseTitle(string title)
  {
    _title = title;
    return this;
  }

  public MainWindowAppBuilder UseFrame(
    int x = DefaultX,
    int y = DefaultY,
    int width = DefaultWidth,
    int height = DefaultHeight)
  {
    _frame = Frame.FromXYWH(x, y, width, height);
    return this;
  }

  public MainWindowApp Build()
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

    return new MainWindowApp(window);
  }
}

public class MainWindowApp : IDisposable
{
  public static MainWindowAppBuilder CreateBuilder(string[] args)
  {
    return new MainWindowAppBuilder(args);
  }

  public static MainWindowApp Create(string[] args)
  {
    return CreateBuilder(args).Build();
  }

  internal MainWindowApp(FlutterWindow window)
  {
    MainWindow = window;
  }

  public FlutterWindow MainWindow { get; }

  public void Run()
  {
    while (PInvoke.GetMessage(out var message, default, 0, 0))
    {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }

  public void Dispose() => MainWindow.Dispose();
}
