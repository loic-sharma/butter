using System.Buffers;

namespace Butter;

// TODO: Allow resizing the channel.
// TODO: Allow disabling channel overflow warnings.
public class MethodChannel<T>
{
  private readonly string _name;
  private readonly BinaryMessenger _messenger;
  private readonly IMethodCodec<T> _codec;

  public MethodChannel(string name, BinaryMessenger messenger, IMethodCodec<T> codec)
  {
    _name = name;
    _messenger = messenger;
    _codec = codec;
  }

  public async Task<MethodResult<T>> InvokeMethodAsync(
    MethodCall<T> method,
    CancellationToken token = default)
  {
    var writer = new ArrayBufferWriter<byte>();
    _codec.EncodeMethodCall(writer, method);

    var result = await _messenger.SendAsync(_name, writer.WrittenMemory, token);
    return _codec.DecodeMethodResult(result.Span);
  }

  public void SetMethodCallHandler(Func<MethodCall<T>, Task<MethodResult<T>>> handler)
  {
    _messenger.SetHandler(_name, async (message, responseWriter) =>
    {
      var methodCall = _codec.DecodeMethodCall(message.Span);
      var result = await handler(methodCall);

      _codec.EncodeMethodResult(responseWriter, result);
    });
  }
}
