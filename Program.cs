namespace Butter;

public class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    using var app = SingleWindowApp.CreateBuilder(args)
      .UseTitle("Butter app")
      .UseFrame(width: 900, height: 672)
      .Build();

    app.Run();
  }
}
