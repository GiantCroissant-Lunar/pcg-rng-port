using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64C32FastKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg64C32Fast(42);

        ulong[] expectedFirst =
        {
            0x8c3dcea9a8ec012aUL,
            0xa3e7dd5265ee932aUL,
            0x12ad974d3d6acc0cUL,
            0xb43c7893cb23f33cUL,
            0xce33b533163330c6UL,
            0x8760bf8099dc52f3UL
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
            "THTTTTTTTHHHHHTHHTHHTTHHTHHHHHHHHTHHHTHTHHHTTTHTTHTTTHHHHHHTTHTTT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            5, 3, 3, 3, 3, 3, 2, 5, 5, 5, 2, 5, 1, 1, 2, 6, 1, 3, 2, 4, 6, 5, 2, 3, 1,
            1, 1, 3, 5, 1, 4, 6, 3
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
            "Ts","2h","Ac","Jd","As","Kc","Jh","2c","Kd","Ks","Qs","5h","4d","7d","Ad","3s","3d","4c","7s","3c","5d","3h",
            "8s","Tc","9s","8h","Qc","2d","5s","Th","9d","Qh","5c","6s","9h","7c","Qd","8d","6h","4s","9c","Kh","Td","6c",
            "Ah","8c","Js","Jc","2s","4h","6d","7h"
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
