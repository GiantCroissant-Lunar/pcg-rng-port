using Xunit;

namespace Pcg.Oracle.Tests;

/// <summary>
/// Oracle tests for Pcg64Fast against C++ pcg64_fast reference output.
/// </summary>
public class Pcg64FastOracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg64_fast");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg64Fast_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0UL;

        var rng = new Pcg64Fast(seed);

        foreach (var entry in testCase.Sequence)
        {
            ulong actual = rng.Next();
            ulong expected = entry.AsUInt64();
            Assert.Equal(expected, actual);
        }
    }
}
