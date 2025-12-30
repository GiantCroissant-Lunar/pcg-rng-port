using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64C32KnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg64C32(42, 54);

        ulong[] expectedFirst =
        {
            0x6a202138d9ee51c5UL,
            0xa0e544ab585ba357UL,
            0x45c954470b9ca7ebUL,
            0xbaa3e4f04003b756UL,
            0x69fc867f81c36264UL,
            0x79080620c60f42f1UL
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
            "HTTHHTTHTHTTTHHTTHTTHHTHHHHHTHHTTHTHTHHHHHTHHTTHTTHTTTTTTTHTHTHTH";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            6, 3, 1, 2, 5, 1, 6, 5, 5, 4, 1, 1, 1, 3, 4, 5, 3, 3, 5, 5, 2, 5, 3, 5, 2,
            3, 2, 3, 6, 6, 5, 1, 4
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
            "Jh","8c","2d","Kd","Jd","4h","8d","3c","5d","6h","Kc","9h","3h","4s","5c","6c",
            "Th","Ad","As","2c","4d","Qc","Js","Qs","7c","5s","9s","Tc","9d","Ts","3d","7d",
            "2s","3s","Qd","9c","Ks","Ac","Jc","Ah","6s","Td","7h","2h","Kh","7s","8s","5h",
            "6d","4c","8h","Qh"
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
