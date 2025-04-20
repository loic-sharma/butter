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

  // TODO: Remove this.
  // private static void AddMessageChannel(MainWindowApp app)
  // {
  //   var channel = new BasicMessageChannel<string>(
  //     "test.example.butter",
  //     app.Engine.Messenger,
  //     StringCodec.Instance);

  //   channel.SetMessageHandler((message) =>
  //   {
  //     Console.WriteLine($"Received '{message}'");
  //     return Task.FromResult("Hello friend!");
  //   });
  // }
}
