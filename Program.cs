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

    using var engine = FlutterEngine.Create(properties);
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

internal class FlutterWindow : IDisposable
{
  private const string WindowClassName = "BUTTER_WINDOW";

  private static readonly Dictionary<HWND, FlutterWindow> Windows = new();

  private readonly HWND _host;
  private readonly FlutterViewController _controller;

  public FlutterWindow(HWND host, FlutterViewController controller)
  {
    _host = host;
    _controller = controller;
  }

  // TODO: This is not thread safe as it mutates a global.
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
      engine,
      frame.Width,
      frame.Height);

    Window.SetParent(controller.View.Hwnd, host);
    Window.Move(controller.View.Hwnd, frame);
    Window.SetFocus(controller.View.Hwnd);

    // Now wrap the two windows a FlutterWindow abstraction.
    var window = new FlutterWindow(host, controller);

    Windows[host] = window;

    return window;
  }

  public void Show() => Window.Show(_host);

  private void Destroy()
  {
    if (Windows.ContainsKey(_host))
    {
      // There view's HWND does not need to be destroyed as that's already
      // done by destroying the view controller.
      _controller.Dispose();
      Window.Destroy(_host);
      Windows.Remove(_host);
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
        Window.Move(window._controller.View.Hwnd, frame);
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
