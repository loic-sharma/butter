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
    var codec = new StandardCodecWriter(writer);
    switch (result.Kind)
    {
      case MethodResultKind.Success:
        // Write the success tag.
        writer.GetSpan(1)[0] = 0;
        writer.Advance(1);

        // Write the success result.
        var success = result.AsSuccess();
        codec.WriteValue(success.Result);
        break;
      case MethodResultKind.Error:
        // Write the error tag.
        writer.GetSpan(1)[0] = 1;
        writer.Advance(1);

        // Write the error code, message, and details.
        var error = result.AsError();
        codec.WriteString(error.ErrorCode);
        if (error.ErrorMessage == null)
        {
          codec.WriteNull();
        }
        else
        {
          codec.WriteString(error.ErrorMessage);
        }
        codec.WriteValue(error.ErrorDetails);
        break;
      default:
        throw new NotSupportedException($"Unsupported method result kind: {result.Kind}.");
    }
  }

  public MethodResult<EncodableValue> DecodeMethodResult(ReadOnlySpan<byte> message)
  {
    var tag = message[0];
    var codec = new StandardCodecReader(message.Slice(1));

    switch (tag)
    {
      case 0:
        // Success result.
        var result = codec.ReadValue();
        return new SuccessMethodResult<EncodableValue>(result);
      case 1:
        // Error result.
        if (!codec.Read()) throw new InvalidDataException("Method result is missing an error code.");
        if (codec.CurrentType != StandardCodecType.String)
          throw new InvalidDataException($"Method result error code is not a string: {codec.CurrentType}.");

        var errorCode = codec.GetString();

        if (!codec.Read()) throw new InvalidDataException("Method result is missing an error message.");
        if (codec.CurrentType != StandardCodecType.String && codec.CurrentType != StandardCodecType.Null)
          throw new InvalidDataException($"Method result error message is not a string: {codec.CurrentType}.");

        string errorMessage = codec.CurrentType == StandardCodecType.String
          ? codec.GetString()
          : string.Empty;

        if (!codec.Read()) throw new InvalidDataException("Method result is missing error details.");
        var errorDetails = codec.GetValue();

        return new ErrorMethodResult<EncodableValue>(errorCode, errorMessage, errorDetails);
      default:
        throw new NotSupportedException($"Unsupported method result tag: {tag}.");
    }
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
