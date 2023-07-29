using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Butter;

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

  public static void Show(HWND hwnd)
  {
    PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);
    PInvoke.UpdateWindow(hwnd);
  }

  public static void Move(HWND hwnd, RECT frame)
  {
    PInvoke.MoveWindow(
      hwnd,
      frame.left,
      frame.top,
      frame.Width,
      frame.Height,
      true);
  }

  public static void SetParent(HWND hwnd, HWND parent)
  {
    PInvoke.SetParent(hwnd, parent);
  }

  public static void SetFocus(HWND hwnd)
  {
    PInvoke.SetFocus(hwnd);
  }

  public static void Destroy(HWND hwnd)
  {
    PInvoke.DestroyWindow(hwnd);
  }
}
