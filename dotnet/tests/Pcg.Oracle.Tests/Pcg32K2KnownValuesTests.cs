using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32K2KnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32K2(42, 54);

        // First six 32-bit values
        uint[] expectedFirst =
        {
            0x23ad45d5u,
            0x6e2b9c53u,
            0x23cf9e33u,
            0x24e90350u,
            0x7a160dc4u,
            0x5cfeb7adu
        };

        uint[] first = new uint[expectedFirst.Length];
        for (int i = 0; i < first.Length; i++)
        {
            first[i] = rng.Next();
        }
        Assert.Equal(expectedFirst, first);

        // Backstep 6 and re-generate
        rng.Backstep((ulong)first.Length);
        for (int i = 0; i < first.Length; i++)
        {
            Assert.Equal(expectedFirst[i], rng.Next());
        }

        // Coins: 65 flips, T/H string
        var coinsBuilder = new StringBuilder();
        for (int i = 0; i < 65; i++)
        {
            coinsBuilder.Append(rng.Next(2) != 0 ? 'H' : 'T');
        }

        const string expectedCoins =
            "TTTTHTTTTHTTHTHTTHTTHTTTTTTTTHHHTHHTHTHTTHTHHTTHTHTTTHTHTHTTTHHHT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        // Rolls: 33 dice rolls (1-6)
        int[] expectedRolls =
        {
            3, 4, 4, 6, 5, 5, 5, 6, 6, 1, 6, 6, 6, 3, 4, 5, 6, 5, 3, 3,
            6, 4, 5, 2, 1, 2, 1, 1, 4, 4, 4, 2, 5
        };

        int[] rolls = new int[expectedRolls.Length];
        for (int i = 0; i < rolls.Length; i++)
        {
            rolls[i] = (int)rng.Next(6) + 1;
        }

        Assert.Equal(expectedRolls, rolls);

        // Cards: shuffled deck of 52 cards
        int[] deck = Enumerable.Range(0, 52).ToArray();
        rng.Shuffle(deck);

        string[] expectedCards =
        {
            "8c","9s","4d","5s","As","9c","2s","Tc","7c","Ks","5d","Td","2d","7h","8h","2h","Ah","6s","2c","Qs","Jd","Kc",
            "4c","8s","Jc","5h","6d","4h","6c","Kh","Th","Kd","Js","9d","7d","Qd","6h","Qc","Jh","3s","9h","Ac","3c","Ts",
            "7s","Qh","5c","Ad","3h","4s","8d","3d"
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
