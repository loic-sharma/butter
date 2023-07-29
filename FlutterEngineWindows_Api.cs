using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;

namespace Butter;

public class FlutterEngineOptions
{
  public string? AssetsPath { get; init; }

  public string? IcuDataPath { get; init; }

  public string? AotLibraryPath { get; init; }

  public string? DartEntrypoint { get; init; }

  public string[]? DartArgs { get; init; }
}

public class FlutterEngine : IDisposable
{
  private readonly FlutterDesktopEngineRef _engineRef;
  private bool _ownsEngine = true;

  public FlutterEngine(FlutterDesktopEngineRef engineRef)
  {
    _engineRef = engineRef;
  }

  public static FlutterEngine Create(FlutterEngineOptions options)
  {
    var properties = new FlutterDesktopEngineProperties
    {
        AssetsPath = options.AssetsPath,
        IcuDataPath = options.IcuDataPath,
        AotLibraryPath = options.AotLibraryPath,
        DartEntrypoint = options.DartEntrypoint,
        // TODO
        DartEntrypointArgc = 0,
        DartEntrypointArgv = IntPtr.Zero,
    };

    var engineRef = Flutter.FlutterDesktopEngineCreate(properties)
      ?? throw new FlutterException("Failed to create FlutterEngine");

    return new FlutterEngine(engineRef);
  }

  public bool Run() => Flutter.FlutterDesktopEngineRun(_engineRef, entryPoint: null);

  public void ReloadSystemFonts() => Flutter.FlutterDesktopEngineReloadSystemFonts(_engineRef);

  public void OnNextFrame(Action callback)
  {
    // TODO: Listen for next frame and  invoke callbacks.
    callback();
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
