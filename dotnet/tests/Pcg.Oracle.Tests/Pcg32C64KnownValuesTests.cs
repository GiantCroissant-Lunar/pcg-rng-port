using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32C64KnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32C64(42, 54);

        uint[] expectedFirst =
        {
            0x020b2353u,
            0x7a157379u,
            0xada83160u,
            0x2953c47eu,
            0x77c0190du,
            0x3f19bcb1u
        };

        uint[] first = new uint[expectedFirst.Length];
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
            "TTTHHHHTTHHTHHHHTHHHHHTHTTHTTHTTHHTHTTTTTHHTTHHTHHHHHTHTTHHTHTHTT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            4, 3, 2, 1, 6, 2, 3, 6, 2, 2, 3, 6, 3, 6, 2, 1, 4, 4, 2, 1, 4, 5, 1, 4, 6,
            2, 4, 1, 2, 5, 6, 2, 4
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
            "Qc","Jh","9c","5c","7s","Kh","2c","Ts","8h","Ks","Ad","5h","Th","Qs","4h","6d","2s","Qh","Js","6c","7c","5s",
            "8c","Ac","2d","8d","9d","Kc","3h","4c","9s","9h","Kd","3s","6h","5d","Td","Ah","As","Qd","3d","4s","8s","Jd",
            "2h","Tc","6s","7d","3c","Jc","4d","7h"
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
