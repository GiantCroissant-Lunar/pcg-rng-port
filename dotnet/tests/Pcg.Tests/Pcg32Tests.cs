using Xunit;

namespace Pcg.Tests;

public class Pcg32Tests
{
    [Fact]
    public void DefaultConstructor_CreatesValidGenerator()
    {
        var rng = new Pcg32();
        
        // Should be able to generate values
        uint value = rng.Next();
        Assert.True(value >= Pcg32.MinValue);
        Assert.True(value <= Pcg32.MaxValue);
    }

    [Fact]
    public void SeededConstructor_ProducesConsistentSequence()
    {
        var rng1 = new Pcg32(42, 54);
        var rng2 = new Pcg32(42, 54);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var rng1 = new Pcg32(1, 1);
        var rng2 = new Pcg32(2, 1);

        // Very unlikely to be equal
        Assert.NotEqual(rng1.Next(), rng2.Next());
    }

    [Fact]
    public void DifferentStreams_ProduceDifferentSequences()
    {
        var rng1 = new Pcg32(42, 1);
        var rng2 = new Pcg32(42, 2);

        // Same seed, different stream = different sequence
        Assert.NotEqual(rng1.Next(), rng2.Next());
    }

    [Fact]
    public void BoundedNext_StaysWithinBounds()
    {
        var rng = new Pcg32(42, 54);

        for (int i = 0; i < 10000; i++)
        {
            uint value = rng.Next(100);
            Assert.True(value < 100);
        }
    }

    [Fact]
    public void Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg32(42, 54);
        var rng2 = new Pcg32(42, 54);

        // Generate 1000 values sequentially
        for (int i = 0; i < 1000; i++)
        {
            rng1.Next();
        }

        // Advance by 1000
        rng2.Advance(1000);

        // Should now produce the same values
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Backstep_ReversesAdvance()
    {
        var rng = new Pcg32(42, 54);
        
        uint first = rng.Next();
        uint second = rng.Next();
        uint third = rng.Next();

        rng.Backstep(3);

        Assert.Equal(first, rng.Next());
        Assert.Equal(second, rng.Next());
        Assert.Equal(third, rng.Next());
    }

    [Fact]
    public void Distance_CalculatesCorrectly()
    {
        var rng1 = new Pcg32(42, 54);
        var rng2 = new Pcg32(42, 54);

        // Advance rng2 by known amount
        for (int i = 0; i < 100; i++)
        {
            rng2.Next();
        }

        // Distance should be 100
        Assert.Equal(100ul, rng2 - rng1);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var rng1 = new Pcg32(42, 54);
        var rng2 = new Pcg32(42, 54);
        var rng3 = new Pcg32(99, 54);

        Assert.Equal(rng1, rng2);
        Assert.NotEqual(rng1, rng3);
    }

    [Fact]
    public void SetStream_ChangesSequence()
    {
        // Verify that different streams produce different sequences
        // by comparing many values - they should differ at some point
        var rng1 = new Pcg32(42, 1);
        var rng2 = new Pcg32(42, 1);

        // Change stream on rng2 before generating anything
        rng2.SetStream(999);

        // Generate some values from each
        var values1 = new List<uint>();
        var values2 = new List<uint>();
        
        for (int i = 0; i < 10; i++)
        {
            values1.Add(rng1.Next());
            values2.Add(rng2.Next());
        }

        // The sequences should be different (at least one value differs)
        Assert.NotEqual(values1, values2);
    }

    [Fact]
    public void Stream_ReturnsCorrectValue()
    {
        var rng = new Pcg32(42, 54);
        Assert.Equal(54ul, rng.Stream);

        rng.SetStream(100);
        Assert.Equal(100ul, rng.Stream);
    }

    [Fact]
    public void Pcg32Unique_InstancesHaveDifferentSequences()
    {
        var rng1 = new Pcg32Unique(42);
        var rng2 = new Pcg32Unique(42);

        uint[] seq1 = new uint[10];
        uint[] seq2 = new uint[10];

        for (int i = 0; i < 10; i++)
        {
            seq1[i] = rng1.Next();
            seq2[i] = rng2.Next();
        }

        bool anyDifferent = false;
        for (int i = 0; i < 10; i++)
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
    public void Pcg32Unique_CopyProducesSameSequence()
    {
        var rng1 = new Pcg32Unique(42);
        var rng2 = rng1;

        uint[] seq1 = new uint[10];
        uint[] seq2 = new uint[10];

        for (int i = 0; i < 10; i++)
        {
            seq1[i] = rng1.Next();
            seq2[i] = rng2.Next();
        }

        Assert.Equal(seq1, seq2);
    }

    [Fact]
    public void Pcg32Unique_Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg32Unique(42);
        var rng2 = rng1;

        for (int i = 0; i < 1000; i++)
        {
            rng1.Next();
        }

        rng2.Advance(1000);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Pcg32Unique_Backstep_ReversesAdvance()
    {
        var rng = new Pcg32Unique(42);

        uint first = rng.Next();
        uint second = rng.Next();
        uint third = rng.Next();

        rng.Backstep(3);

        Assert.Equal(first, rng.Next());
        Assert.Equal(second, rng.Next());
        Assert.Equal(third, rng.Next());
    }
}
