using System.Buffers;
using System.Text;

namespace Butter;

public interface IMessageCodec<T>
{
  void EncodeMessage(IBufferWriter<byte> writer, T message);
  T DecodeMessage(ReadOnlySpan<byte> message);
}

public class StringCodec : IMessageCodec<string>
{
  public static readonly StringCodec Instance = new StringCodec();

  public void EncodeMessage(IBufferWriter<byte> writer, string message)
  {
    Encoding.UTF8.GetBytes(message, writer);
  }

  public string DecodeMessage(ReadOnlySpan<byte> message) => Encoding.UTF8.GetString(message);
}

public class StandardMessageCodec : IMessageCodec<EncodableValue>
{
  public static readonly StandardMessageCodec Instance = new StandardMessageCodec();

  public void EncodeMessage(IBufferWriter<byte> writer, EncodableValue message)
  {
    var codec = new StandardCodecWriter(writer);
    codec.WriteValue(message);
  }

  public EncodableValue DecodeMessage(ReadOnlySpan<byte> message)
  {
    var reader = new StandardCodecReader(message);
    return reader.ReadValue();
  }
}
