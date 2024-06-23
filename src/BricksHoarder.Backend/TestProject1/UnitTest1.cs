using Xunit;

namespace TestProject1
{
    public interface IDependency
    {
        int Value { get; }
    }

    internal class DependencyClass : IDependency
    {
        public int Value => 1;
    }

    public class MyAwesomeTests
    {
        private readonly IDependency _d;

        public MyAwesomeTests(IDependency d) => _d = d;

        [Fact]
        public void AssertThatWeDoStuff()
        {
            Assert.Equal(1, _d.Value);
        }
    }
}