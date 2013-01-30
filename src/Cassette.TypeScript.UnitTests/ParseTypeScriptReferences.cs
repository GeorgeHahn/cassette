using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class ParseLessReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToLessAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.ts");

            var lessSource = @"
// @reference ""another1.ts"";
// @reference '/another2.ts';
// @reference '../test/another3.ts';
";
            asset.Setup(a => a.OpenStream())
                 .Returns(lessSource.AsStream());
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseTypeScriptReferences();
            processor.Process(bundle);

            asset.Verify(a => a.AddReference("another1.ts", 2));
            asset.Verify(a => a.AddReference("/another2.ts", 3));
            asset.Verify(a => a.AddReference("../test/another3.ts", 4));
        }
    }
}

