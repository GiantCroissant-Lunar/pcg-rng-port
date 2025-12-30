using Xunit;

namespace Pcg.Tests;

public class Pcg64C32Tests
{
    [Fact]
    public void Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg64C32(42, 54);
        var rng2 = new Pcg64C32(42, 54);

        for (int i = 0; i < 1000; i++)
        {
            rng1.Next();
        }

        rng2.Advance(1000);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }
}
