using System.IO;
using Cassette.BundleProcessing;
using Cassette.TinyIoC;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class TypeScriptBundlePipelineModifier_Tests
	{
	    readonly ScriptPipeline originalPipeline;
	    readonly IBundlePipeline<ScriptBundle> modifiedPipeline;

	    public TypeScriptBundlePipelineModifier_Tests()
	    {
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var compiler = new Mock<ITypeScriptCompiler>();
	        var minifier = new Mock<IJavaScriptMinifier>();
	        var settings = new CassetteSettings();

	        var container = new TinyIoCContainer();
	        container.Register(compiler.Object);
	        container.Register(minifier);
	        container.Register(urlGenerator);
	        container.Register(settings);

            originalPipeline = new ScriptPipeline(container, settings);
            var modifier = new TypeScriptBundlePipelineModifier();
            modifiedPipeline = modifier.Modify(originalPipeline);
	    }

        [Fact]
        public void ModifiedPipelineIsSameObjectAsOriginalPipeline()
        {
            modifiedPipeline.ShouldBeSameAs(originalPipeline);
        }

	    [Fact]
	    public void WhenModifiedPipelineProcessesBundle_ThenReferenceInLessAssetIsParsed()
	    {
	        var asset = new Mock<IAsset>();
	        asset.SetupGet(a => a.Path).Returns("~/file.ts");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.ts';".AsStream());
            var bundle = new ScriptBundle("~");
	        bundle.Assets.Add(asset.Object);

	        modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddReference("other.ts", 1));
	    }

	    [Fact]
        public void WhenModifiedPipelineProcessesBundle_ThenLessAssetHasCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.ts");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }
	}
}