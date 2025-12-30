using System.Linq;
using System.Text;
using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64K32KnownValuesTests
{
    [Fact]
    public void Round1_CoinsRollsCards_MatchCppReference()
    {
        var rng = new Pcg64K32(42, 54);

        // First six 64-bit values
        ulong[] expectedFirst =
        {
            0x2dcbaf9339a0a8dbUL,
            0xa5486595bb7ecc26UL,
            0x800934faba0d0759UL,
            0x7eff732dee5b72cbUL,
            0x44760d3bb06156f9UL,
            0x6f3622730aff6c44UL
        };

        ulong[] first = new ulong[expectedFirst.Length];
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

        // Coins: 65 flips, T/H string (Round 1 from check-pcg64_k32.out)
        var coinsBuilder = new StringBuilder();
        for (int i = 0; i < 65; i++)
        {
            coinsBuilder.Append(rng.Next(2) != 0 ? 'H' : 'T');
        }

        const string expectedCoins =
            "THTTHHHHHHTHTTHHTHTTTHTTTTTHTTHHHHHHHHHHHHTTHHHHTTHTHTTHTHTHTHHHH";

        Assert.Equal(expectedCoins, coinsBuilder.ToString());

        // Rolls: 33 dice rolls (1-6)
        int[] expectedRolls =
        {
            4, 3, 1, 4, 1, 6, 1, 3, 6, 2, 1, 4, 2, 1, 5, 4, 5, 6, 5, 4, 6, 1, 5, 5, 1,
            2, 2, 2, 2, 6, 2, 6, 4
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
            "2s","2d","9s","Jd","3c","4h","Qs","5s","4c","2h","9c","6s","Qd","Kh","Ac","Ah","7h","8h","6c","Ks","5c","Tc",
            "Qh","8d","8s","Kd","4s","Th","9h","3h","Ts","7s","Td","5d","2c","4d","Kc","Qc","3d","As","6h","8c","6d","7c",
            "Jh","Ad","3s","5h","9d","7d","Js","Jc"
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
