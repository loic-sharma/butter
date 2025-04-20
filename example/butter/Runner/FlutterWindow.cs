using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Butter.Example;

public class FlutterWindow : IDisposable
{
  private const string WindowClassName = "BUTTER_WINDOW";

  private static bool ClassRegistered = false;
  private static readonly Dictionary<HWND, FlutterWindow> Windows = new();

  private readonly ViewController _controller;

  public static void RegisterWindowClass()
  {
    if (!ClassRegistered)
    {
      Window.RegisterWindowClass(WindowClassName, WndProc);
      ClassRegistered = true;
    }
  }

  // TODO: This is not thread safe as it mutates a global.
  public static FlutterWindow Create(Engine engine, string title, Frame frame)
  {
    // Create the top-level "host" window for the application.
    var host = Window.Create(
      WindowClassName,
      title,
      frame);

    // Create the view and attach it to the host.
    var controller = ViewController.Create(
      engine,
      frame.Width,
      frame.Height);
    var viewHwnd = new HWND(controller.View.Hwnd);

    PInvoke.SetParent(viewHwnd, host);
    PInvoke.MoveWindow(
      viewHwnd,
      frame.X,
      frame.Y,
      frame.Width,
      frame.Height,
      bRepaint: true);
    PInvoke.SetFocus(viewHwnd);

    // Now wrap window and its view in the FlutterWindow abstraction.
    var window = new FlutterWindow(host, controller);

    Windows[host] = window;

    return window;
  }

  private FlutterWindow(HWND hwnd, ViewController controller)
  {
    Hwnd = hwnd;
    _controller = controller;
  }

  public HWND Hwnd { get; private set; }

  public void Show()
  {
    PInvoke.ShowWindow(Hwnd, SHOW_WINDOW_CMD.SW_NORMAL);
    PInvoke.UpdateWindow(Hwnd);
  }

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
          new HWND(window._controller.View.Hwnd),
          frame.left,
          frame.top,
          frame.Width,
          frame.Height,
          bRepaint: true);
        return new LRESULT(0);

      case PInvoke.WM_DWMCOLORIZATIONCOLORCHANGED:
        Window.UpdateTheme(hwnd);
        return new LRESULT(0);

      case PInvoke.WM_FONTCHANGE:
        window._controller.Engine.ReloadSystemFonts();
        return new LRESULT(0);

      case PInvoke.WM_DESTROY:
        window.Dispose();
        PInvoke.PostQuitMessage(0);
        return new LRESULT(0);
    }

    return PInvoke.DefWindowProc(hwnd, message, wparam, lparam);
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
      if (Windows.ContainsKey(Hwnd))
      {
        Windows.Remove(Hwnd);
        PInvoke.DestroyWindow(Hwnd);
        _controller.Dispose();
      }
    }
  }
}

internal static class Window
{
  public static void RegisterWindowClass(
    string className,
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
  }

  public static HWND Create(
    string className,
    string title,
    Frame frame)
  {
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

    if (hwnd.IsNull)
    {
      throw new ButterException("Failed to create the window");
    }

    UpdateTheme(hwnd);
    return hwnd;
  }

  public static unsafe void UpdateTheme(HWND hwnd)
  {
    // TODO: Check the registry's preferred brightness setting.
    var darkMode = new BOOL(true);
    PInvoke.DwmSetWindowAttribute(
      hwnd,
      DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
      &darkMode,
      (uint)Marshal.SizeOf<BOOL>());
  }
}

// TODO: Copy more methods from RECT.
public struct Frame
{
  private int _left;
  private int _top;
  private int _right;
  private int _bottom;

  public static Frame FromXYWH(int x, int y, int width, int height) =>
    new(x, y, unchecked(x + width), unchecked(y + height));

  internal Frame(int left, int top, int right, int bottom)
  {
    _left = left;
    _top = top;
    _right = right;
    _bottom = bottom;
  }

  public readonly int Width => unchecked(_right - _left);
  public readonly int Height => unchecked(_bottom - _top);
  public readonly int X => _left;
  public readonly int Y => _top;

  public static implicit operator RECT(Frame value) => new(
    value._left,
    value._top,
    value._right,
    value._bottom);
}
