using System.Text;

namespace Butter.Windows;

public interface FlutterMessageCodec<T> {
  byte[] EncodeMessage(T message);
  T DecodeMessage(byte[] message);
}

public class FlutterStringCodec : FlutterMessageCodec<string> {
  public static readonly FlutterStringCodec Instance = new FlutterStringCodec();

  public byte[] EncodeMessage(string message) {
    return Encoding.UTF8.GetBytes(message);
  }

  public string DecodeMessage(byte[] message) {
    return Encoding.UTF8.GetString(message);
  }
}
