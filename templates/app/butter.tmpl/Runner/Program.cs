using Butter;

// TODO: Change namespace to match the project name.
namespace Butter.Example;

public class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    using var app = MainWindowApp.CreateBuilder(args)
      .UseTitle("Butter example app")
      .Build();

    app.Run();
  }
}
