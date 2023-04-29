using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Butter;

internal class Window
{
  public Window(HWND hwnd)
  {
    Debug.Assert(hwnd != IntPtr.Zero);

    Hwnd = hwnd;
  }

  public HWND Hwnd { get; private set; }

  public static Window Create(
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

    return new Window(hwnd);
  }

  public void Show()
  {
    PInvoke.ShowWindow(Hwnd, SHOW_WINDOW_CMD.SW_NORMAL);
    PInvoke.UpdateWindow(Hwnd);
  }

  public void Move(RECT frame)
  {
    PInvoke.MoveWindow(
      Hwnd,
      frame.left,
      frame.top,
      frame.Width,
      frame.Height,
      true);
  }

  public void SetParent(Window parent)
  {
    PInvoke.SetParent(Hwnd, parent.Hwnd);
  }

  public void SetFocus()
  {
    PInvoke.SetFocus(Hwnd);
  }
}
