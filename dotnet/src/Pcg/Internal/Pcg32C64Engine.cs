using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg32C64Engine : IEquatable<Pcg32C64Engine>
{
    private Pcg32Engine _base;
    private readonly uint[] _data;

    // pcg32_c64 = ext_setseq_xsh_rr_64_32<6,16,false>
    private const int TablePow2 = 6;
    private const int AdvancePow2 = 16;

    public Pcg32C64Engine(ulong seed, ulong stream)
    {
        _base = new Pcg32Engine(seed, stream);
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
        // For kdd = false extended generators, the C++ reference does not
        // provide an efficient analytic advance. To ensure that Advance
        // behaves exactly like calling Next() delta times (including all
        // table ticks), we perform a simple loop.

        for (ulong i = 0; i < delta; i++)
        {
            Next();
        }
    }

    public void Backstep(ulong delta)
    {
        // The C++ pcg32_c64 extended generator does not define an analytic
        // backstep/advance for the extended state. Exposing a "best effort"
        // implementation here would be misleading, so we fail fast instead.
        throw new NotSupportedException("Backstep is not supported for Pcg32C64 (pcg32_c64).");
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
            kdd: false);
    }

    public bool Equals(Pcg32C64Engine other)
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
        return obj is Pcg32C64Engine other && Equals(other);
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
