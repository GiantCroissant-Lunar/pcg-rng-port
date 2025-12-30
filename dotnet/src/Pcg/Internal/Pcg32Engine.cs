using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg32Engine : IPcg32ExtendedBase, IEquatable<Pcg32Engine>
{
    private ulong _state;
    private ulong _inc;  // Stream value (must be odd)

    public Pcg32Engine(ulong seed, ulong stream)
    {
        // Ensure increment is odd (matches C++ stream_mixin initialization)
        _inc = (stream << 1) | 1;

        // Initialize state: state_ = bump(state + increment())
        // This matches the C++ two-argument constructor exactly
        _state = unchecked(seed + _inc);
        _state = Bump(_state);
    }

    public ulong Stream => _inc >> 1;

    public ulong State
    {
        readonly get => _state;
        set => _state = value;
    }

    public ulong Increment => _inc;

    public bool IsMcg => false;

    public void SetStream(ulong stream)
    {
        _inc = (stream << 1) | 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(_state);
        return OutputFunctions.XshRr(oldState);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next(uint upperBound)
    {
        // Threshold for rejection sampling
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
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier64, _inc);
    }

    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    public readonly ulong Distance(Pcg32Engine other)
    {
        if (_inc != other._inc)
        {
            throw new ArgumentException("Cannot calculate distance between generators with different streams", nameof(other));
        }

        return PcgMath.Distance(_state, other._state, PcgConstants.Multiplier64, _inc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ulong Bump(ulong state)
    {
        return state * PcgConstants.Multiplier64 + _inc;
    }

    public readonly bool Equals(Pcg32Engine other)
    {
        return _state == other._state && _inc == other._inc;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32Engine other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }
}
