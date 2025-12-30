using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64K32OneseqKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg64K32Oneseq(42);

        ulong[] expectedFirst =
        {
            0x16877a0956ab7ab2UL,
            0x6c8007d328f335a7UL,
            0x38a4f59634854ee6UL,
            0xcc6f1d46b7d5fb7aUL,
            0x18042420cc04c23dUL,
            0x60251b7df26b98dbUL
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
            "THHTHHHTHHHHTHHTTHHTTTTHTTTHHHHHHHTHHHHTTHTHHTTHTTHHTTTTHTTHTTHHT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            5, 4, 3, 6, 2, 3, 6, 1, 3, 5, 6, 5, 3, 6, 2, 4, 2, 4, 4, 5, 6, 2, 1, 4, 2,
            6, 5, 3, 3, 2, 5, 6, 6
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
            "5d","Qs","8d","4h","Qd","3c","8s","Qh","7s","7c","2c","Kc","6s","6d","Ad","Td","Jh","9c","Ac","2s","Jd","Ah",
            "3h","Kh","Tc","Th","As","2h","2d","4s","9d","Ks","3s","Ts","Jc","5h","9h","5c","7d","7h","Qc","3d","5s","4c",
            "Kd","8c","6c","4d","8h","Js","6h","9s"
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
