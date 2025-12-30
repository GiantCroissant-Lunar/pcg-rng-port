using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32C64OneseqKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32C64Oneseq(42);

        uint[] expectedFirst =
        {
            0x77b22db0u,
            0xc0d701deu,
            0xa73ff12eu,
            0x7bec2cfcu,
            0x94682a9du,
            0x493de263u
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
            "TTHTTHHTTTTHHTHHTHTHTHTTHHHHTTTHTHHHHTHTTHHTTHHTHTHTHHHHHTTTHHTHH";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            6, 4, 6, 6, 6, 2, 3, 3, 3, 2, 3, 1, 1, 1, 4, 2, 3, 6, 4, 3, 5, 3, 1, 6, 2,
            1, 5, 3, 1, 5, 4, 4, 5
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
            "Kc","2d","9h","2s","7d","5c","Td","6h","9c","Th","5s","8h","3s","3h","Jc","4d","Qc","4h","Ac","Js","6s","Kd",
            "4c","9s","9d","8d","8c","Kh","Qh","Jh","6d","Ts","7s","8s","2c","2h","Tc","Ad","5h","3c","7h","As","Qd","5d",
            "Jd","3d","4s","6c","Ks","7c","Ah","Qs"
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
