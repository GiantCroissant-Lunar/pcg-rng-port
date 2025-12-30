using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg64C32Engine : IEquatable<Pcg64C32Engine>
{
    private UInt128 _state;
    private UInt128 _inc;
    private readonly ulong[] _data;

    private const int TablePow2 = 5;
    private const int AdvancePow2 = 16;
    private const int TableSize = 1 << TablePow2;
    private const int TableShift = 128 - TablePow2;
    private const int TickShift = 128 - AdvancePow2;

    public Pcg64C32Engine(ulong seed, ulong stream)
    {
        _state = default;
        _inc = default;
        _data = new ulong[TableSize];

        InitBase(seed, stream);
        SelfInit();
    }

    private void InitBase(ulong seed, ulong stream)
    {
        var seed128 = new UInt128(0, seed);
        var stream128 = new UInt128(0, stream);

        _inc = (stream128 << 1) | UInt128.One;
        _state = seed128 + _inc;
        _state = Bump(_state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UInt128 Bump(UInt128 state)
    {
        return state * PcgConstants.Multiplier128 + _inc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextBase()
    {
        _state = _state * PcgConstants.Multiplier128 + _inc;
        return OutputFunctions.XslRr(_state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        ulong rhs = GetExtendedValue();
        ulong lhs = NextBase();
        return lhs ^ rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next(ulong upperBound)
    {
        ulong threshold = (ulong)(-(long)upperBound) % upperBound;

        ulong r;
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
        // The C++ pcg64_c32 extended generator does not define an analytic
        // backstep/advance for the extended state. Exposing a "best effort"
        // implementation here would be misleading, so we fail fast instead.
        throw new NotSupportedException("Backstep is not supported for Pcg64C32 (pcg64_c32).");
    }

    public void AdvanceBase(UInt128 delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier128, _inc);
    }

    public void AdvanceBase(ulong delta)
    {
        AdvanceBase(new UInt128(0, delta));
    }

    private void SelfInit()
    {
        ulong lhs = NextBase();
        ulong rhs = NextBase();
        ulong xdiff = lhs - rhs;

        for (int i = 0; i < _data.Length; i++)
        {
            _data[i] = NextBase() ^ xdiff;
        }
    }

    private void AdvanceTableSingleTick()
    {
        bool carry = false;

        for (int i = 0; i < _data.Length; i++)
        {
            if (carry)
            {
                carry = InsideOutRxsMXs64.ExternalStep(ref _data[i], (ulong)(i + 1));
            }

            bool carry2 = InsideOutRxsMXs64.ExternalStep(ref _data[i], (ulong)(i + 1));
            carry = carry || carry2;
        }
    }

    private ulong GetExtendedValue()
    {
        UInt128 state = _state;
        int index = (int)(ulong)(state >> TableShift);

        if ((state >> TickShift) == UInt128.Zero)
        {
            AdvanceTableSingleTick();
        }

        return _data[index];
    }

    public bool Equals(Pcg64C32Engine other)
    {
        if (_state != other._state || _inc != other._inc)
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
        return obj is Pcg64C32Engine other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(_state, _inc);
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
