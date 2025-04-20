using System.Buffers;

namespace Butter;

interface IMethodCodec<T>
{
  MethodCall<T> DecodeMethodCall(ReadOnlySpan<byte> message);
  MethodResult<T> DecodeMethodResult(ReadOnlySpan<byte> message);

  void EncodeMethodCall(IBufferWriter<byte> writer, MethodCall<T> message);
  void EncodeMethodResult(IBufferWriter<byte> writer, MethodResult<T> result);
}

public class StandardMethodCodec : IMethodCodec<EncodableValue>
{
  public static readonly StandardMethodCodec Instance = new StandardMethodCodec();

  public void EncodeMethodCall(
    IBufferWriter<byte> writer,
    MethodCall<EncodableValue> message)
  {
    var codec = new StandardCodecWriter(writer);

    codec.WriteString(message.Name);
    codec.WriteValue(message.Arguments);
  }

  public MethodCall<EncodableValue> DecodeMethodCall(ReadOnlySpan<byte> message)
  {
    var reader = new StandardCodecReader(message);

    if (!reader.Read())
    {
      throw new InvalidOperationException("No more data to read.");
    }

    var method = reader.GetString();
    var arguments = reader.ReadValue();

    return new MethodCall<EncodableValue>(method, arguments);
  }

  public void EncodeMethodResult(
    IBufferWriter<byte> writer,
    MethodResult<EncodableValue> result)
  {
    throw new NotImplementedException();
  }

  public MethodResult<EncodableValue> DecodeMethodResult(ReadOnlySpan<byte> message)
  {
    throw new NotImplementedException();
  }
}

public record MethodCall<T>(string Name, T Arguments);

public enum MethodResultKind
{
  Success,
  Error,
}

public record SuccessMethodResult<T>(T Result) : MethodResult<T>(MethodResultKind.Success);

public record ErrorMethodResult<T>(
  string ErrorCode,
  string? ErrorMessage,
  T? ErrorDetails) : MethodResult<T>(MethodResultKind.Error);

public abstract record MethodResult<T>(MethodResultKind Kind)
{
  public SuccessMethodResult<T> AsSuccess()
  {
    VerifyKind(MethodResultKind.Success);
    return (SuccessMethodResult<T>)this;
  }

  public ErrorMethodResult<T> AsError()
  {
    VerifyKind(MethodResultKind.Error);
    return (ErrorMethodResult<T>)this;
  }

  private void VerifyKind(MethodResultKind kind)
  {
    if (kind != Kind) throw new InvalidOperationException($"Method result is a {Kind}, not a {kind}.");
  }
}
