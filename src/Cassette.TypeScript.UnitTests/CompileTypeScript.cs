using Cassette.BundleProcessing;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class CompileTypeScript_Tests
    {
        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerAddedToTypeScriptAsset()
        {
            var processor = new CompileTypeScript(Mock.Of<ITypeScriptCompiler>(), new CassetteSettings());
            var bundle = new ScriptBundle("~");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("test.ts");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)));
        }

        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerNotAddedToJsAsset()
        {
            var processor = new CompileTypeScript(Mock.Of<ITypeScriptCompiler>(), new CassetteSettings());
            var bundle = new ScriptBundle("~");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("test.ts");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)), Times.Never());
        }
    }
}