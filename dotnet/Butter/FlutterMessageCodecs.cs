using System.Text;

namespace Butter;

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

public enum StandardCodecTokenType
{
  Null = 0,
  True = 1,
  False = 2,
  Int32 = 3,
  Int64 = 4,
  LargeInt = 5,
  Float64 = 6,
  String = 7,
  UInt8List = 8,
  Int32List = 9,
  Int64List = 10,
  Float64List = 11,
  List = 12,
  Map = 13,
  Float32List = 14,

  // Default token type if no data has been read yet.
  None,
}
