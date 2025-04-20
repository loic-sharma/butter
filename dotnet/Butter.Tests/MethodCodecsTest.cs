namespace Butter.Tests;


public class StandardMethodCodecTest
{
  [Fact]
  public void DecodeMethodCall()
  {
    // Method call 'hello' with arguments {'a': true, 'b': 'world'}.
    var buffer = new byte[] { 7, 5, 104, 101, 108, 108, 111, 13, 2, 7, 1, 97, 1, 7, 1, 98, 7, 5, 119, 111, 114, 108, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var reader = new StandardMethodCodec();

    var method = reader.DecodeMethodCall(buffer);
    var arguments = method.Arguments.GetMap().ToList();

    Assert.Equal("hello", method.Name);
    Assert.Equal(2, arguments.Count);
    Assert.Equal("a", arguments[0].Key.GetString());
    Assert.True(arguments[0].Value.GetBool());
    Assert.Equal("b", arguments[1].Key.GetString());
    Assert.Equal("world", arguments[1].Value.GetString());
  }
}
