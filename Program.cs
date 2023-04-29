using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Butter;

// https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
// https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
internal class Program
{
  [STAThread]
  public static void Main()
  {
    //while (!Debugger.IsAttached)
    //{
    //  Thread.Sleep(100);
    //}

    Window.Create("Butter application");

    while (PInvoke.GetMessage(out var message, default, 0, 0))
    {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }
}

internal class Window
{
  private const string WindowClassName = "BUTTER_WINDOW";

  public static void Create(string title)
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

    HWND window;
    unsafe
    {
      window =
          PInvoke.CreateWindowEx(
              0,
              WindowClassName,
              title,
              WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
              0,
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

    var cwd = Directory.GetCurrentDirectory();
    var properties = new FlutterDesktopEngineProperties
    {
      AotLibraryPath = Path.Join(cwd, "build/windows/app.so"),
      IcuDataPath = Path.Join(cwd, "windows/flutter/ephemeral/icudtl.dat"),
      AssetsPath = Path.Join(cwd, "build/flutter_assets"),
      DartEntrypointArgc = 0,
      DartEntrypointArgv = IntPtr.Zero,
    };

    var engine = Flutter.FlutterDesktopEngineCreate(properties);
    var controller = Flutter.FlutterDesktopViewControllerCreate(900, 672, engine);
    var view = Flutter.FlutterDesktopViewControllerGetView(controller);
    var hwnd = new HWND(Flutter.FlutterDesktopViewGetHWND(view));

    PInvoke.SetParent(hwnd, window);
    PInvoke.MoveWindow(hwnd, 0, 0, 900, 672, true);
    PInvoke.SetFocus(hwnd);
  }

  private static LRESULT WndProc(HWND hwnd, uint message, WPARAM wparam, LPARAM lparam)
  {
    switch (message)
    {
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