using Xunit;

namespace Pcg.Oracle.Tests;

/// <summary>
/// Oracle tests for Pcg64OneSeq against C++ pcg64_oneseq reference output.
/// </summary>
public class Pcg64OneSeqOracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg64_oneseq");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg64OneSeq_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0UL;

        var rng = new Pcg64OneSeq(seed);

        foreach (var entry in testCase.Sequence)
        {
            ulong actual = rng.Next();
            ulong expected = entry.AsUInt64();
            Assert.Equal(expected, actual);
        }
    }
}
