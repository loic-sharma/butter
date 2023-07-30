using Butter.Windows.Bindings;
using Windows.Win32.Foundation;

namespace Butter.Windows;

internal class FlutterView
{
  private readonly FlutterDesktopViewRef _viewRef;

  public FlutterView(FlutterDesktopViewRef viewRef, HWND hwnd)
  {
    _viewRef = viewRef;
    Hwnd = hwnd;
  }

  public HWND Hwnd { get; private set; }
}
