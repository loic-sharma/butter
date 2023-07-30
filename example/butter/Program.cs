using Butter.Windows;

namespace Butter.Example;

public class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    using var app = MainWindowApp.CreateBuilder(args)
      .UseTitle("Butter app")
      .UseFrame(width: 900, height: 672)
      .Build();

    app.Run();
  }
}
