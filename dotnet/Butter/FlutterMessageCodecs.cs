using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
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

public enum StandardCodecType
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

public ref struct StandardCodecReader
{
  private readonly ReadOnlySpan<byte> _buffer;

  private int _position = 0;
  private int? _size = 0;

  public StandardCodecReader(ReadOnlySpan<byte> buffer)
  {
    _buffer = buffer;
  }

  public StandardCodecType CurrentType { get; private set; } = StandardCodecType.None;
  public ReadOnlySpan<byte> ValueSpan { get; private set; }
  
  public bool Read()
  {
    if (_position >= _buffer.Length) return false;

    int type = _buffer[_position++];
    switch (type)
    {
      case (byte)StandardCodecType.Null:
      case (byte)StandardCodecType.True:
      case (byte)StandardCodecType.False:
        CurrentType = (StandardCodecType)type;
        ValueSpan = ReadOnlySpan<byte>.Empty;
        _size = null;
        break;
      case (byte)StandardCodecType.Int32:
        CurrentType = StandardCodecType.Int32;
        ValueSpan = _buffer.Slice(_position, 4);
        _position += 4;
        _size = null;
        break;
      case (byte)StandardCodecType.Int64:
        CurrentType = StandardCodecType.Int64;
        ValueSpan = _buffer.Slice(_position, 8);
        _position += 8;
        _size = null;
        break;
      case (byte)StandardCodecType.Float64:
        CurrentType = StandardCodecType.Float64;
        _ReadAlignment(8);
        ValueSpan = _buffer.Slice(_position, 8);
        _position += 8;
        _size = null;
        break;
      case (byte)StandardCodecType.LargeInt:
      case (byte)StandardCodecType.String:
        _size = _ReadSize();

        CurrentType = StandardCodecType.String;
        ValueSpan = _buffer.Slice(_position, _size.Value);
        _position += _size.Value;
        break;
      case (byte)StandardCodecType.UInt8List:
      case (byte)StandardCodecType.Int32List:
      case (byte)StandardCodecType.Int64List:
      case (byte)StandardCodecType.Float32List:
      case (byte)StandardCodecType.Float64List:
        CurrentType = (StandardCodecType)type;

        _size = _ReadSize();
        var itemSize = CurrentType switch
        {
          StandardCodecType.UInt8List => 1,
          StandardCodecType.Int32List | StandardCodecType.Float32List => 2,
          StandardCodecType.Int64List | StandardCodecType.Float64List => 4,
          _ => throw new UnreachableException(),
        };

        if (itemSize > 1)
        {
          _ReadAlignment(itemSize);
        }

        ValueSpan = _buffer.Slice(_position, _size.Value * itemSize);
        _position = _size.Value * itemSize;
        break;

      case (byte)StandardCodecType.List:
      case (byte)StandardCodecType.Map:
        CurrentType = (StandardCodecType)type;
        ValueSpan = ReadOnlySpan<byte>.Empty;
        _size = _ReadSize();
        break;

      default:
          throw new InvalidDataException($"Invalid type byte '{type}'.");
    }

    return true;
  }

  public int GetSize()
  {
    switch (CurrentType)
    {
      case StandardCodecType.UInt8List:
      case StandardCodecType.Int32List:
      case StandardCodecType.Int64List:
      case StandardCodecType.Float32List:
      case StandardCodecType.Float64List:
      case StandardCodecType.List:
      case StandardCodecType.Map:
        break;
      
      default:
        throw new InvalidOperationException($"Cannot get size for type '{CurrentType}'.");
    }

    Debug.Assert(_size != null);
    return _size.Value;
  }

  public bool GetBool()
  {
    return CurrentType switch {
      StandardCodecType.False => false,
      StandardCodecType.True => true,
      _ => throw new InvalidOperationException($"Invalid current type: {CurrentType}"),
    };
  }

  public int GetInt32()
  {
    _VerifyType(StandardCodecType.Int32);
    return BinaryPrimitives.ReadInt32BigEndian(ValueSpan);
  }

  public long GetInt64()
  {
    _VerifyType(StandardCodecType.Int64);
    return BinaryPrimitives.ReadInt64BigEndian(ValueSpan);
  }

  public double GetFloat64()
  {
    _VerifyType(StandardCodecType.Float64);
    return BinaryPrimitives.ReadDoubleBigEndian(ValueSpan);
  }

  public string GetString()
  {
    _VerifyType(StandardCodecType.String);
    return Encoding.UTF8.GetString(ValueSpan);
  }

  public byte GetUInt8ListValue(int index)
  {
    _VerifyType(StandardCodecType.UInt8List);
    return ValueSpan[index];
  }


  public float GetFloat32ListValue(int index)
  {
    _VerifyType(StandardCodecType.Float32List);
    return BinaryPrimitives.ReadSingleBigEndian(ValueSpan.Slice(index * 4));
  }

  public double GetFloat64ListValue(int index)
  {
    _VerifyType(StandardCodecType.Float64List);
    return BinaryPrimitives.ReadDoubleBigEndian(ValueSpan.Slice(index * 8));
  }

  private void _ReadAlignment(int alignment)
  {
    var mod = _position % alignment;
    if (mod != 0)
    {
      _position += alignment - mod;
    }
  }

  private int _ReadSize()
  {
    var sizeSpan = _buffer.Slice(_position);
    var sizeFirstByte = sizeSpan[0];
    if (sizeFirstByte < 254)
    {
      _position += 1;
      return sizeFirstByte;
    }
    else if (sizeFirstByte == 254)
    {
      _position += 2;
      return BinaryPrimitives.ReadUInt16BigEndian(sizeSpan);
    }
    else
    {
      _position += 4;
      return BinaryPrimitives.ReadInt32BigEndian(sizeSpan);
    }
  }

  private void _VerifyType(StandardCodecType type)
  {
    if (CurrentType != type)
    {
      throw new InvalidOperationException($"Invalid current type {CurrentType}, expected: {type}");
    }
  }
}
