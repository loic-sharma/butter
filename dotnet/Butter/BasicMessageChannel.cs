namespace Butter;

public delegate Task<T> MessageHandler<T>(T message);

public class BasicMessageChannel<T> {
  private readonly string _name;
  private readonly BinaryMessenger _messenger;
  private readonly MessageCodec<T> _codec;

  public BasicMessageChannel(
    string name,
    BinaryMessenger messenger,
    MessageCodec<T> codec) {
    _name = name;
    _messenger = messenger;
    _codec = codec;
  }

  public void Send(T message) {
    var data = _codec.EncodeMessage(message);
    _messenger.Send(_name, data);
  }

  public async Task<byte[]> SendAsync(T message) {
    var data = _codec.EncodeMessage(message);
    return await _messenger.SendAsync(_name, data);
  }

  public void SetMessageHandler(MessageHandler<T> handler) {
    _messenger.SetHandler(_name, async (message) => {
      // TODO: Handle the case where the decoder cannot decode the message.
      var decodedMessage = _codec.DecodeMessage(message);
      var response = await handler(decodedMessage);
      return _codec.EncodeMessage(response);
    });
  }
}
