using System.Security;

namespace Butter;

interface IMethodCodec
{
  MethodCall DecodeMethodCall(ReadOnlySpan<byte> message);
  MethodResult DecodeMethodResult(ReadOnlySpan<byte> message);

  ReadOnlySpan<byte> EncodeMethodCall(MethodCall message);
  ReadOnlySpan<byte> EncodeMethodResult(MethodResult result);
}

public class StandardMethodCodec : IMethodCodec
{
  public static readonly StandardMethodCodec Instance = new StandardMethodCodec();

  public ReadOnlySpan<byte> EncodeMethodCall(MethodCall message)
  {
    var writer = new StandardCodecWriter();
    writer.WriteString(message.Name);
    writer.WriteValue(message.Arguments);
    return writer.Buffer;
  }

  public MethodCall DecodeMethodCall(ReadOnlySpan<byte> message)
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

  public ReadOnlySpan<byte> EncodeMethodResult(MethodResult result)
  {
    throw new NotImplementedException();
  }

  public MethodResult DecodeMethodResult(ReadOnlySpan<byte> message)
  {
    throw new NotImplementedException();
  }
}

public record MethodCall(string Name, EncodableValue Arguments);

public enum MethodResultKind
{
  Success,
  Error,
}

public class SuccessMethodResult : MethodResult
{
  public SuccessMethodResult(EncodableValue result)
  {
    Result = result;
  }

  public EncodableValue Result { get; }
}

public class ErrorMethodResult : MethodResult
{
  public ErrorMethodResult(string errorCode, string? errorMessage, EncodableValue? errorDetails)
  {
    ErrorCode = errorCode;
    ErrorMessage = errorMessage;
    ErrorDetails = errorDetails;
  }

  public string ErrorCode { get; }
  public string? ErrorMessage { get; }
  public EncodableValue? ErrorDetails { get; }
}

public abstract class MethodResult
{
  public MethodResultKind Kind { get; private set; }

  public SuccessMethodResult AsSuccess()
  {
    VerifyKind(MethodResultKind.Success);
    return (SuccessMethodResult)this;
  }

  public ErrorMethodResult AsError()
  {
    VerifyKind(MethodResultKind.Error);
    return (ErrorMethodResult)this;
  }

  private void VerifyKind(MethodResultKind kind)
  {
    if (kind != Kind)
    {
      throw new InvalidOperationException($"Method result is a {Kind}, not a {kind}.");
    }
  }
}
