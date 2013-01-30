using System;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ParseTypeScriptReferences : ParseReferences<ScriptBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.Path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase);
        }

        protected override ICommentParser CreateCommentParser()
        {
            // TypeScript supports the same comment syntax as JavaScript.
            // So we'll just reuse the JavaScript comment parser!
            return new JavaScriptCommentParser();
        }
    }
}
