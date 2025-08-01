namespace Butter.Tests;

public class EncodableValueTest
{
  [Fact]
  public void Default()
  {
    var value = new EncodableValue();

    Assert.Equal(StandardCodecType.Null, value.Type);
  }
}

public class StandardMessageCodecTest
{
  [Fact]
  public void DecodeNull()
  {
    var buffer = new byte[] { 0 };

    var codec = new StandardMessageCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal(StandardCodecType.Null, value.Type);
  }

  [Fact]
  public void DecodeTrue()
  {
    var buffer = new byte[] { 1 };

    var codec = new StandardMessageCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal(StandardCodecType.True, value.Type);
  }

  [Fact]
  public void DecodeFalse()
  {
    var buffer = new byte[] { 2 };

    var codec = new StandardMessageCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal(StandardCodecType.False, value.Type);
  }

  [Fact]
  public void DecodeString()
  {
    // String: "foo"
    var buffer = new byte[] { 7, 3, 102, 111, 111 };

    var codec = new StandardMessageCodec();
    var value = codec.DecodeMessage(buffer);

    Assert.Equal("foo", value.GetString());
  }

  [Fact]
  public void DecodeListOfFloat64()
  {
    // List of doubles:
    // [1, 2, 3]
    var buffer = new byte[] { 12, 3, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 63, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var codec = new StandardMessageCodec();
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

    var codec = new StandardMessageCodec();
    var value = codec.DecodeMessage(buffer);
    var map = value.GetMap().ToList();

    Assert.Equal(2, map.Count);
    Assert.Equal("a", map[0].Key.GetString());
    Assert.True(map[0].Value.GetBool());

    Assert.Equal("b", map[1].Key.GetString());
    Assert.False(map[1].Value.GetBool());
  }
}
