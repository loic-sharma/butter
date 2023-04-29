using Windows.Win32;
using Windows.Win32.Foundation;

namespace Butter;

// https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
// https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
internal class Program
{
  [STAThread]
  public static void Main()
  {
    var frame = RECT.FromXYWH(0, 0, 900, 672);

    var cwd = Directory.GetCurrentDirectory();
    var properties = new FlutterDesktopEngineProperties
    {
      AotLibraryPath = Path.Join(cwd, "build/windows/app.so"),
      IcuDataPath = Path.Join(cwd, "windows/flutter/ephemeral/icudtl.dat"),
      AssetsPath = Path.Join(cwd, "build/flutter_assets"),
      DartEntrypointArgc = 0,
      DartEntrypointArgv = IntPtr.Zero,
    };

    var engine = FlutterEngine.Create(properties);
    var window = FlutterWindow.Create(engine, "Butter application", frame);

    // TODO: show after the first frame is rendered.
    window.Show();

    while (PInvoke.GetMessage(out var message, default, 0, 0))
    {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }
}

internal class FlutterWindow
{
  private const string WindowClassName = "BUTTER_WINDOW";

  private static readonly Dictionary<HWND, FlutterWindow> Windows = new();

  private Window _host;
  private Window _view;

  public FlutterWindow(Window host, Window view)
  {
    _host = host;
    _view = view;
  }

  public static FlutterWindow Create(FlutterEngine engine, string title, RECT frame)
  {
    // Create two windows: the "host" window for the application,
    // and the "view" window for Flutter to render into.
    var host = Window.Create(
      title,
      WindowClassName,
      frame,
      WndProc);

    // Create the view window and attach it to the host.
    var controller = FlutterViewController.Create(
      frame.Width,
      frame.Height,
      engine);

    var view = new Window(controller.View.GetHwnd());
    view.SetParent(host);
    view.Move(frame);
    view.SetFocus();

    // Now wrap the two windows a FlutterWindow abstraction.
    var window = new FlutterWindow(host, view);

    Windows[host.Hwnd] = window;

    return window;
  }

  public void Show() => _host.Show();

  // https://github.com/flutter/flutter/blob/845c12fb1091fe02f336cb06b60b09fa6f389481/packages/flutter_tools/templates/app_shared/windows.tmpl/runner/win32_window.cpp#L177
  public static LRESULT WndProc(HWND hwnd, uint message, WPARAM wparam, LPARAM lparam)
  {
    switch (message)
    {
      case PInvoke.WM_SIZE:
        Windows.TryGetValue(hwnd, out var window);

        if (window == null) break;
        if (window._view == null) break;

        PInvoke.GetClientRect(hwnd, out RECT frame);
        PInvoke.MoveWindow(
          window._view.Hwnd,
          frame.left,
          frame.top,
          frame.Width,
          frame.Height,
          true);
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
