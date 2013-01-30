using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class TypeScriptFileSearchModifier_Tests
    {
        [Fact]
        public void ModifyAddsLessPattern()
        {
            var modifier = new TypeScriptFileSearchModifier();
            var fileSearch = new FileSearch();
            modifier.Modify(fileSearch);
            fileSearch.Pattern.ShouldContain("*.ts");
        }
    }
}