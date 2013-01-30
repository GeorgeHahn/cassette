using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class TypeScriptBundlePipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            var index = pipeline.IndexOf<ParseJavaScriptReferences>();
            pipeline.Insert<ParseTypeScriptReferences>(index + 1);
            pipeline.Insert<CompileTypeScript>(index + 2);

            return pipeline;
        }
    }
}