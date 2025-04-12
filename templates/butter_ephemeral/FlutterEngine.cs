using Butter.Windows.Bindings;

namespace Butter.Windows;

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
  private readonly EngineHandle _handle;

  internal FlutterEngine(EngineHandle handle)
  {
    _handle = handle;
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

    var handle = Flutter.FlutterDesktopEngineCreate(properties)
      ?? throw new FlutterException("Failed to create FlutterEngine");

    return new FlutterEngine(handle);
  }

  internal EngineHandle Handle => _handle;

  public bool Run() => Flutter.FlutterDesktopEngineRun(_handle, entryPoint: null);

  public void ReloadSystemFonts() => Flutter.FlutterDesktopEngineReloadSystemFonts(_handle);

  public void OnNextFrame(Action callback)
  {
    // TODO: Listen for next frame and invoke callbacks.
    callback();
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
      _handle.Dispose();
    }
  }
}
