using System.Runtime.InteropServices;

namespace Butter.Bindings;

// TODO: Move everything to Interop class below?
internal abstract class FlutterSafeHandle : SafeHandle
{
  public FlutterSafeHandle(bool ownsHandle = true) : base(IntPtr.Zero, ownsHandle)
  {
  }

  public override bool IsInvalid => handle == IntPtr.Zero;

  protected override bool ReleaseHandle()
  {
    SetHandle(IntPtr.Zero);
    return true;
  }
}

internal class EngineHandle : FlutterSafeHandle
{
    protected override bool ReleaseHandle()
    {
        // TODO: Currently the view controller owns the engine, if there is a view controller.
        // Destroying the view controller also destroys the engine.
        // After multi-window, the engine will be owned separately.
        if (IsClosed)
        {
            return true;
        }

        return Flutter.FlutterDesktopEngineDestroy(handle);
    }
}

internal class ViewControllerHandle : FlutterSafeHandle
{
    protected override bool ReleaseHandle()
    {
        Flutter.FlutterDesktopViewControllerDestroy(handle);
        return true;
    }
}

internal class ViewHandle : FlutterSafeHandle
{ }

internal class PluginRegistrarHandle : FlutterSafeHandle
{ }

internal class MessengerHandle : FlutterSafeHandle
{ }

internal class TextureRegistrarHandle : FlutterSafeHandle
{ }

internal delegate void FlutterDesktopBinaryReply(byte[] data, IntPtr dataSize, IntPtr userData);

internal delegate void FlutterDesktopOnPluginRegistrarDestroyed(PluginRegistrarHandle registrar);

internal delegate void FlutterDesktopMessageCallback(IntPtr messenger, FlutterDesktopMessage message, IntPtr userData);

internal delegate bool FlutterDesktopWindowProcCallback(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr userData, IntPtr result);

[StructLayout(LayoutKind.Sequential)]
internal struct FlutterDesktopEngineProperties
{
  [MarshalAs(UnmanagedType.LPWStr)]
  public string? AssetsPath;

  [MarshalAs(UnmanagedType.LPWStr)]
  public string? IcuDataPath;

  [MarshalAs(UnmanagedType.LPWStr)]
  public string? AotLibraryPath;

  [MarshalAs(UnmanagedType.LPStr)]
  public string? DartEntrypoint;

  public int DartEntrypointArgc;

  // TODO
  public IntPtr DartEntrypointArgv;
}

[StructLayout(LayoutKind.Sequential)]
struct FlutterDesktopViewControllerProperties {
  public int Width;

  public int Height;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FlutterDesktopMessage
{
  public IntPtr StructSize;

  [MarshalAs(UnmanagedType.LPStr)]
  public string Channel;

  public IntPtr Message;

  public IntPtr MessageSize;

  public IntPtr ResponseHandle;
}

// TODO: Rename to Interop?
internal static class Flutter
{
  public static IntPtr CreateSwitches(string[] switches)
  {
    var result = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * switches.Length);
    for (int i = 0; i < switches.Length; i++)
    {
      var s = Marshal.StringToHGlobalAnsi(switches[i]);
      Marshal.WriteIntPtr(result, i * Marshal.SizeOf(typeof(IntPtr)), s);
    }
    return result;
  }

  [DllImport("flutter_windows")]
  public static extern ViewControllerHandle FlutterDesktopViewControllerCreate(
      int width,
      int height,
      EngineHandle engine);

  [DllImport("flutter_windows")]
  public static extern ViewControllerHandle FlutterDesktopEngineCreateViewController(
      EngineHandle engine,
      FlutterDesktopViewControllerProperties properties);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopViewControllerDestroy(
      IntPtr controller);

  [DllImport("flutter_windows")]
  public static extern IntPtr FlutterDesktopViewControllerGetEngine(
      ViewControllerHandle controller);

  [DllImport("flutter_windows")]
  public static extern ViewHandle FlutterDesktopViewControllerGetView(
      ViewControllerHandle controller);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopViewControllerForceRedraw(
      ViewControllerHandle controller);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopViewControllerHandleTopLevelWindowProc(
      ViewControllerHandle controller,
      IntPtr hwnd,
      uint message,
      nuint wParam,
      nint lParam,
      out IntPtr result);

  [DllImport("flutter_windows")]
  public static extern EngineHandle FlutterDesktopEngineCreate(
      FlutterDesktopEngineProperties engineProperties);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopEngineDestroy(
      IntPtr engine);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopEngineRun(
      EngineHandle engine,
      [MarshalAs(UnmanagedType.LPStr)] string? entryPoint);

  [DllImport("flutter_windows")]
  public static extern ulong FlutterDesktopEngineProcessMessages(
      EngineHandle engine);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopEngineReloadSystemFonts(
      EngineHandle engine);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopEngineReloadPlatformBrightness(
      EngineHandle engine);

  [DllImport("flutter_windows")]
  public static extern PluginRegistrarHandle FlutterDesktopEngineGetPluginRegistrar(
      EngineHandle engine,
      [MarshalAs(UnmanagedType.LPStr)] string pluginName);

  [DllImport("flutter_windows")]
  public static extern MessengerHandle FlutterDesktopEngineGetMessenger(
      EngineHandle engine);

  [DllImport("flutter_windows")]
  public static extern TextureRegistrarHandle FlutterDesktopEngineGetTextureRegistrar(
      EngineHandle engine);

  [DllImport("flutter_windows")]
  public static extern IntPtr FlutterDesktopViewGetHWND(
      ViewHandle view);

  [DllImport("flutter_windows")]
  public static extern ViewHandle FlutterDesktopPluginRegistrarGetView(
      PluginRegistrarHandle registrar);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopPluginRegistrarRegisterTopLevelWindowProcDelegate(
      PluginRegistrarHandle registrar,
      FlutterDesktopWindowProcCallback callback,
      IntPtr userData);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopPluginRegistrarUnregisterTopLevelWindowProcDelegate(
      PluginRegistrarHandle registrar,
      FlutterDesktopWindowProcCallback callback);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopResyncOutputStreams();

  [DllImport("flutter_windows")]
  public static extern MessengerHandle FlutterDesktopPluginRegistrarGetMessenger(
      PluginRegistrarHandle registrar);

  [DllImport("flutter_windows")]
  public static extern TextureRegistrarHandle FlutterDesktopRegistrarGetTextureRegistrar(
      PluginRegistrarHandle registrar);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopPluginRegistrarSetDestructionHandler(
      IntPtr registrar,
      FlutterDesktopOnPluginRegistrarDestroyed callback);

  [DllImport("flutter_windows")]
  public static extern uint FlutterDesktopGetDpiForHWND(
      IntPtr hwnd);

  [DllImport("flutter_windows")]
  public static extern uint FlutterDesktopGetDpiForMonitor(
      IntPtr monitor);

  // TODO: Span and interop to avoid copying spans into byte arrays.
  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopMessengerSend(
      MessengerHandle messenger,
      [MarshalAs(UnmanagedType.LPStr)] string channel,
      byte[] message,
      IntPtr messageSize);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopMessengerSendResponse(
      MessengerHandle messenger,
      IntPtr handle,
      byte[] data,
      IntPtr dataLength);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopMessengerSendWithReply(
      MessengerHandle messenger,
      [MarshalAs(UnmanagedType.LPStr)] string channel,
      byte[] message,
      IntPtr messageSize,
      FlutterDesktopBinaryReply reply,
      IntPtr userData);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopMessengerSetCallback(
      MessengerHandle messenger,
      [MarshalAs(UnmanagedType.LPStr)] string channel,
      FlutterDesktopMessageCallback? callback,
      IntPtr userData);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopTextureRegistrarUnregisterExternalTexture(
      TextureRegistrarHandle textureRegistrar,
      long textureId);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopTextureRegistrarMarkExternalTextureFrameAvailable(
      TextureRegistrarHandle textureRegistrar,
      long textureId);
}
