using System.Buffers;

namespace Butter.Tests;

public class StandardCodecReaderTest
{
  // Generate test cases using:
  //
  // final codec = StandardMessageCodec();
  // final buffer = codec.encodeMessage([1, 2, 3]);
  // print(buffer?.buffer.asUint8List());

  [Fact]
  public void ReadNull()
  {
    var buffer = new byte[] { 0 };

    var reader = new StandardCodecReader(buffer);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Null, reader.CurrentType);
    Assert.False(reader.Read());
  }

  [Fact]
  public void ReadTrue()
  {
    var buffer = new byte[] { 1 };

    var reader = new StandardCodecReader(buffer);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.True, reader.CurrentType);
    Assert.True(reader.GetBool());
    Assert.False(reader.Read());
  }

  [Fact]
  public void ReadFalse()
  {
    var buffer = new byte[] { 2 };

    var reader = new StandardCodecReader(buffer);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.False, reader.CurrentType);
    Assert.False(reader.GetBool());
    Assert.False(reader.Read());
  }

  [Fact]
  public void ReadString()
  {
    // String: "foo"
    var buffer = new byte[] { 7, 3, 102, 111, 111 };

    var reader = new StandardCodecReader(buffer);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.String, reader.CurrentType);
    Assert.Equal("foo", reader.GetString());
    Assert.False(reader.Read());
  }

  [Fact]
  public void ReadListOfFloat64()
  {
    // List of doubles:
    // [1, 2, 3]
    var buffer = new byte[] { 12, 3, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 63, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var reader = new StandardCodecReader(buffer);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.List, reader.CurrentType);
    Assert.Equal(3, reader.GetSize());

    Assert.True(reader.Read());
    Assert.Equal(1, reader.GetFloat64());

    Assert.True(reader.Read());
    Assert.Equal(2, reader.GetFloat64());

    Assert.True(reader.Read());
    Assert.Equal(3, reader.GetFloat64());

    // Ignore padding bytes.
    while (reader.Read()) Assert.Equal(StandardCodecType.Null, reader.CurrentType);
  }

  [Fact]
  public void ReadMapOfStringToBool()
  {
    // Map of string to bools:
    // { "a": true, "b": false }
    var buffer = new byte[] { 13, 2, 7, 1, 97, 1, 7, 1, 98, 2 };

    var reader = new StandardCodecReader(buffer);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Map, reader.CurrentType);
    Assert.Equal(2, reader.GetSize());

    // Entry 1
    Assert.True(reader.Read());
    Assert.Equal("a", reader.GetString());
    Assert.True(reader.Read());
    Assert.True(reader.GetBool());

    // Entry 2
    Assert.True(reader.Read());
    Assert.Equal("b", reader.GetString());
    Assert.True(reader.Read());
    Assert.False(reader.GetBool());

    Assert.False(reader.Read());
  }

  [Fact]
  public void ReadMethodCall()
  {
    // Method call 'hello' with arguments {'a': true, 'b': 'world'}.
    var buffer = new byte[] { 7, 5, 104, 101, 108, 108, 111, 13, 2, 7, 1, 97, 1, 7, 1, 98, 7, 5, 119, 111, 114, 108, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var reader = new StandardCodecReader(buffer);

    Assert.Equal("hello", reader.ReadValue().GetString());

    var arguments = reader.ReadValue().GetMap().ToList();
    Assert.Equal(2, arguments.Count);
    Assert.Equal("a", arguments[0].Key.GetString());
    Assert.True(arguments[0].Value.GetBool());
    Assert.Equal("b", arguments[1].Key.GetString());
    Assert.Equal("world", arguments[1].Value.GetString());
  }
}

public class StandardCodecWriterTest
{
  [Fact]
  public void WriteNull()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteNull();

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Null, reader.CurrentType);
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteTrue()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteBool(true);

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.True, reader.CurrentType);
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteFalse()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteBool(false);

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.False, reader.CurrentType);
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteInt32()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteInt32(123);

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Int32, reader.CurrentType);
    Assert.Equal(123, reader.GetInt32());
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteInt64()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteInt64(123);

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Int64, reader.CurrentType);
    Assert.Equal(123, reader.GetInt64());
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteFloat64()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteFloat64(123.0f);

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Float64, reader.CurrentType);
    Assert.Equal(123.0f, reader.GetFloat64());
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteString()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteString("foo");

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.String, reader.CurrentType);
    Assert.Equal("foo", reader.GetString());
    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteList()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteListStart(3);
    writer.WriteBool(true);
    writer.WriteInt32(1);
    writer.WriteString("foo");

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.List, reader.CurrentType);
    Assert.Equal(3, reader.GetSize());

    Assert.True(reader.Read());
    Assert.True(reader.GetBool());

    Assert.True(reader.Read());
    Assert.Equal(1, reader.GetInt32());

    Assert.True(reader.Read());
    Assert.Equal("foo", reader.GetString());

    Assert.False(reader.Read());
  }

  [Fact]
  public void WriteMap()
  {
    var buffer = new ArrayBufferWriter<byte>();
    var writer = new StandardCodecWriter(buffer);

    writer.WriteMapStart(2);
    writer.WriteString("a");
    writer.WriteBool(true);
    writer.WriteString("b");
    writer.WriteString("foo");

    var reader = new StandardCodecReader(buffer.WrittenSpan);

    Assert.True(reader.Read());
    Assert.Equal(StandardCodecType.Map, reader.CurrentType);
    Assert.Equal(2, reader.GetSize());

    // Entry 1
    Assert.True(reader.Read());
    Assert.Equal("a", reader.GetString());
    Assert.True(reader.Read());
    Assert.True(reader.GetBool());

    // Entry 2
    Assert.True(reader.Read());
    Assert.Equal("b", reader.GetString());
    Assert.True(reader.Read());
    Assert.Equal("foo", reader.GetString());

    Assert.False(reader.Read());
  }
}
