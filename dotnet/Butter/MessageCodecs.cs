using System.Buffers;
using System.Text;

namespace Butter;

public interface MessageCodec<T>
{
  ReadOnlySpan<byte> EncodeMessage(T message);
  T DecodeMessage(ReadOnlySpan<byte> message);
}

public class StringCodec : MessageCodec<string>
{
  public static readonly StringCodec Instance = new StringCodec();

  public ReadOnlySpan<byte> EncodeMessage(string message) => Encoding.UTF8.GetBytes(message);

  public string DecodeMessage(ReadOnlySpan<byte> message) => Encoding.UTF8.GetString(message);
}

public class StandardMessageCodec : MessageCodec<EncodableValue>
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

public class StandardMethodCodec : MessageCodec<MethodCall>
{
  public static readonly StandardMethodCodec Instance = new StandardMethodCodec();

  public ReadOnlySpan<byte> EncodeMessage(MethodCall message)
  {
    var writer = new StandardCodecWriter();
    writer.WriteString(message.Name);
    writer.WriteValue(message.Arguments);
    return writer.Buffer;
  }

  public MethodCall DecodeMessage(ReadOnlySpan<byte> message)
  {
    var reader = new StandardCodecReader(message);

    if (!reader.Read())
    {
      throw new InvalidOperationException("No more data to read.");
    }

    var method = reader.GetString();
    var arguments = reader.ReadValue();

    return new MethodCall(method, arguments);
  }
}

public record MethodCall(string Name, EncodableValue Arguments);
