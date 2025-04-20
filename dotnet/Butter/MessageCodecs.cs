using System.Text;

namespace Butter;

public interface IMessageCodec<T>
{
  ReadOnlySpan<byte> EncodeMessage(T message);
  T DecodeMessage(ReadOnlySpan<byte> message);
}

public class StringCodec : IMessageCodec<string>
{
  public static readonly StringCodec Instance = new StringCodec();

  public ReadOnlySpan<byte> EncodeMessage(string message) => Encoding.UTF8.GetBytes(message);

  public string DecodeMessage(ReadOnlySpan<byte> message) => Encoding.UTF8.GetString(message);
}

public class StandardMessageCodec : IMessageCodec<EncodableValue>
{
  public static readonly StandardMessageCodec Instance = new StandardMessageCodec();

  public ReadOnlySpan<byte> EncodeMessage(EncodableValue message)
  {
    var writer = new StandardCodecWriter();
    writer.WriteValue(message);
    return writer.Buffer;
  }

  public EncodableValue DecodeMessage(ReadOnlySpan<byte> message)
  {
    var reader = new StandardCodecReader(message);

    return reader.ReadValue();
  }
}
