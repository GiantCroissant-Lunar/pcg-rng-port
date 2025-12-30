using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Pcg;
using System;

namespace Pcg.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(new[]
        {
            typeof(Pcg32Benchmarks),
            typeof(Pcg32ExtendedBenchmarks),
            typeof(Pcg64Benchmarks),
            typeof(Pcg64ExtendedBenchmarks)
        });
    }
}

[MemoryDiagnoser]
public class Pcg32Benchmarks
{
    private Pcg32 _pcg;
    private Random _random;
    private byte[] _buffer;

    [GlobalSetup]
    public void Setup()
    {
        _pcg = new Pcg32(42UL, 54UL);
        _random = new Random(42);
        _buffer = new byte[256];
    }

    [Benchmark]
    public uint Pcg32_Next() => _pcg.Next();

    [Benchmark]
    public int Random_Next() => _random.Next();

    [Benchmark]
    public uint Pcg32_Next_Bounded() => _pcg.Next(100u);

    [Benchmark]
    public int Random_Next_Bounded() => _random.Next(100);

    [Benchmark]
    public double Pcg32_NextDouble() => _pcg.NextDouble();

    [Benchmark]
    public double Random_NextDouble() => _random.NextDouble();

    [Benchmark]
    public void Pcg32_NextBytes() => _pcg.NextBytes(_buffer);

    [Benchmark]
    public void Random_NextBytes() => _random.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _pcg.Shuffle(values);
    }

    [Benchmark]
    public void Random_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        // Fisher–Yates shuffle using System.Random
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}

[MemoryDiagnoser]
public class Pcg64ExtendedBenchmarks
{
    private Pcg64K32Oneseq _k32Oneseq;
    private Pcg64K32Fast _k32Fast;
    private Pcg64C32Oneseq _c32Oneseq;
    private Pcg64C32Fast _c32Fast;
    private byte[] _buffer;

    [GlobalSetup]
    public void Setup()
    {
        _k32Oneseq = new Pcg64K32Oneseq(42UL);
        _k32Fast = new Pcg64K32Fast(42UL);
        _c32Oneseq = new Pcg64C32Oneseq(42UL);
        _c32Fast = new Pcg64C32Fast(42UL);
        _buffer = new byte[256];
    }

    [Benchmark]
    public ulong Pcg64K32Oneseq_Next() => _k32Oneseq.Next();

    [Benchmark]
    public ulong Pcg64K32Fast_Next() => _k32Fast.Next();

    [Benchmark]
    public ulong Pcg64C32Oneseq_Next() => _c32Oneseq.Next();

    [Benchmark]
    public ulong Pcg64C32Fast_Next() => _c32Fast.Next();

    [Benchmark]
    public ulong Pcg64K32Oneseq_Next_Bounded() => _k32Oneseq.Next(100UL);

    [Benchmark]
    public ulong Pcg64K32Fast_Next_Bounded() => _k32Fast.Next(100UL);

    [Benchmark]
    public ulong Pcg64C32Oneseq_Next_Bounded() => _c32Oneseq.Next(100UL);

    [Benchmark]
    public ulong Pcg64C32Fast_Next_Bounded() => _c32Fast.Next(100UL);

    [Benchmark]
    public double Pcg64K32Oneseq_NextDouble() => _k32Oneseq.NextDouble();

    [Benchmark]
    public double Pcg64K32Fast_NextDouble() => _k32Fast.NextDouble();

    [Benchmark]
    public double Pcg64C32Oneseq_NextDouble() => _c32Oneseq.NextDouble();

    [Benchmark]
    public double Pcg64C32Fast_NextDouble() => _c32Fast.NextDouble();

    [Benchmark]
    public void Pcg64K32Oneseq_NextBytes() => _k32Oneseq.NextBytes(_buffer);

    [Benchmark]
    public void Pcg64K32Fast_NextBytes() => _k32Fast.NextBytes(_buffer);

    [Benchmark]
    public void Pcg64C32Oneseq_NextBytes() => _c32Oneseq.NextBytes(_buffer);

    [Benchmark]
    public void Pcg64C32Fast_NextBytes() => _c32Fast.NextBytes(_buffer);

    [Benchmark]
    public void Pcg64K32Oneseq_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _k32Oneseq.Shuffle(values);
    }

    [Benchmark]
    public void Pcg64K32Fast_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _k32Fast.Shuffle(values);
    }

    [Benchmark]
    public void Pcg64C32Oneseq_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _c32Oneseq.Shuffle(values);
    }

    [Benchmark]
    public void Pcg64C32Fast_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _c32Fast.Shuffle(values);
    }
}

[MemoryDiagnoser]
public class Pcg32ExtendedBenchmarks
{
    private Pcg32 _pcg;
    private Pcg32Fast _fast;
    private Pcg32K64 _k64;
    private Pcg32K64Oneseq _k64Oneseq;
    private Pcg32K64Fast _k64Fast;
    private Pcg32C64 _c64;
    private Pcg32C64Oneseq _c64Oneseq;
    private Pcg32C64Fast _c64Fast;
    private byte[] _buffer;

    [GlobalSetup]
    public void Setup()
    {
        _pcg = new Pcg32(42UL, 54UL);
        _fast = new Pcg32Fast(42UL);
        _k64 = new Pcg32K64(42UL, 54UL);
        _k64Oneseq = new Pcg32K64Oneseq(42UL);
        _k64Fast = new Pcg32K64Fast(42UL);
        _c64 = new Pcg32C64(42UL, 54UL);
        _c64Oneseq = new Pcg32C64Oneseq(42UL);
        _c64Fast = new Pcg32C64Fast(42UL);
        _buffer = new byte[256];
    }

    [Benchmark]
    public uint Pcg32_Next() => _pcg.Next();

    [Benchmark]
    public uint Pcg32Fast_Next() => _fast.Next();

    [Benchmark]
    public uint Pcg32K64_Next() => _k64.Next();

    [Benchmark]
    public uint Pcg32K64Oneseq_Next() => _k64Oneseq.Next();

    [Benchmark]
    public uint Pcg32K64Fast_Next() => _k64Fast.Next();

    [Benchmark]
    public uint Pcg32C64_Next() => _c64.Next();

    [Benchmark]
    public uint Pcg32C64Oneseq_Next() => _c64Oneseq.Next();

    [Benchmark]
    public uint Pcg32C64Fast_Next() => _c64Fast.Next();

    [Benchmark]
    public uint Pcg32_Next_Bounded() => _pcg.Next(100u);

    [Benchmark]
    public uint Pcg32Fast_Next_Bounded() => _fast.Next(100u);

    [Benchmark]
    public uint Pcg32K64_Next_Bounded() => _k64.Next(100u);

    [Benchmark]
    public uint Pcg32K64Oneseq_Next_Bounded() => _k64Oneseq.Next(100u);

    [Benchmark]
    public uint Pcg32K64Fast_Next_Bounded() => _k64Fast.Next(100u);

    [Benchmark]
    public uint Pcg32C64_Next_Bounded() => _c64.Next(100u);

    [Benchmark]
    public uint Pcg32C64Oneseq_Next_Bounded() => _c64Oneseq.Next(100u);

    [Benchmark]
    public uint Pcg32C64Fast_Next_Bounded() => _c64Fast.Next(100u);

    [Benchmark]
    public double Pcg32_NextDouble() => _pcg.NextDouble();

    [Benchmark]
    public double Pcg32Fast_NextDouble() => _fast.NextDouble();

    [Benchmark]
    public double Pcg32K64_NextDouble() => _k64.NextDouble();

    [Benchmark]
    public double Pcg32K64Oneseq_NextDouble() => _k64Oneseq.NextDouble();

    [Benchmark]
    public double Pcg32K64Fast_NextDouble() => _k64Fast.NextDouble();

    [Benchmark]
    public double Pcg32C64_NextDouble() => _c64.NextDouble();

    [Benchmark]
    public double Pcg32C64Oneseq_NextDouble() => _c64Oneseq.NextDouble();

    [Benchmark]
    public double Pcg32C64Fast_NextDouble() => _c64Fast.NextDouble();

    [Benchmark]
    public void Pcg32_NextBytes() => _pcg.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32Fast_NextBytes() => _fast.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32K64_NextBytes() => _k64.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32K64Oneseq_NextBytes() => _k64Oneseq.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32K64Fast_NextBytes() => _k64Fast.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32C64_NextBytes() => _c64.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32C64Oneseq_NextBytes() => _c64Oneseq.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32C64Fast_NextBytes() => _c64Fast.NextBytes(_buffer);

    [Benchmark]
    public void Pcg32_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _pcg.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32Fast_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _fast.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32K64_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _k64.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32K64Oneseq_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _k64Oneseq.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32K64Fast_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _k64Fast.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32C64_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _c64.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32C64Oneseq_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _c64Oneseq.Shuffle(values);
    }

    [Benchmark]
    public void Pcg32C64Fast_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _c64Fast.Shuffle(values);
    }
}

[MemoryDiagnoser]
public class Pcg64Benchmarks
{
    private Pcg64 _pcg;
    private Random _random;
    private byte[] _buffer;

    [GlobalSetup]
    public void Setup()
    {
        _pcg = new Pcg64(42UL, 54UL);
        _random = new Random(42);
        _buffer = new byte[256];
    }

    [Benchmark]
    public ulong Pcg64_Next() => _pcg.Next();

    [Benchmark]
    public int Random_Next() => _random.Next();

    [Benchmark]
    public ulong Pcg64_Next_Bounded() => _pcg.Next(100UL);

    [Benchmark]
    public int Random_Next_Bounded() => _random.Next(100);

    [Benchmark]
    public double Pcg64_NextDouble() => _pcg.NextDouble();

    [Benchmark]
    public double Random_NextDouble() => _random.NextDouble();

    [Benchmark]
    public void Pcg64_NextBytes()
    {
        // Adapt 64-bit RNG to bytes
        _pcg.NextBytes(_buffer);
    }

    [Benchmark]
    public void Pcg64_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        _pcg.Shuffle(values);
    }

    [Benchmark]
    public void Random_Shuffle()
    {
        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        // Fisher–Yates shuffle using System.Random
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}
