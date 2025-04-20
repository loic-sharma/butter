using Butter.Bindings;

namespace Butter;

public class ViewController : IDisposable
{
  private readonly ViewControllerHandle _handle;

  internal ViewController(
    ViewControllerHandle handle,
    Engine engine,
    View view)
  {
    _handle = handle;
    Engine = engine;
    View = view;
  }

  public Engine Engine { get; private set; }
  public View View { get; private set; }

  public static ViewController Create(
    Engine engine,
    int width,
    int height)
  {
    var engineHandle = engine.Handle;
    var handle = Flutter.FlutterDesktopViewControllerCreate(width, height, engineHandle);
    if (handle.IsInvalid)
    {
      throw new ButterException("Failed to create FlutterViewController");
    }

    var viewHandle = Flutter.FlutterDesktopViewControllerGetView(handle);
    var hwnd = Flutter.FlutterDesktopViewGetHWND(viewHandle);
    var view = new View(viewHandle, hwnd);

    return new ViewController(handle, engine, view);
  }

  public bool TryHandleTopLevelWindowProc(
    uint message,
    nuint wParam,
    nint lParam,
    out nint? result)
  {
    var handled = Flutter.FlutterDesktopViewControllerHandleTopLevelWindowProc(
        _handle,
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
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      // TODO: Currently the view controller owns the engine, if there is a view controller.
      // Destroying the view controller also destroys the engine.
      // After multi-window, the engine will be owned separately.
      Engine.Handle.SetHandleAsInvalid();
      _handle.Dispose();
    }
  }
}
