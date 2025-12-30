using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32K64OneseqKnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32K64Oneseq(42);

        uint[] expectedFirst =
        {
            0x8bfad379u,
            0x21d323adu,
            0xd58574f9u,
            0x282d3aa0u,
            0x33b49afcu,
            0x5821fdb2u
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
            "TTHHTTHHTTTHTTTHTTHHTHHHHTTTTTTTHTHTHTHTHTTHHTHHTTHHTTHHHTTHHHTTH";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        int[] expectedRolls =
        {
            1, 6, 3, 2, 1, 3, 4, 4, 4, 2, 3, 2, 4, 3, 2, 3, 3, 1, 2, 2, 4, 5, 4, 6, 1,
            4, 1, 4, 4, 3, 3, 2, 2
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
            "8s","5s","2d","Qh","Ad","Kd","4c","3c","4d","Ac","Jd","Js","Kh","2h","Qc","Th","Kc","6h","5d","3h","Ah","Td",
            "7h","8c","9s","7s","9h","Ks","4s","3d","2s","7c","7d","3s","5c","2c","6s","8d","Jc","6d","9d","Ts","5h","Tc",
            "6c","Jh","Qd","9c","4h","8h","As","Qs"
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
