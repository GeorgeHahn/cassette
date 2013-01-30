using System;
using Cassette.BundleProcessing;
using Cassette.Stylesheets;

namespace Cassette.Scripts
{
    public class CompileTypeScript : IBundleProcessor<ScriptBundle>
    {
        readonly ITypeScriptCompiler typeScriptCompiler;
        readonly CassetteSettings settings;

        public CompileTypeScript(ITypeScriptCompiler typeScriptCompiler, CassetteSettings settings)
        {
            this.typeScriptCompiler = typeScriptCompiler;
            this.settings = settings;
        }

        public void Process(ScriptBundle bundle)
        {
            foreach (var asset in bundle.Assets)
            {
                if (asset.Path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                {
                    asset.AddAssetTransformer(new CompileAsset(typeScriptCompiler, settings.SourceDirectory));
                }
            }
        }
    }
}