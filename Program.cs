﻿using Windows.Win32;
using Windows.Win32.Foundation;

namespace Butter;

// https://github.com/microsoft/CsWin32/blob/abb1b3de5bc2298cf3919a8cf724e7d18ea916c7/test/WinRTInteropTest/Program.cs#L79
// https://github.com/timsneath/win32_runner/blob/main/lib/src/window.dart
internal class Program
{
  [STAThread]
  public static void Main()
  {
    var cwd = Directory.GetCurrentDirectory();
    var properties = new FlutterDesktopEngineProperties
    {
      AotLibraryPath = Path.Join(cwd, "build/windows/app.so"),
      IcuDataPath = Path.Join(cwd, "windows/flutter/ephemeral/icudtl.dat"),
      AssetsPath = Path.Join(cwd, "build/flutter_assets"),
      DartEntrypointArgc = 0,
      DartEntrypointArgv = IntPtr.Zero,
    };

    var frame = RECT.FromXYWH(0, 0, 900, 672);
    using var engine = FlutterEngine.Create(properties);
    using var window = FlutterWindow.Create(engine, "Butter application", frame);

    // TODO: show after the first frame is rendered.
    window.Show();

    while (PInvoke.GetMessage(out var message, default, 0, 0))
    {
      PInvoke.TranslateMessage(message);
      PInvoke.DispatchMessage(message);
    }
  }
}
