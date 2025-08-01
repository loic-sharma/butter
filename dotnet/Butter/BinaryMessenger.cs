using System.Buffers;
using System.Runtime.InteropServices;
using Butter.Bindings;

namespace Butter;

public delegate Task BinaryMessageHandler(
  ReadOnlyMemory<byte> message,
  IBufferWriter<byte> responseWriter);

public class BinaryMessenger : IDisposable
{
  private readonly MessengerHandle _handle;
  private readonly Dictionary<string, BinaryMessageHandler> _handlers = new Dictionary<string, BinaryMessageHandler>();

  internal BinaryMessenger(MessengerHandle handle)
  {
    _handle = handle;
  }

  public void Send(string channel, ReadOnlyMemory<byte> message)
  {
    Flutter.FlutterDesktopMessengerSend(_handle, channel, message);
  }

  public async Task<ReadOnlyMemory<byte>> SendAsync(
    string channel,
    ReadOnlyMemory<byte> message,
    CancellationToken token = default)
  {
    var completer = new TaskCompletionSource<ReadOnlyMemory<byte>>();
    using var tokenRegistration = token.Register(() => completer.TrySetCanceled());

    Flutter.FlutterDesktopMessengerSendWithReply(
      _handle,
      channel,
      message,
      (data, dataSize, userData) => completer.TrySetResult(data));

    return await completer.Task;
  }

  public void SetHandler(string channel, BinaryMessageHandler handler)
  {
    if (_handlers.ContainsKey(channel))
    {
      Flutter.FlutterDesktopMessengerSetCallback(_handle, channel, null);
    }

    _handlers[channel] = handler;

    Flutter.FlutterDesktopMessengerSetCallback(
      _handle,
      channel,
      async (messenger, message, userData) =>
      {
        // TODO: Make this thread-safe.
        // Flutter Windows locks the messenger, drops the response if the
        // messenger is not available, sends the response, then unlocks the messenger.

        // TODO: Avoid copying the message.
        var data = new byte[message.MessageSize.ToInt32()];
        Marshal.Copy(message.Message, data, 0, data.Length);

        var responseWriter = new ArrayBufferWriter<byte>();
        await handler(data, responseWriter);

        Flutter.FlutterDesktopMessengerSendResponse(
          _handle,
          message.ResponseHandle,
          responseWriter.WrittenMemory);
      });
  }

  public void Dispose()
  {
    _handle.Dispose();
  }
}
