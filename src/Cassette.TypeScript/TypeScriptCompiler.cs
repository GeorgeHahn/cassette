using System;
using System.Collections.Generic;
using System.IO;
using BundleTransformer.Core.Assets;
using Cassette.IO;
using Cassette.Utilities;

#if NET35
using Iesi.Collections.Generic;
#endif

using BundleTransformer.TypeScript.Compilers;

namespace Cassette.Scripts
{
    public class TypeScriptCompiler : ITypeScriptCompiler
    {
        HashedSet<string> importedFilePaths;

        public CompileResult Compile(string source, CompileContext context)
        {
            var sourceFile = context.RootDirectory.GetFile(context.SourceFilePath);
            importedFilePaths = new HashedSet<string>();

            var compiler = new TypeScriptCompileEngine(true);

            string js;
            try
            {
                js = compiler.Compile(source, new List<Dependency>());
            }
            catch (Exception ex)
            {
                throw new TypeScriptCompileException(
                    string.Format("Error compiling {0}{1}{2}", context.SourceFilePath, Environment.NewLine, ex.Message),
                    ex
                );
            }

            return new CompileResult(js, importedFilePaths);
        }

        class CassetteTypeScriptFileReader
        {
            readonly IDirectory directory;
            readonly HashedSet<string> importFilePaths;

            public CassetteTypeScriptFileReader(IDirectory directory, HashedSet<string> importFilePaths)
            {
                this.directory = directory;
                this.importFilePaths = importFilePaths;
            }

            public byte[] GetBinaryFileContents(string fileName)
            {
                var file = directory.GetFile(fileName);
                importFilePaths.Add(file.FullPath);
                using (var buffer = new MemoryStream())
                {
                    using (var fileStream = file.OpenRead())
                    {
                        fileStream.CopyTo(buffer);
                    }
                    return buffer.ToArray();
                }
            }

            public string GetFileContents(string fileName)
            {
                var file = directory.GetFile(fileName);
                importFilePaths.Add(file.FullPath);
                return file.OpenRead().ReadToEnd();
            }

            public bool DoesFileExist(string fileName)
            {
                return directory.GetFile(fileName).Exists;
            }
        }
    }
}