using Windows.Win32.Foundation;

namespace Butter;

internal class FlutterEngine : IDisposable
{
  private readonly FlutterDesktopEngineRef _engineRef;
  private bool _ownsEngine = true;

  public FlutterEngine(FlutterDesktopEngineRef engineRef)
  {
    _engineRef = engineRef;
  }

  public static FlutterEngine Create(FlutterDesktopEngineProperties properties)
  {
    return new FlutterEngine(Flutter.FlutterDesktopEngineCreate(properties));
  }

  internal FlutterDesktopEngineRef RelinquishEngine()
  {
    _ownsEngine = false;
    return _engineRef;
  }

  public void Dispose()
  {
    if (_ownsEngine)
    {
      Flutter.FlutterDesktopEngineDestroy(_engineRef);
    }
  }
}

internal class FlutterViewController : IDisposable
{
  private readonly FlutterDesktopViewControllerRef _controllerRef;

  public FlutterViewController(
    FlutterDesktopViewControllerRef controllerRef,
    FlutterView view)
  {
    _controllerRef = controllerRef;
    View = view;
  }

  public FlutterView View { get; private set; }

  public static FlutterViewController Create(
    FlutterEngine engine,
    int width,
    int height)
  {
    var engineRef = engine.RelinquishEngine();
    var controllerRef = Flutter.FlutterDesktopViewControllerCreate(width, height, engineRef)
      ?? throw new FlutterException("Failed to create FlutterViewController");

    var viewRef = Flutter.FlutterDesktopViewControllerGetView(controllerRef);
    var hwnd = new HWND(Flutter.FlutterDesktopViewGetHWND(viewRef));
    var view = new FlutterView(viewRef, hwnd);

    return new FlutterViewController(controllerRef, view);
  }

  public void Dispose()
  {
    Flutter.FlutterDesktopViewControllerDestroy(_controllerRef);
  }
}

internal class FlutterView
{
  private readonly FlutterDesktopViewRef _viewRef;

  public FlutterView(FlutterDesktopViewRef viewRef, HWND hwnd)
  {
    _viewRef = viewRef;
    Hwnd = hwnd;
  }

  public HWND Hwnd { get; private set; }
}

public class FlutterException : Exception
{
  public FlutterException(string message) : base(message) { }
}
