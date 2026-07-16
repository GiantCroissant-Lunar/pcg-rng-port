using Xunit;

namespace Pcg.Oracle.Tests;

/// <summary>
/// Cross-runtime compatibility tests proving that <see cref="Pcg32OneSeq"/> can reproduce
/// the 32-bit output stream of the terrain-diffusion <c>portable_rng._pcg64_next</c>
/// reference (pure-Python PCG XSH-RR 64/32) when seeded via
/// <see cref="Pcg32OneSeq.FromRawState"/>.
/// </summary>
public class Pcg32OneSeqPortableRngCompatTests
{
    // PCG XSH-RR 64/32 LCG constants. Identical in all three places:
    //   - Pcg.Internal.PcgConstants.Multiplier64 / DefaultIncrement64
    //   - terrain_diffusion/inference/portable_rng.py PCG64_MULT / PCG64_INC
    //   - the official pcg-cpp reference.
    private const ulong Mult = 6364136223846793005UL;
    private const ulong Inc = 1442695040888963407UL;

    // The first 8 32-bit outputs of portable_rng._pcg64_next for each seed.
    // Generated with the throwaway pure-Python script reproduced below (no numba/numpy
    // needed: _pcg64_next is self-contained). Reproduce locally with:
    //
    //   python3 - <<'PY'
    //   MULT=6364136223846793005; INC=1442695040888963407; M=0xFFFFFFFFFFFFFFFF
    //   def nxt(s):
    //       s=(s*MULT+INC)&M
    //       x=(((s>>18)^s)>>27)&0xFFFFFFFF
    //       rot=s>>59
    //       return s,((x>>rot)|(x<<((32-rot)&31)))&0xFFFFFFFF
    //   for label,seed in [("1",1),("42",42),("1337",1337),
    //                      ("0x123456789ABCDEF0",0x123456789ABCDEF0),
    //                      ("0xFFFFFFFFFFFFFFFF",0xFFFFFFFFFFFFFFFF)]:
    //       s=seed&M; outs=[]
    //       for _ in range(8): s,o=nxt(s); outs.append(o)
    //       print(label, [f"0x{o:08x}" for o in outs])
    //   PY
    public static IEnumerable<object[]> GoldenSeeds => new[]
    {
        new object[]
        {
            1UL,
            new uint[]
            {
                0xb99c5774, 0xc5344d14, 0x91cf3bf5, 0x9b84069a,
                0x2445c23a, 0x3dd00434, 0x64745ce9, 0x05faae15,
            },
        },
        new object[]
        {
            42UL,
            new uint[]
            {
                0x75830bbd, 0x0e6dfdb2, 0xce19afdf, 0xd8cfe2c3,
                0x012b061e, 0xd6de3682, 0xb2439ac1, 0x4e0e85d9,
            },
        },
        new object[]
        {
            1337UL,
            new uint[]
            {
                0x466026a8, 0x76a68cbc, 0x0a029e31, 0x3a78cbd4,
                0x55375c97, 0x0f6180d0, 0x8ed5b47b, 0x1595f361,
            },
        },
        new object[]
        {
            0x123456789ABCDEF0UL,
            new uint[]
            {
                0x7b75f3c1, 0x2c1f919a, 0x7ec843f4, 0x71f83b2e,
                0x29019ea8, 0x57153f69, 0x06a8626d, 0x8e130446,
            },
        },
        new object[]
        {
            0xFFFFFFFFFFFFFFFFUL,
            new uint[]
            {
                0xea6f52ec, 0xbc82e8d4, 0xd57bff22, 0xdfd368fa,
                0x32074c80, 0x6d1d9c1d, 0x5c1ccac9, 0x689fcb07,
            },
        },
    };

    // portable_rng: state0 = seed (raw, no ritual); each step ADVANCES then EMITS.
    // Pcg32OneSeq.Next(): EMITS from current state then advances.
    // Loading state = seed*MULT+INC (one portable advance from the raw seed) realigns the
    // streams: the first Next() emits XshRr(seed*MULT+INC) == portable out[0].
    private static Pcg32OneSeq CreatePortableCompat(ulong seed) =>
        Pcg32OneSeq.FromRawState(unchecked(seed * Mult + Inc));

    [Theory]
    [MemberData(nameof(GoldenSeeds))]
    public void FromRawStatePortableCompat_ReproducesPortableRngStream(ulong seed, uint[] expected)
    {
        var rng = CreatePortableCompat(seed);

        var actual = new uint[expected.Length];
        for (int i = 0; i < expected.Length; i++)
        {
            actual[i] = rng.Next();
        }

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RegularConstructor_DoesNotMatchPortableStream_Seed42()
    {
        // Negative control: the canonical PCG seed ritual (bump(seed + increment)) produces a
        // different stream from portable_rng's raw-seed convention for the same seed value.
        // This documents that matching LCG constants alone do NOT yield identical streams.
        var rng = new Pcg32OneSeq(42);
        var portable = CreatePortableCompat(42);

        var ritual = new uint[8];
        var compat = new uint[8];
        for (int i = 0; i < 8; i++)
        {
            ritual[i] = rng.Next();
            compat[i] = portable.Next();
        }

        Assert.NotEqual(compat, ritual);
    }

    [Fact]
    public void FromRawState_BypassesSeedingRitual()
    {
        // FromRawState(x).Next() emits XshRr(x); the constructor never applies bump(x + inc).
        // Feeding the ritual-initialized state through FromRawState must therefore match a
        // normally-constructed generator value-for-value.
        ulong seed = 42;
        var ritual = new Pcg32OneSeq(seed);

        var fromRaw = Pcg32OneSeq.FromRawState(unchecked((seed + Inc) * Mult + Inc));

        for (int i = 0; i < 8; i++)
        {
            Assert.Equal(ritual.Next(), fromRaw.Next());
        }
    }
}
