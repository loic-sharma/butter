using Butter.Bindings;

namespace Butter;

public class View
{
  // Weak reference. Does not need to be disposed.
  private readonly ViewHandle _handle;

  internal View(ViewHandle handle, IntPtr hwnd)
  {
    _handle = handle;
    Hwnd = hwnd;
  }

  public IntPtr Hwnd { get; private set; }
}
