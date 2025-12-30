using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32K64FastKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32K64Fast(42);

        uint[] expectedFirst =
        {
            0x623d127cu,
            0xfa4987c9u,
            0x1a27eeeeu,
            0x603880d8u,
            0xd94c8039u,
            0x0f4aa323u
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
            "HTHTTTHHHHTHHHTTHHTHHTTTHTHHTHTHTTHHTTTTHTTHTHHHTTTHTTTTTTTHHHHHT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            3, 2, 2, 5, 4, 5, 5, 2, 6, 2, 2, 1, 3, 5, 4, 5, 4, 4, 2, 6, 2, 3, 3, 5, 5,
            1, 3, 3, 6, 1, 5, 2, 5
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
            "5d","3s","9c","Jd","6c","8d","2s","8h","Qd","9d","Kd","3h","7d","4c","7s","Jc","Js","Th","5s","2d","Ad","9h",
            "Ts","Ks","Ah","8c","3d","9s","Qh","3c","7c","5c","6h","2h","As","Kh","Td","7h","4h","Jh","Qs","Kc","2c","6d",
            "8s","Ac","Tc","4s","5h","4d","Qc","6s"
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
