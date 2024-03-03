using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using BricksHoarder.Domain;
using MessagePack;

namespace BricksHoarder.Arch.Tests
{
    public class AggregateTests
    {
        private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(typeof(BricksHoarderDomainAssemblyPointer).Assembly, typeof(MessagePackObjectAttribute).Assembly).Build();
    }
}