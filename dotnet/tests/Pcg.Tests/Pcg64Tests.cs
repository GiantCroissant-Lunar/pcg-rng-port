using Xunit;

namespace Pcg.Tests;

public class Pcg64Tests
{
    [Fact]
    public void Pcg64_DefaultConstructor_GeneratesValues()
    {
        var rng = new Pcg64();
        ulong value = rng.Next();
        Assert.InRange(value, Pcg64.MinValue, Pcg64.MaxValue);
    }

    [Fact]
    public void Pcg64Fast_DefaultConstructor_GeneratesValues()
    {
        var rng = new Pcg64Fast();
        ulong value = rng.Next();
        Assert.InRange(value, Pcg64Fast.MinValue, Pcg64Fast.MaxValue);
    }

    [Fact]
    public void Pcg64Fast_BoundedNext_WithinRange()
    {
        var rng = new Pcg64Fast(1234UL);

        for (int i = 0; i < 1000; i++)
        {
            ulong v = rng.Next(100);
            Assert.True(v < 100UL);
        }
    }

    [Fact]
    public void Pcg64Fast_Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg64Fast(1234UL);
        var rng2 = new Pcg64Fast(1234UL);

        // Generate 256 values sequentially
        for (int i = 0; i < 256; i++)
        {
            rng1.Next();
        }

        // Advance by 256 steps
        rng2.Advance(256UL);

        // Now they should produce the same next values
        for (int i = 0; i < 64; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Pcg64OneSeq_DefaultConstructor_GeneratesValues()
    {
        var rng = new Pcg64OneSeq();
        ulong value = rng.Next();
        Assert.InRange(value, Pcg64OneSeq.MinValue, Pcg64OneSeq.MaxValue);
    }

    [Fact]
    public void Pcg64Unique_InstancesHaveDifferentSequences()
    {
        var rng1 = new Pcg64Unique(42UL);
        var rng2 = new Pcg64Unique(42UL);

        ulong[] seq1 = new ulong[16];
        ulong[] seq2 = new ulong[16];

        for (int i = 0; i < 16; i++)
        {
            seq1[i] = rng1.Next();
            seq2[i] = rng2.Next();
        }

        bool anyDifferent = false;
        for (int i = 0; i < 16; i++)
        {
            if (seq1[i] != seq2[i])
            {
                anyDifferent = true;
                break;
            }
        }

        Assert.True(anyDifferent);
    }

    [Fact]
    public void Pcg64Unique_CopyProducesSameSequence()
    {
        var rng1 = new Pcg64Unique(42UL);
        var rng2 = rng1;

        ulong[] seq1 = new ulong[16];
        ulong[] seq2 = new ulong[16];

        for (int i = 0; i < 16; i++)
        {
            seq1[i] = rng1.Next();
            seq2[i] = rng2.Next();
        }

        Assert.Equal(seq1, seq2);
    }

    [Fact]
    public void Pcg64Unique_Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg64Unique(42UL);
        var rng2 = rng1;

        for (int i = 0; i < 512; i++)
        {
            rng1.Next();
        }

        rng2.Advance(512UL);

        for (int i = 0; i < 128; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Pcg64Unique_Backstep_ReversesAdvance()
    {
        var rng = new Pcg64Unique(42UL);

        ulong first = rng.Next();
        ulong second = rng.Next();
        ulong third = rng.Next();

        rng.Backstep(3UL);

        Assert.Equal(first, rng.Next());
        Assert.Equal(second, rng.Next());
        Assert.Equal(third, rng.Next());
    }
}
