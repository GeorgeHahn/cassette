using System;

namespace Cassette.Scripts
{
    public class TypeScriptCompileException : Exception
    {
        public TypeScriptCompileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TypeScriptCompileException(string message)
            : base(message)
        {
        }
    }
}

