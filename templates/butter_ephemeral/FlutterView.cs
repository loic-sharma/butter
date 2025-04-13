using Butter.Bindings;

namespace Butter;

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
