using System.Buffers;
using System.Runtime.InteropServices;
using Butter.Bindings;

namespace Butter;

public delegate Task BinaryMessageHandler(
  ReadOnlyMemory<byte> message,
  IBufferWriter<byte> responseWriter);

public class BinaryMessenger {
  private readonly MessengerHandle _handle;
  private readonly Dictionary<string, BinaryMessageHandler> _handlers = new Dictionary<string, BinaryMessageHandler>();

  internal BinaryMessenger(MessengerHandle handle) {
    _handle = handle;
  }

  public void Send(string channel, ReadOnlyMemory<byte> message) {
    // TODO: Avoid copying the message.
    Flutter.FlutterDesktopMessengerSend(_handle, channel, message.ToArray(), (IntPtr)message.Length);
  }

  public async Task<byte[]> SendAsync(string channel, ReadOnlyMemory<byte> message) {
    // TODO: Avoid copying the message.
    var completer = new TaskCompletionSource<byte[]>();
    Flutter.FlutterDesktopMessengerSendWithReply(
      _handle,
      channel,
      message.ToArray(),
      (IntPtr)message.Length,
      (data, dataSize, userData) => completer.SetResult(data),
      IntPtr.Zero);
    return await completer.Task;
  }

  public void SetHandler(string channel, BinaryMessageHandler handler) {
    if (_handlers.ContainsKey(channel)) {
      Flutter.FlutterDesktopMessengerSetCallback(_handle, channel, null, IntPtr.Zero);
    }

    _handlers[channel] = handler;

    Flutter.FlutterDesktopMessengerSetCallback(
      _handle,
      channel,
      async (messenger, message, userData) => {
        // TODO: Avoid copying the message.
        // TODO: Make this thread-safe.
        // Flutter Windows locks the messenger, drops the response if the
        // messenger is not available, sends the response, then unlocks the messenger.
        var data = new byte[message.MessageSize.ToInt32()];
        Marshal.Copy(message.Message, data, 0, data.Length);

        var responseWriter = new ArrayBufferWriter<byte>();
        await handler(data, responseWriter);

        Flutter.FlutterDesktopMessengerSendResponse(
          _handle,
          message.ResponseHandle,
          responseWriter.WrittenMemory);
      },
      IntPtr.Zero);
  }
}
