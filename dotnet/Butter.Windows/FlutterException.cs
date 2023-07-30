using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;

namespace Butter.Windows;

public class FlutterException : Exception
{
  public FlutterException(string message) : base(message) { }
}
