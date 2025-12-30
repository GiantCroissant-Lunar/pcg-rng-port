using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64K32FastKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg64K32Fast(42);

        ulong[] expectedFirst =
        {
            0xb630722e1a261b23UL,
            0xa3e7dd5265ee932aUL,
            0x88ec8f3994a7ea76UL,
            0xaed90a0ebbb4cc14UL,
            0x58fb1a0fa08733a2UL,
            0xab8b8a9f25b4d827UL
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
            "HTHTTHHTHTTHHTTTHTTTHHTHHHHHHHHTTTTTHHHHTTTHHHTTHHHHHTTHHHHTHHHTH";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            4, 6, 2, 1, 6, 1, 3, 1, 5, 3, 2, 5, 5, 3, 5, 2, 6, 5, 2, 1, 1, 3, 5, 2, 1,
            1, 6, 1, 1, 1, 4, 2, 1
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
            "2h","6d","8s","9h","4h","Kh","Jd","2d","Ks","Qh","8d","4c","3d","3c","5s","7h","6h","Tc","8h","2c","Ah","Jc",
            "Kd","6c","Ts","5c","Qs","9s","Kc","As","9d","7s","8c","5d","Ac","Jh","4d","Qc","Ad","5h","7d","3s","7c","6s",
            "3h","9c","Qd","2s","Td","4s","Js","Th"
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
