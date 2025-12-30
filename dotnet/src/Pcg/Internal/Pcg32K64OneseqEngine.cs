using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg32K64OneseqEngine : IEquatable<Pcg32K64OneseqEngine>
{
    private Pcg32FastEngineCore _base;
    private readonly uint[] _data;

    // pcg32_k64_oneseq = ext_mcg_xsh_rs_64_32<6,32,true>
    private const int TablePow2 = 6;
    private const int AdvancePow2 = 32;
    private const ulong TableMask = (1UL << TablePow2) - 1UL;

    public Pcg32K64OneseqEngine(ulong seed)
    {
        _base = new Pcg32FastEngineCore(seed);
        _data = new uint[1 << TablePow2];
        SelfInit();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        uint rhs = GetExtendedValue();
        uint lhs = _base.Next();
        return lhs ^ rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next(uint upperBound)
    {
        uint threshold = (uint)(-(int)upperBound) % upperBound;

        uint r;
        do
        {
            r = Next();
        } while (r < threshold);

        return r % upperBound;
    }

    public void Advance(ulong delta)
    {
        Pcg32ExtendedKddHelper.Advance(
            ref _base,
            _data,
            TablePow2,
            AdvancePow2,
            kdd: true,
            distance: delta,
            forwards: true);
    }

    public void Backstep(ulong delta)
    {
        Pcg32ExtendedKddHelper.Advance(
            ref _base,
            _data,
            TablePow2,
            AdvancePow2,
            kdd: true,
            distance: delta,
            forwards: false);
    }

    private void SelfInit()
    {
        Pcg32ExtendedKddHelper.SelfInit(ref _base, _data);
    }

    private uint GetExtendedValue()
    {
        return Pcg32ExtendedKddHelper.GetExtendedValue(
            ref _base,
            _data,
            TablePow2,
            AdvancePow2,
            kdd: true);
    }

    public bool Equals(Pcg32K64OneseqEngine other)
    {
        if (!_base.Equals(other._base))
            return false;

        if (_data.Length != other._data.Length)
            return false;

        for (int i = 0; i < _data.Length; i++)
        {
            if (_data[i] != other._data[i])
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pcg32K64OneseqEngine other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = _base.GetHashCode();
        unchecked
        {
            for (int i = 0; i < _data.Length; i++)
            {
                hash = (hash * 397) ^ (int)_data[i];
            }
        }

        return hash;
    }
}
