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
  // private static void AddStringMessageChannel(MainWindowApp app)
  // {
  //   var channel = new BasicMessageChannel<string>(
  //     "butter.string_message_channel",
  //     app.Engine.Messenger,
  //     StringCodec.Instance);

  //   // Respond to messages sent from Dart.
  //   channel.SetMessageHandler((message) =>
  //   {
  //     Console.WriteLine($"C# received message '{message}'");
  //     return Task.FromResult("Responding from C#!");
  //   });
  // }

  // TODO: Remove this.
  // private static void AddMethodChannel(MainWindowApp app)
  // {
  //   var channel = new MethodChannel<EncodableValue>(
  //     "butter.method_channel",
  //     app.Engine.Messenger,
  //     StandardMethodCodec.Instance);

  //   // Respond to messages sent from Dart.
  //   channel.SetMethodCallHandler((call) =>
  //   {
  //     switch (call.Name)
  //     {
  //       case "sendMessage":
  //         Console.WriteLine($"C# received message '{call.Arguments}'");
  //         return Task.FromResult<MethodResult<EncodableValue>>(
  //           new SuccessMethodResult<EncodableValue>(
  //             new EncodableValue("Responding from C#!")));

  //       default:
  //         return Task.FromResult<MethodResult<EncodableValue>>(
  //           new ErrorMethodResult<EncodableValue>("UnknownMethod"));
  //     }
  //   });
  // }
}
