using System.Runtime.InteropServices;
using Butter.Bindings;

namespace Butter;

public delegate Task<byte[]> BinaryMessageHandler(byte[] message);

public class FlutterBinaryMessenger {
  private readonly MessengerHandle _handle;
  private readonly Dictionary<string, BinaryMessageHandler> _handlers = new Dictionary<string, BinaryMessageHandler>();

  internal FlutterBinaryMessenger(MessengerHandle handle) {
    _handle = handle;
  }

  public void Send(string channel, byte[] message) {
    Flutter.FlutterDesktopMessengerSend(_handle, channel, message, (IntPtr)message.Length);
  }

  public async Task<byte[]> SendAsync(string channel, byte[] message) {
    var completer = new TaskCompletionSource<byte[]>();
    Flutter.FlutterDesktopMessengerSendWithReply(
      _handle,
      channel,
      message,
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
        // TODO: Make this thread-safe.
        // Flutter Windows locks the messenger, drops the response if the
        // messenger is not available, sends the response, then unlocks the messenger.
        var data = new byte[message.MessageSize.ToInt32()];
        Marshal.Copy(message.Message, data, 0, data.Length);
        var response = await handler(data);

        Flutter.FlutterDesktopMessengerSendResponse(
          _handle,
          message.ResponseHandle,
          response,
          (IntPtr)response.Length);
      },
      IntPtr.Zero);
  }
}
