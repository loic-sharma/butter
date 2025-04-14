using System;

namespace Butter.Tests;

public class FlutterMessageCodecsTest
{
  public class StandardMessageCodecTest
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

      Assert.False(reader.Read());
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
  }

}