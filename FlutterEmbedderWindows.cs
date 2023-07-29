﻿using System.Runtime.InteropServices;
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

// TODO: Disposal

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

// TODO: Disposal
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

// Forked from: https://github.com/LiveOrNot/FlutterSharp/blob/8b24bdf14465c090b53ecc04c0c2c2598ae7aff3/FlutterSharp/Integrations/FlutterInterop.cs
// See: https://github.com/flutter/engine/blob/68f2ed0a1db5f8de76b265b6101481db6e4ec503/shell/platform/windows/public/flutter_windows.h
public abstract class FlutterSafeHandle : SafeHandle
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

public class FlutterDesktopViewControllerState : FlutterSafeHandle
{ }

public class FlutterDesktopViewControllerRef : FlutterDesktopViewControllerState
{ }

public class FlutterDesktopView : FlutterSafeHandle
{ }

public class FlutterDesktopViewRef : FlutterDesktopView
{ }

public class FlutterDesktopEngine : FlutterSafeHandle
{ }

public class FlutterDesktopEngineRef : FlutterDesktopEngine
{ }

public class FlutterDesktopPluginRegistrar : FlutterSafeHandle
{ }

public class FlutterDesktopPluginRegistrarRef : FlutterSafeHandle
{ }

public class FlutterDesktopMessageResponseHandle : FlutterSafeHandle
{
  public static implicit operator FlutterDesktopMessageResponseHandle(IntPtr handle)
  {
    return new FlutterDesktopMessageResponseHandle { handle = handle };
  }
}

public class FlutterDesktopMessenger : FlutterSafeHandle
{ }

public class FlutterDesktopMessengerRef : FlutterSafeHandle
{ }

public class FlutterDesktopTextureRegistrar : FlutterSafeHandle
{ }

public class FlutterDesktopTextureRegistrarRef : FlutterSafeHandle
{ }

public delegate void FlutterDesktopBinaryReply(byte[] data, IntPtr dataSize, IntPtr userData);

public delegate void FlutterDesktopOnPluginRegistrarDestroyed(FlutterDesktopPluginRegistrarRef registrar);

public delegate void FlutterDesktopMessageCallback(IntPtr messenger, FlutterDesktopMessage message, IntPtr userData);

public delegate bool FlutterDesktopWindowProcCallback(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr userData, IntPtr result);

[StructLayout(LayoutKind.Sequential)]
public struct FlutterDesktopEngineProperties
{
  [MarshalAs(UnmanagedType.LPWStr)]
  public string AssetsPath;

  [MarshalAs(UnmanagedType.LPWStr)]
  public string IcuDataPath;

  [MarshalAs(UnmanagedType.LPWStr)]
  public string AotLibraryPath;

  [MarshalAs(UnmanagedType.LPStr)]
  public string DartEntrypoint;

  public int DartEntrypointArgc;

  // TODO
  public IntPtr DartEntrypointArgv;
}

[StructLayout(LayoutKind.Sequential)]
public struct FlutterDesktopMessage
{
  public IntPtr StructSize;

  [MarshalAs(UnmanagedType.LPStr)]
  public string Channel;

  public IntPtr Message;

  public IntPtr MessageSize;

  public IntPtr ResponseHandle;
}

public static class Flutter
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
  public static extern FlutterDesktopViewControllerRef FlutterDesktopViewControllerCreate(
      int width,
      int height,
      FlutterDesktopEngineRef engine);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopViewControllerDestroy(
      FlutterDesktopViewControllerRef controller);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopEngineRef FlutterDesktopViewControllerGetEngine(
      FlutterDesktopViewControllerRef controller);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopViewRef FlutterDesktopViewControllerGetView(
      FlutterDesktopViewControllerRef controller);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopViewControllerForceRedraw(
      FlutterDesktopViewControllerRef controller);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopViewControllerHandleTopLevelWindowProc(
      FlutterDesktopViewControllerRef controller,
      IntPtr hwnd,
      uint message,
      IntPtr wParam,
      IntPtr lParam,
      IntPtr result);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopEngineRef FlutterDesktopEngineCreate(
      FlutterDesktopEngineProperties engineProperties);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopEngineDestroy(
      FlutterDesktopEngineRef engine);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopEngineRun(
      FlutterDesktopEngineRef engine,
      [MarshalAs(UnmanagedType.LPStr)] string entryPoint);

  [DllImport("flutter_windows")]
  public static extern ulong FlutterDesktopEngineProcessMessages(
      FlutterDesktopEngineRef engine);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopEngineReloadSystemFonts(
      FlutterDesktopEngineRef engine);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopEngineReloadPlatformBrightness(
      FlutterDesktopEngineRef engine);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopPluginRegistrarRef FlutterDesktopEngineGetPluginRegistrar(
      FlutterDesktopEngineRef engine,
      [MarshalAs(UnmanagedType.LPStr)] string pluginName);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopMessengerRef FlutterDesktopEngineGetMessenger(
      FlutterDesktopEngineRef engine);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopTextureRegistrarRef FlutterDesktopEngineGetTextureRegistrar(
      FlutterDesktopTextureRegistrarRef textureRegistrar);

  [DllImport("flutter_windows")]
  public static extern IntPtr FlutterDesktopViewGetHWND(
      FlutterDesktopViewRef view);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopViewRef FlutterDesktopPluginRegistrarGetView(
      FlutterDesktopPluginRegistrarRef registrar);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopPluginRegistrarRegisterTopLevelWindowProcDelegate(
      FlutterDesktopPluginRegistrarRef registrar,
      FlutterDesktopWindowProcCallback callback,
      IntPtr userData);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopPluginRegistrarUnregisterTopLevelWindowProcDelegate(
      FlutterDesktopPluginRegistrarRef registrar,
      FlutterDesktopWindowProcCallback callback);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopResyncOutputStreams();

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopMessengerRef FlutterDesktopPluginRegistrarGetMessenger(
      FlutterDesktopPluginRegistrarRef registrar);

  [DllImport("flutter_windows")]
  public static extern FlutterDesktopTextureRegistrarRef FlutterDesktopRegistrarGetTextureRegistrar(
      FlutterDesktopPluginRegistrarRef registrar);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopPluginRegistrarSetDestructionHandler(
      FlutterDesktopPluginRegistrarRef registrar,
      FlutterDesktopOnPluginRegistrarDestroyed callback);

  [DllImport("flutter_windows")]
  public static extern uint FlutterDesktopGetDpiForHWND(
      IntPtr hwnd);

  [DllImport("flutter_windows")]
  public static extern uint FlutterDesktopGetDpiForMonitor(
      IntPtr monitor);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopMessengerSend(
      FlutterDesktopMessengerRef messenger,
      [MarshalAs(UnmanagedType.LPStr)] string channel,
      byte[] message,
      IntPtr messageSize);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopMessengerSendResponse(
      FlutterDesktopMessengerRef messenger,
      FlutterDesktopMessageResponseHandle handle,
      byte[] data,
      IntPtr dataLength);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopMessengerSendWithReply(
      FlutterDesktopMessengerRef messenger,
      [MarshalAs(UnmanagedType.LPStr)] string channel,
      byte[] message,
      IntPtr messageSize,
      FlutterDesktopBinaryReply reply,
      IntPtr userData);

  [DllImport("flutter_windows")]
  public static extern void FlutterDesktopMessengerSetCallback(
      FlutterDesktopMessengerRef messenger,
      [MarshalAs(UnmanagedType.LPStr)] string channel,
      FlutterDesktopMessageCallback callback,
      IntPtr userData);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopTextureRegistrarUnregisterExternalTexture(
      FlutterDesktopTextureRegistrarRef textureRegistrar,
      long textureId);

  [DllImport("flutter_windows")]
  public static extern bool FlutterDesktopTextureRegistrarMarkExternalTextureFrameAvailable(
      FlutterDesktopTextureRegistrarRef textureRegistrar,
      long textureId);
}