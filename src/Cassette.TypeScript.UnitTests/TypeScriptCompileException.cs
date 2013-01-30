using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class LessCompileException_Tests
    {
        [Fact]
        public void LessCompileExceptionConstructorAcceptsMessage()
        {
            new TypeScriptCompileException("test").Message.ShouldEqual("test");
        }
    }
}

