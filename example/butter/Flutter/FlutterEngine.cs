using System.Diagnostics.CodeAnalysis;
using Butter.Windows.Bindings;
using Windows.Win32.Foundation;

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
