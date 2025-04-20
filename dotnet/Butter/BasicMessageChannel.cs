using System.Buffers;

namespace Butter;

public delegate Task<T> MessageHandler<T>(T message);

public class BasicMessageChannel<T>
{
  private readonly string _name;
  private readonly BinaryMessenger _messenger;
  private readonly IMessageCodec<T> _codec;

  public BasicMessageChannel(
    string name,
    BinaryMessenger messenger,
    IMessageCodec<T> codec)
  {
    _name = name;
    _messenger = messenger;
    _codec = codec;
  }

  public void Send(T message)
  {
    var writer = new ArrayBufferWriter<byte>();

    _codec.EncodeMessage(writer, message);
    _messenger.Send(_name, writer.WrittenMemory);
  }

  public async Task<T> SendAsync(T message, CancellationToken token = default)
  {
    var writer = new ArrayBufferWriter<byte>();
    _codec.EncodeMessage(writer, message);

    var response = await _messenger.SendAsync(_name, writer.WrittenMemory, token);
    return _codec.DecodeMessage(response.Span);
  }

  public void SetMessageHandler(MessageHandler<T> handler)
  {
    _messenger.SetHandler(_name, async (message, responseWriter) =>
    {
      // TODO: Handle the case where the decoder cannot decode the message.
      var decodedMessage = _codec.DecodeMessage(message.Span);
      var response = await handler(decodedMessage);

      _codec.EncodeMessage(responseWriter, response);
    });
  }
}
