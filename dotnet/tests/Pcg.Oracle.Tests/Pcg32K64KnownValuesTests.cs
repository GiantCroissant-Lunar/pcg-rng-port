using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32K64KnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg32K64(42, 54);

        // First six 32-bit values
        uint[] expectedFirst =
        {
            0xe85244a0u,
            0x7112822fu,
            0x9325f975u,
            0xf50dea01u,
            0x8cec9bbau,
            0xaa9fa4b3u
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

        // Coins: 65 flips, T/H string (Round 1 from check-pcg32_k64.out)
        var coinsBuilder = new StringBuilder();
        for (int i = 0; i < 65; i++)
        {
            coinsBuilder.Append(rng.Next(2) != 0 ? 'H' : 'T');
        }

        const string expectedCoins =
            "HHTTTHTHTTHHHHHHTHTHTHTTHHTHTHHHHTTTHHHTTHTHHHHTHTTHTTTTTTTTHTTTT";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        // Rolls: 33 dice rolls (1-6)
        int[] expectedRolls =
        {
            4, 6, 3, 6, 2, 3, 3, 5, 5, 2, 4, 3, 6, 6, 2, 5, 3, 6, 1, 3, 1, 2, 6, 4, 6,
            5, 4, 5, 6, 1, 3, 2, 2
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
            "Qs","2d","6s","8c","6d","4d","5d","Ah","Ks","3d","5s","Qd","6h","Td","5c","4h","2c","8h","Ad","2s","Js","7c",
            "5h","Qc","Ac","7s","4s","Jd","Kc","9c","9s","2h","Kh","Ts","Qh","Jc","7h","8d","Th","Tc","3h","As","Jh","4c",
            "8s","3c","9h","6c","3s","7d","9d","Kd"
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
