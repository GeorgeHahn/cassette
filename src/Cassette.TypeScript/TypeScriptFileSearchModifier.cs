
namespace Cassette.Scripts
{
    public class TypeScriptFileSearchModifier : IFileSearchModifier<ScriptBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.ts";
        }
    }
}