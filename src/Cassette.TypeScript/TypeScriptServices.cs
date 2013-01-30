using Cassette.TinyIoC;

namespace Cassette.Scripts
{
    [ConfigurationOrder(20)]
    public class TypeScriptServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<ITypeScriptCompiler, TypeScriptCompiler>().AsMultiInstance();
        }
    }
}