using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64C32OneseqKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg64C32Oneseq(42);

        ulong[] expectedFirst =
        {
            0x676b3134f9ebe60fUL,
            0xe59f0e55fe310920UL,
            0xf7321efeda8b40cbUL,
            0xda90bfd7e3230978UL,
            0x641802497d52fda0UL,
            0xd96e3fb921e8b33dUL
        };

        ulong[] first = new ulong[expectedFirst.Length];
        for (int i = 0; i < first.Length; i++)
        {
            first[i] = rng.Next();
        }
        Assert.Equal(expectedFirst, first);

        var coinsBuilder = new StringBuilder();
        for (int i = 0; i < 65; i++)
        {
            coinsBuilder.Append(rng.Next(2) != 0 ? 'H' : 'T');
        }

        const string expectedCoins =
            "HTHHTTTHTTTHHTHHTHHTTTHTTHHHHTTHTTTTTHTTTTHTTTTTHTHHTTTHHHHHHHHHT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            2, 3, 3, 2, 2, 6, 5, 1, 2, 2, 5, 2, 5, 4, 4, 2, 1, 6, 6, 2, 6, 2, 4, 6, 4,
            4, 2, 3, 6, 6, 4, 4, 3
        };

        int[] rolls = new int[expectedRolls.Length];
        for (int i = 0; i < rolls.Length; i++)
        {
            rolls[i] = (int)rng.Next(6) + 1;
        }

        Assert.Equal(expectedRolls, rolls);

        int[] deck = Enumerable.Range(0, 52).ToArray();
        rng.Shuffle(deck);

        string[] expectedCards =
        {
            "Ts","Qh","4d","Ad","9d","8d","6c","Qd","4s","7c","8s","9h","8h","2s","Ks","5d","Ac","Tc","4c","Js","Kc","5h",
            "2h","6s","3h","Qc","2d","6h","Td","6d","Qs","3c","Ah","9c","Th","2c","3d","7s","Jd","Jh","7h","7d","Kd","5c",
            "5s","4h","8c","As","Kh","3s","Jc","9s"
        };

        string[] actualCards = deck.Select(FormatCard).ToArray();
        Assert.Equal(expectedCards, actualCards);
    }

    private static string FormatCard(int card)
    {
        const string numbers = "A23456789TJQK";
        const string suits = "hcds";

        int numberIndex = card / 4;
        int suitIndex = card % 4;

        return $"{numbers[numberIndex]}{suits[suitIndex]}";
    }
}
