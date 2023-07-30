using System.Diagnostics.CodeAnalysis;
using Butter.Windows.Bindings;
using Windows.Win32.Foundation;

namespace Butter.Windows;

internal class FlutterViewController : IDisposable
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
    var hwnd = new HWND(Flutter.FlutterDesktopViewGetHWND(viewRef));
    var view = new FlutterView(viewRef, hwnd);

    return new FlutterViewController(controllerRef, engine, view);
  }

  public bool TryHandleTopLevelWindowProc(
    uint message,
    WPARAM wParam,
    LPARAM lParam,
    [NotNullWhen(true)] out LRESULT? result)
  {
    var handled = Flutter.FlutterDesktopViewControllerHandleTopLevelWindowProc(
        _controllerRef,
        View.Hwnd,
        message,
        wParam,
        lParam,
        out var value);

    result = handled ? new LRESULT(value) : null;
    return result is not null;
  }

  public void Dispose()
  {
    Flutter.FlutterDesktopViewControllerDestroy(_controllerRef);
  }
}
