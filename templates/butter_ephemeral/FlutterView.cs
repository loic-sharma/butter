using Butter.Windows.Bindings;

namespace Butter.Windows;

public class FlutterView
{
  private readonly ViewHandle _handle;

  internal FlutterView(ViewHandle handle, IntPtr hwnd)
  {
    _handle = handle;
    Hwnd = hwnd;
  }

  public IntPtr Hwnd { get; private set; }
}
