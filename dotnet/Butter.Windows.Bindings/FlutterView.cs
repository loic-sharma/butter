using Butter.Windows.Bindings;

namespace Butter.Windows;

public class FlutterView
{
  private readonly FlutterDesktopViewRef _viewRef;

  public FlutterView(FlutterDesktopViewRef viewRef, IntPtr hwnd)
  {
    _viewRef = viewRef;
    Hwnd = hwnd;
  }

  public IntPtr Hwnd { get; private set; }
}
