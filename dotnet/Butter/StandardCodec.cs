using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;

namespace Butter;

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

  // The current position in the buffer.
  private int _position = 0;

  // If the current type is a list or map, the number of items.
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
        ReadAlignment(8);
        ValueSpan = _buffer.Slice(_position, 8);
        _position += 8;
        _size = null;
        break;
      case (byte)StandardCodecType.LargeInt:
      case (byte)StandardCodecType.String:
        _size = ReadSize();

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

        _size = ReadSize();
        var itemSize = CurrentType switch
        {
          StandardCodecType.UInt8List => 1,
          StandardCodecType.Int32List | StandardCodecType.Float32List => 4,
          StandardCodecType.Int64List | StandardCodecType.Float64List => 8,
          _ => throw new UnreachableException(),
        };

        if (itemSize > 1)
        {
          ReadAlignment(itemSize);
        }

        ValueSpan = _buffer.Slice(_position, _size.Value * itemSize);
        _position = _size.Value * itemSize;
        break;

      case (byte)StandardCodecType.List:
      case (byte)StandardCodecType.Map:
        CurrentType = (StandardCodecType)type;
        ValueSpan = ReadOnlySpan<byte>.Empty;
        _size = ReadSize();
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
    VerifyType(StandardCodecType.Int32);
    return BinaryPrimitives.ReadInt32LittleEndian(ValueSpan);
  }

  public long GetInt64()
  {
    VerifyType(StandardCodecType.Int64);
    return BinaryPrimitives.ReadInt64LittleEndian(ValueSpan);
  }

  public double GetFloat64()
  {
    VerifyType(StandardCodecType.Float64);
    return BinaryPrimitives.ReadDoubleLittleEndian(ValueSpan);
  }

  public string GetString()
  {
    VerifyType(StandardCodecType.String);
    return Encoding.UTF8.GetString(ValueSpan);
  }

  public byte GetUInt8ListValue(int index)
  {
    VerifyType(StandardCodecType.UInt8List);
    return ValueSpan[index];
  }

  public int GetInt32ListValue(int index)
  {
    VerifyType(StandardCodecType.Int32List);
    return BinaryPrimitives.ReadInt32LittleEndian(ValueSpan.Slice(index * 4));
  }

  public long GetInt64ListValue(int index)
  {
    VerifyType(StandardCodecType.Int64List);
    return BinaryPrimitives.ReadInt64LittleEndian(ValueSpan.Slice(index * 8));
  }

  public float GetFloat32ListValue(int index)
  {
    VerifyType(StandardCodecType.Float32List);
    return BinaryPrimitives.ReadSingleLittleEndian(ValueSpan.Slice(index * 4));
  }

  public double GetFloat64ListValue(int index)
  {
    VerifyType(StandardCodecType.Float64List);
    return BinaryPrimitives.ReadDoubleLittleEndian(ValueSpan.Slice(index * 8));
  }

  private void ReadAlignment(int alignment)
  {
    var mod = _position % alignment;
    if (mod != 0)
    {
      _position += alignment - mod;
    }
  }

  private int ReadSize()
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
      return BinaryPrimitives.ReadUInt16LittleEndian(sizeSpan);
    }
    else
    {
      _position += 4;
      return BinaryPrimitives.ReadInt32LittleEndian(sizeSpan);
    }
  }

  private void VerifyType(StandardCodecType type)
  {
    if (CurrentType != type)
    {
      throw new InvalidOperationException($"Invalid current type {CurrentType}, expected: {type}");
    }
  }
}

public static class ReadEncodableValueExtensions
{
  public static EncodableValue ReadValue(this ref StandardCodecReader reader)
  {
    if (!reader.Read())
    {
      throw new InvalidOperationException("No more data to read.");
    }

    return reader.CurrentType switch
    {
      StandardCodecType.Null => EncodableValue.Null,
      StandardCodecType.True => EncodableValue.True,
      StandardCodecType.False => EncodableValue.False,
      StandardCodecType.Int32 => new EncodableValue(reader.GetInt32()),
      StandardCodecType.Int64 => new EncodableValue(reader.GetInt64()),
      StandardCodecType.Float64 => new EncodableValue(reader.GetFloat64()),
      StandardCodecType.String => new EncodableValue(reader.GetString()),
      StandardCodecType.UInt8List => GetUInt8ListValue(reader),
      StandardCodecType.Int32List => GetInt32ListValue(reader),
      StandardCodecType.Int64List => GetInt64ListValue(reader),
      StandardCodecType.Float32List => GetFloat32ListValue(reader),
      StandardCodecType.Float64List => GetFloat64ListValue(reader),
      StandardCodecType.List => ReadListValue(ref reader),
      StandardCodecType.Map => ReadMapValue(ref reader),
      _ => throw new NotImplementedException($"Unsupported type: {reader.CurrentType}"),
    };
  }

  private static EncodableValue GetUInt8ListValue(StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new byte[size];
    for (int i = 0; i < size; i++) values[i] = reader.GetUInt8ListValue(i);
    return new EncodableValue(values);
  }

  private static EncodableValue GetInt32ListValue(StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new int[size];
    for (int i = 0; i < size; i++) values[i] = reader.GetInt32ListValue(i);
    return new EncodableValue(values);
  }

  private static EncodableValue GetInt64ListValue(StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new long[size];
    for (int i = 0; i < size; i++) values[i] = reader.GetInt64ListValue(i);
    return new EncodableValue(values);
  }

  private static EncodableValue GetFloat32ListValue(StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new float[size];
    for (int i = 0; i < size; i++) values[i] = reader.GetFloat32ListValue(i);
    return new EncodableValue(values);
  }

  private static EncodableValue GetFloat64ListValue(StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new double[size];
    for (int i = 0; i < size; i++) values[i] = reader.GetFloat64ListValue(i);
    return new EncodableValue(values);
  }

  private static EncodableValue ReadListValue(ref StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new List<EncodableValue>(size);
    for (int i = 0; i < size; i++) values.Add(ReadValue(ref reader));
    return new EncodableValue(values);
  }

  private static EncodableValue ReadMapValue(ref StandardCodecReader reader)
  {
    var size = reader.GetSize();
    var values = new Dictionary<EncodableValue, EncodableValue>(size);
    for (int i = 0; i < size; i++)
    {
      var key = ReadValue(ref reader);
      var value = ReadValue(ref reader);
      values[key] = value;
    }
    return new EncodableValue(values);
  }
}

public struct EncodableValue
{
  private object _value;

  public static readonly EncodableValue Null = new EncodableValue(StandardCodecType.Null, 0);
  public static readonly EncodableValue True = new EncodableValue(StandardCodecType.True, true);
  public static readonly EncodableValue False = new EncodableValue(StandardCodecType.False, false);

  public EncodableValue(int value) : this(StandardCodecType.Int32, value) { }
  public EncodableValue(long value) : this(StandardCodecType.Int64, value) { }
  public EncodableValue(double value) : this(StandardCodecType.Float64, value) { }
  public EncodableValue(string value) : this(StandardCodecType.String, value) { }
  public EncodableValue(byte[] value) : this(StandardCodecType.UInt8List, value) { }
  public EncodableValue(int[] value) : this(StandardCodecType.Int32List, value) { }
  public EncodableValue(long[] value) : this(StandardCodecType.Int64List, value) { }
  public EncodableValue(float[] value) : this(StandardCodecType.Float32List, value) { }
  public EncodableValue(double[] value) : this(StandardCodecType.Float64List, value) { }
  public EncodableValue(List<EncodableValue> value) : this(StandardCodecType.List, value) { }
  public EncodableValue(Dictionary<EncodableValue, EncodableValue> value) : this(StandardCodecType.Map, value) { }

  private EncodableValue(StandardCodecType type, object value)
  {
    _value = value;
    Type = type;
  }

  public StandardCodecType Type { get; private set; }

  public bool GetBool()
  {
    return Type switch
    {
      StandardCodecType.False => false,
      StandardCodecType.True => true,
      _ => throw new InvalidOperationException($"Encodable value is type {Type}, not boolean"),
    };
  }

  public int GetInt32() => Get<int>(StandardCodecType.Int32);
  public long GetInt64() => Get<long>(StandardCodecType.Int64);
  public double GetFloat64() => Get<double>(StandardCodecType.Float64);
  public string GetString() => Get<string>(StandardCodecType.String);
  public byte[] GetUInt8List() => Get<byte[]>(StandardCodecType.UInt8List);
  public float[] GetFloat32List() => Get<float[]>(StandardCodecType.Float32List);
  public double[] GetFloat64List() => Get<double[]>(StandardCodecType.Float64List);
  public IReadOnlyList<EncodableValue> GetList() => Get<List<EncodableValue>>(StandardCodecType.List);
  public IReadOnlyDictionary<EncodableValue, EncodableValue> GetMap() => Get<Dictionary<EncodableValue, EncodableValue>>(StandardCodecType.Map);

  private T Get<T>(StandardCodecType type)
  {
    if (Type != type)
    {
      throw new InvalidOperationException($"Encodable value is type {Type}, not type {type}");
    }

    return (T)_value;
  }
}
