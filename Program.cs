/*
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

internal class Program
{
  private unsafe static void Main(string[] args)
  {
    // https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
    var windowCaption = "Butter app";
    var className = "ButterWindow";
    WNDPROC windowProc = WndProc;
    HINSTANCE? hInstance = null;

#pragma warning disable CA1416 // Validate platform compatibility
    // https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
    HWND window;

    fixed (char* szClassName = className)
    fixed (char* szWindowCaption = windowCaption) {
      PCWSTR szNull = default;
      PCWSTR szCursorName = new((char*)PInvoke.IDC_ARROW);
      PCWSTR szIconName = new((char*)PInvoke.IDI_APPLICATION);

      var wc = default(WNDCLASSEXW);
      wc.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEXW));
      //style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
      wc.lpfnWndProc = windowProc;
      wc.cbClsExtra = 0;
      wc.hInstance = PInvoke.GetModuleHandle(szNull);
      wc.hCursor = PInvoke.LoadCursor(wc.hInstance, szCursorName);
      wc.hIcon = PInvoke.LoadIcon(wc.hInstance, szIconName);
      wc.hbrBackground = new HBRUSH(new IntPtr(6));
      wc.lpszClassName = szClassName;


      var classAtom = PInvoke.RegisterClassEx(wc);
      var classAtomPtr = new IntPtr((int)(uint)classAtom);

      window = PInvoke.CreateWindowEx(
        dwExStyle: 0,
        lpClassName: className,
        lpWindowName: windowCaption,
        dwStyle: WINDOW_STYLE.WS_OVERLAPPED | WINDOW_STYLE.WS_VISIBLE,
        X: PInvoke.CW_USEDEFAULT,
        Y: 0,
        nWidth: 900,
        nHeight: 672,
        hWndParent: default,
        hMenu: default,
        hInstance: default,
        lpParam: null
      );

      int lastError = Marshal.GetLastWin32Error();
      string errorMessage = new Win32Exception(lastError).Message;
    }

    PInvoke.ShowWindow(window, SHOW_WINDOW_CMD.SW_NORMAL);
    PInvoke.UpdateWindow(window);

    while (PInvoke.GetMessage(out var message, default, 0, 0)) {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
#pragma warning restore CA1416 // Validate platform compatibility
  }

  private static LRESULT WndProc(HWND hwnd, uint message, WPARAM wparam, LPARAM lparam) {
#pragma warning disable CA1416 // Validate platform compatibility
    switch (message) {
      case PInvoke.WM_DESTROY:
        PInvoke.PostQuitMessage(0);
        return new LRESULT(0);

      case PInvoke.WM_PAINT:
        return new LRESULT(0);
    }
    return new LRESULT(0);
#pragma warning restore CA1416 // Validate platform compatibilitys
  }
}
*/

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Butter;

internal class Program
{
  private const int AddButtonId = 1000;
  private const string WindowClassName = "ButterWindow";
  private const string WindowCaption = "Butter application";

  private static readonly Random Rnd = new();

  private static void Main() {
    RegisterWindowClass();
    CreateWindow();

    while (PInvoke.GetMessage(out var message, default, 0, 0)) {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }

  private static void RegisterWindowClass()
  {
    unsafe
    {
      fixed (char* szClassName = WindowClassName)
      {
        var wcex = default(WNDCLASSEXW);
        PCWSTR szNull = default;
        PCWSTR szCursorName = new((char*)PInvoke.IDC_ARROW);
        PCWSTR szIconName = new((char*)PInvoke.IDI_APPLICATION);
        wcex.cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>();
        wcex.lpfnWndProc = WndProc;
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

  private static void CreateWindow()
  {
    HWND window;
    unsafe
    {
      window =
          PInvoke.CreateWindowEx(
              0,
              WindowClassName,
              WindowCaption,
              WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
              PInvoke.CW_USEDEFAULT,
              0,
              900,
              672,
              default,
              default,
              default,
              null);
    }

    PInvoke.ShowWindow(window, SHOW_WINDOW_CMD.SW_NORMAL);
    PInvoke.UpdateWindow(window);
  }

  private static LRESULT WndProc(HWND hwnd, uint message, WPARAM wparam, LPARAM lparam)
  {
    switch (message) {
      case PInvoke.WM_PAINT:
        PInvoke.BeginPaint(hwnd, out PAINTSTRUCT ps);
        // TODO...
        PInvoke.EndPaint(hwnd, ps);
        break;

      case PInvoke.WM_CLOSE:
        PInvoke.DestroyWindow(hwnd);
        break;

      case PInvoke.WM_DESTROY:
        PInvoke.PostQuitMessage(0);
        break;

      default:
        return PInvoke.DefWindowProc(hwnd, message, wparam, lparam);
    }

    return new LRESULT(0);
  }
}