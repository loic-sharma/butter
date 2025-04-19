using System;

namespace Butter.Tests;

public class FlutterStandardCodecTest
{
  [Fact]
  public void DecodeNull()
  {
    var buffer = new byte[] { 0 };

    var codec = new FlutterStandardCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal(StandardCodecType.Null, value.Type);
  }

  [Fact]
  public void DecodeTrue()
  {
    var buffer = new byte[] { 1 };

    var codec = new FlutterStandardCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal(StandardCodecType.True, value.Type);
  }

  [Fact]
  public void DecodeFalse()
  {
    var buffer = new byte[] { 2 };

    var codec = new FlutterStandardCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal(StandardCodecType.False, value.Type);
  }

  [Fact]
  public void DecodeString()
  {
    // String: "foo"
    var buffer = new byte[] { 7, 3, 102, 111, 111 };

    var codec = new FlutterStandardCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal("foo", value.GetString());
  }

  [Fact]
  public void DecodeListOfFloat64()
  {
    // List of doubles:
    // [1, 2, 3]
    var buffer = new byte[] { 12, 3, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 63, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var codec = new FlutterStandardCodec();
    var value = codec.DecodeMessage(buffer);
    var values = value.GetList();

    Assert.Equal(3, values.Count);
    Assert.Equal(1, values[0].GetFloat64());
    Assert.Equal(2, values[1].GetFloat64());
    Assert.Equal(3, values[2].GetFloat64());
  }

  [Fact]
  public void DecodeMapOfStringToBool()
  {
    // Map of string to bools:
    // { "a": true, "b": false }
    var buffer = new byte[] { 13, 2, 7, 1, 97, 1, 7, 1, 98, 2 };

    var codec = new FlutterStandardCodec();
    var value = codec.DecodeMessage(buffer);
    var map = value.GetMap().ToList();

    Assert.Equal(2, map.Count);
    Assert.Equal("a", map[0].Key.GetString());
    Assert.True(map[0].Value.GetBool());

    Assert.Equal("b", map[1].Key.GetString());
    Assert.False(map[1].Value.GetBool());
  }
}

public class FlutterStandardMethodCodecTest
{
  [Fact]
  public void DecodeMethodCall()
  {
    // Method call 'hello' with arguments {'a': true, 'b': 'world'}.
    var buffer = new byte[] { 7, 5, 104, 101, 108, 108, 111, 13, 2, 7, 1, 97, 1, 7, 1, 98, 7, 5, 119, 111, 114, 108, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var reader = new FlutterStandardMethodCodec();

    var method = reader.DecodeMessage(buffer);
    var arguments = method.Arguments.ToList();

    Assert.Equal("hello", method.Method);
    Assert.Equal(2, arguments.Count);
    Assert.Equal("a", arguments[0].Key.GetString());
    Assert.True(arguments[0].Value.GetBool());
    Assert.Equal("b", arguments[1].Key.GetString());
    Assert.Equal("world", arguments[1].Value.GetString());
  }
}

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
