using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Butter;

internal class FlutterWindow : IDisposable
{
  private const string WindowClassName = "BUTTER_WINDOW";

  private static readonly Dictionary<HWND, FlutterWindow> Windows = new();

  private readonly HWND _host;
  private readonly FlutterViewController _controller;

  private FlutterWindow(HWND host, FlutterViewController controller)
  {
    _host = host;
    _controller = controller;
  }

  // TODO: This is not thread safe as it mutates a global.
  public static FlutterWindow Create(FlutterEngine engine, string title, RECT frame)
  {
    // Create the top-level "host" window for the application.
    var host = Window.Create(
      title,
      WindowClassName,
      frame,
      WndProc);

    // Create the view and attach it to the host.
    var controller = FlutterViewController.Create(
      engine,
      frame.Width,
      frame.Height);

    PInvoke.SetParent(controller.View.Hwnd, host);
    PInvoke.MoveWindow(
      controller.View.Hwnd,
      frame.left,
      frame.top,
      frame.Width,
      frame.Height,
      bRepaint: true);
    PInvoke.SetFocus(controller.View.Hwnd);

    // Now wrap window and its view in the FlutterWindow abstraction.
    var window = new FlutterWindow(host, controller);

    Windows[host] = window;

    return window;
  }

  public void Show()
  {
    PInvoke.ShowWindow(_host, SHOW_WINDOW_CMD.SW_NORMAL);
    PInvoke.UpdateWindow(_host);
  }

  private void Destroy()
  {
    if (Windows.ContainsKey(_host))
    {
      Windows.Remove(_host);
      _controller.Dispose();
      PInvoke.DestroyWindow(_host);
    }
  }

  public void Dispose() => Destroy();

  // https://github.com/flutter/flutter/blob/845c12fb1091fe02f336cb06b60b09fa6f389481/packages/flutter_tools/templates/app_shared/windows.tmpl/runner/win32_window.cpp#L177
  public static LRESULT WndProc(HWND hwnd, uint message, WPARAM wparam, LPARAM lparam)
  {
    Windows.TryGetValue(hwnd, out var window);
    if (window == null)
    {
      return PInvoke.DefWindowProc(hwnd, message, wparam, lparam);
    }

    // TODO
    // Let Flutter and plugins handle messages first.
    // This breaks closing the window - we don't receive a redispatched WM_CLOSE message.
    // if (window._controller.TryHandleTopLevelWindowProc(message, wparam, lparam, out var result))
    // {
    //   return (LRESULT)result;
    // }

    switch (message)
    {
      case PInvoke.WM_SIZE:
        PInvoke.GetClientRect(hwnd, out RECT frame);
        PInvoke.MoveWindow(
          window._controller.View.Hwnd,
          frame.left,
          frame.top,
          frame.Width,
          frame.Height,
          bRepaint: true);
        return new LRESULT(0);

      case PInvoke.WM_FONTCHANGE:
        window._controller.Engine.ReloadSystemFonts();
        return new LRESULT(0);

      case PInvoke.WM_DESTROY:
        window.Destroy();
        PInvoke.PostQuitMessage(0);
        return new LRESULT(0);
    }

    return PInvoke.DefWindowProc(hwnd, message, wparam, lparam);
  }
}

internal static class Window
{
  public static HWND Create(
    string title,
    string className,
    RECT frame,
    WNDPROC windowProc)
  {
    unsafe
    {
      fixed (char* szClassName = className)
      {
        var wcex = default(WNDCLASSEXW);
        PCWSTR szNull = default;
        PCWSTR szCursorName = new((char*)PInvoke.IDC_ARROW);
        PCWSTR szIconName = new((char*)PInvoke.IDI_APPLICATION);
        wcex.cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>();
        wcex.lpfnWndProc = windowProc;
        wcex.cbClsExtra = 0;
        wcex.hInstance = PInvoke.GetModuleHandle(szNull);
        wcex.hCursor = PInvoke.LoadCursor(wcex.hInstance, szCursorName);
        wcex.hIcon = PInvoke.LoadIcon(wcex.hInstance, szIconName);
        wcex.hbrBackground = new HBRUSH(new IntPtr(6));
        wcex.lpszClassName = szClassName;
        PInvoke.RegisterClassEx(wcex);
      }
    }

    HWND hwnd;
    unsafe
    {
      hwnd =
          PInvoke.CreateWindowEx(
              0,
              className,
              title,
              WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
              frame.X,
              frame.Y,
              frame.Width,
              frame.Height,
              default,
              default,
              default,
              null);
    }

    return hwnd;
  }
}
