using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32C64FastKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32C64Fast(42);

        uint[] expectedFirst =
        {
            0x24b21affu,
            0x7d390e28u,
            0x82e2dfabu,
            0x0a2ae49eu,
            0x3b9b07d7u,
            0xd6c66bd1u
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
            "HTTTHHTHTHTTHHTHHHHHHHTHHHHTHTHHHTTHHHTTHHTHTHTHHHHTHHHHHHTTHHTHH";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            5, 5, 3, 6, 1, 3, 2, 3, 6, 6, 5, 1, 3, 4, 2, 5, 5, 3, 1, 4, 3, 2, 1, 5, 2,
            3, 5, 6, 6, 5, 4, 6, 3
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
            "Qc","6h","2h","8c","Tc","5c","Ac","7h","Ts","5d","Jd","8h","3s","Th","9s","9h","Ad","2d","Kd","Jh","7c","4h",
            "4c","9c","8s","6c","Kh","6d","Ks","4s","2c","3d","Td","Kc","Qs","8d","7s","5s","3c","9d","Ah","7d","2s","Qd",
            "Jc","3h","As","6s","Qh","4d","Js","5h"
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
