using Butter.Bindings;

namespace Butter;

public class EngineOptions
{
  public string? AssetsPath { get; init; }

  public string? IcuDataPath { get; init; }

  public string? AotLibraryPath { get; init; }

  public string? DartEntrypoint { get; init; }

  public string[]? DartArgs { get; init; }
}

public class Engine : IDisposable
{
  private readonly EngineHandle _handle;
  private readonly BinaryMessenger _messenger;

  internal Engine(EngineHandle handle, BinaryMessenger messenger)
  {
    _handle = handle;
    _messenger = messenger;
  }

  public static Engine Create(EngineOptions options)
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

    var handle = Flutter.FlutterDesktopEngineCreate(properties);
    if (handle.IsInvalid)
    {
      throw new ButterException("Failed to create FlutterEngine");
    }

    var messengerHandle = Flutter.FlutterDesktopEngineGetMessenger(handle);
    var messenger = new BinaryMessenger(messengerHandle);

    return new Engine(handle, messenger);
  }

  internal EngineHandle Handle => _handle;
  public BinaryMessenger Messenger => _messenger;

  public bool Run() => Flutter.FlutterDesktopEngineRun(_handle, entryPoint: null);

  public void ReloadSystemFonts() => Flutter.FlutterDesktopEngineReloadSystemFonts(_handle);

  public PluginRegistrar GetRegistrarForPlugin(string pluginName)
  {
    var handle = Flutter.FlutterDesktopEngineGetPluginRegistrar(_handle, pluginName);
    var messengerHandle = Flutter.FlutterDesktopEngineGetMessenger(_handle);
    var messenger = new BinaryMessenger(messengerHandle);
    var registrar = new PluginRegistrar(handle, messenger);

    Flutter.FlutterDesktopPluginRegistrarSetDestructionHandler(
      handle,
      (IntPtr registrarHandle) => registrar.Dispose());

    return registrar;
  }

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
