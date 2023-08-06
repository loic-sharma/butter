using Butter.Windows.Bindings;

namespace Butter.Windows;

public class FlutterViewController : IDisposable
{
  private readonly FlutterDesktopViewControllerRef _controllerRef;

  public FlutterViewController(
    FlutterDesktopViewControllerRef controllerRef,
    FlutterEngine engine,
    FlutterView view)
  {
    _controllerRef = controllerRef;
    Engine = engine;
    View = view;
  }

  public FlutterEngine Engine { get; private set; }
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
    var hwnd = Flutter.FlutterDesktopViewGetHWND(viewRef);
    var view = new FlutterView(viewRef, hwnd);

    return new FlutterViewController(controllerRef, engine, view);
  }

  public bool TryHandleTopLevelWindowProc(
    uint message,
    nuint wParam,
    nint lParam,
    out nint? result)
  {
    var handled = Flutter.FlutterDesktopViewControllerHandleTopLevelWindowProc(
        _controllerRef,
        View.Hwnd,
        message,
        wParam,
        lParam,
        out var value);

    result = handled ? value : null;
    return result != null;
  }

  public void Dispose()
  {
    Flutter.FlutterDesktopViewControllerDestroy(_controllerRef);
  }
}
