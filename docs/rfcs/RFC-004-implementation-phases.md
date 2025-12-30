# RFC-004: Implementation Phases

**Status:** Draft  
**Created:** 2024-12-02  
**Author:** Cascade AI

## Summary

This RFC outlines the phased implementation approach for porting PCG RNG to .NET.

## Phase Overview

```
Phase 0: Project Setup           [Week 1]
    ↓
Phase 1: Core Infrastructure     [Week 1-2]
    ↓
Phase 2: pcg32 Implementation    [Week 2-3]
    ↓
Phase 3: Oracle Test Setup       [Week 3]
    ↓
Phase 4: pcg64 Implementation    [Week 4]
    ↓
Phase 5: Variants & Extensions   [Week 5-6]
    ↓
Phase 6: Polish & Package        [Week 6-7]
```

## Phase 0: Project Setup

### Goals
- Create solution and project structure
- Set up build configuration
- Configure CI/CD pipeline

### Deliverables

1. **Solution Structure**
   ```
   dotnet/
   ├── Pcg.sln
   ├── src/Pcg/Pcg.csproj
   ├── tests/Pcg.Tests/Pcg.Tests.csproj
   └── tests/Pcg.Oracle.Tests/Pcg.Oracle.Tests.csproj
   ```

2. **Project Files**
   ```xml
   <!-- src/Pcg/Pcg.csproj -->
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFrameworks>net8.0;netstandard2.1</TargetFrameworks>
       <LangVersion>12.0</LangVersion>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
       
       <PackageId>Pcg.Random</PackageId>
       <Version>0.1.0</Version>
       <Authors>Your Name</Authors>
       <Description>PCG Random Number Generator for .NET</Description>
       <PackageLicenseExpression>MIT</PackageLicenseExpression>
       <PackageProjectUrl>https://github.com/...</PackageProjectUrl>
       <RepositoryUrl>https://github.com/...</RepositoryUrl>
       <PackageTags>random;rng;pcg;prng</PackageTags>
     </PropertyGroup>
   </Project>
   ```

3. **GitHub Actions CI**
   ```yaml
   # .github/workflows/ci.yml
   name: CI
   on: [push, pull_request]
   jobs:
     build-and-test:
       runs-on: ${{ matrix.os }}
       strategy:
         matrix:
           os: [ubuntu-latest, windows-latest, macos-latest]
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '8.0.x'
         - run: dotnet build dotnet/Pcg.sln
         - run: dotnet test dotnet/Pcg.sln
   ```

### Exit Criteria
- [ ] Solution builds on all target platforms
- [ ] Empty test project runs
- [ ] CI pipeline runs on push

---

## Phase 1: Core Infrastructure

### Goals
- Implement 128-bit integer support
- Implement output functions
- Implement advance algorithm
- Define constants

### Deliverables

1. **UInt128 Support** (`Internal/UInt128.cs`)
   - For netstandard2.1 compatibility
   - Use System.UInt128 for net7.0+
   
2. **Output Functions** (`Internal/OutputFunctions.cs`)
   ```csharp
   internal static class OutputFunctions
   {
       public static uint XshRr(ulong state);      // pcg32
       public static uint XshRs(ulong state);      // pcg32_fast
       public static ulong XslRr(UInt128 state);   // pcg64
       public static uint RxsMXs32(uint state);    // insecure variants
       public static ulong RxsMXs64(ulong state);
   }
   ```

3. **Constants** (`Internal/Constants.cs`)
   ```csharp
   internal static class PcgConstants
   {
       // Multipliers
       public const ulong Mult64 = 6364136223846793005UL;
       public static readonly UInt128 Mult128 = ...;
       
       // Default increments
       public const ulong Inc64 = 1442695040888963407UL;
       public static readonly UInt128 Inc128 = ...;
   }
   ```

4. **Advance Algorithm** (`Internal/PcgMath.cs`)
   ```csharp
   internal static class PcgMath
   {
       public static ulong Advance(ulong state, ulong delta, 
                                   ulong mult, ulong inc);
       public static UInt128 Advance(UInt128 state, UInt128 delta,
                                     UInt128 mult, UInt128 inc);
   }
   ```

### Unit Tests
- Output function tests with known inputs/outputs
- Advance algorithm tests
- UInt128 arithmetic tests

### Exit Criteria
- [ ] All output functions implemented and tested
- [ ] Advance algorithm implemented and tested
- [ ] UInt128 operations working

---

## Phase 2: pcg32 Implementation

### Goals
- Implement `Pcg32` (setseq_xsh_rr_64_32)
- Implement `Pcg32OneSeq`
- Implement `Pcg32Fast` (MCG)

### Deliverables

1. **Pcg32** (`Generators/Pcg32.cs`)
   ```csharp
   public struct Pcg32 : IPcgRng<uint>, ISettableStream<ulong>
   {
       public Pcg32();
       public Pcg32(ulong seed);
       public Pcg32(ulong seed, ulong stream);
       
       public uint Next();
       public uint Next(uint upperBound);
       public void Advance(ulong delta);
       public void Backstep(ulong delta);
       
       public ulong Stream { get; }
       public void SetStream(ulong stream);
   }
   ```

2. **Pcg32OneSeq** (`Generators/Pcg32OneSeq.cs`)
   - Fixed stream variant
   
3. **Pcg32Fast** (`Generators/Pcg32Fast.cs`)
   - MCG variant (no increment)

### Unit Tests
- Constructor tests
- Sequence generation tests
- Advance/backstep tests
- Stream behavior tests

### Exit Criteria
- [ ] All Pcg32 variants implemented
- [ ] Basic unit tests passing
- [ ] API matches design spec

---

## Phase 3: Oracle Test Setup

### Goals
- Create C++ oracle generator
- Generate initial test data
- Create .NET oracle test infrastructure

### Deliverables

1. **Oracle Generator** (`tools/OracleGenerator/`)
   - CMake project
   - Generates JSON test data
   - Covers all test scenarios from RFC-003

2. **Generated Test Data** (`tests/Pcg.Oracle.Tests/OracleData/`)
   ```
   pcg32.json
   pcg32_oneseq.json
   pcg32_fast.json
   ```

3. **Oracle Test Framework**
   ```csharp
   public class Pcg32OracleTests
   {
       [Theory]
       [MemberData(nameof(GetPcg32TestCases))]
       public void Pcg32_MatchesOracle(OracleTestCase testCase);
   }
   ```

### Exit Criteria
- [ ] Oracle generator builds and runs
- [ ] Test data generated for pcg32 variants
- [ ] Oracle tests pass for pcg32

---

## Phase 4: pcg64 Implementation

### Goals
- Implement `Pcg64` (setseq_xsl_rr_128_64)
- Implement `Pcg64OneSeq`
- Implement `Pcg64Fast`

### Deliverables

1. **Pcg64** (`Generators/Pcg64.cs`)
   ```csharp
   public struct Pcg64 : IPcgRng<ulong>, ISettableStream<UInt128>
   {
       public Pcg64();
       public Pcg64(UInt128 seed);
       public Pcg64(UInt128 seed, UInt128 stream);
       
       // Convenience overloads
       public Pcg64(ulong seedLo, ulong seedHi);
       public Pcg64(ulong seedLo, ulong seedHi, ulong streamLo, ulong streamHi);
       
       public ulong Next();
       // ...
   }
   ```

2. **Pcg64 Variants**
   - Pcg64OneSeq
   - Pcg64Fast

3. **Oracle Test Data**
   - Generate and add pcg64 test vectors

### Exit Criteria
- [ ] All Pcg64 variants implemented
- [ ] Oracle tests pass for pcg64
- [ ] 128-bit arithmetic verified correct

---

## Phase 5: Variants & Extensions

### Goals
- Implement remaining variants
- Implement extended generators (if time permits)
- Implement utility methods

### Deliverables

1. **Unique Stream Variants**
   - Pcg32Unique
   - Pcg64Unique

2. **Insecure Variants** (RXS M XS output function)
   - Pcg32OnceInsecure
   - Pcg64OnceInsecure

3. **Extension Methods** (`Extensions/PcgExtensions.cs`)
   ```csharp
   public static class PcgExtensions
   {
       public static void Shuffle<T>(this ref Pcg32 rng, Span<T> span);
       public static double NextDouble(this ref Pcg32 rng);
       public static float NextFloat(this ref Pcg32 rng);
   }
   ```

4. **System.Random Compatibility**
   ```csharp
   public sealed class PcgRandom : Random
   {
       public PcgRandom();
       public PcgRandom(int seed);
       public PcgRandom(Pcg32 rng);
   }
   ```

### Exit Criteria
- [ ] Unique variants implemented
- [ ] Extension methods implemented
- [ ] System.Random wrapper works

---

## Phase 6: Polish & Package

### Goals
- Documentation
- Performance optimization
- NuGet package preparation

### Deliverables

1. **Documentation**
   - XML documentation comments on all public APIs
   - README.md with usage examples
   - API reference generation

2. **Performance**
   - Benchmark suite
   - Optimization passes
   - Comparison with System.Random

3. **Package**
   - NuGet package configuration
   - License file
   - Release notes

4. **CI/CD**
   - Automated package publishing
   - Version tagging

### Exit Criteria
- [ ] All public APIs documented
- [ ] Performance benchmarks complete
- [ ] Package publishes successfully
- [ ] README has clear usage examples

---

## Risk Mitigation

### Risk: 128-bit Performance on 32-bit Platforms
- **Mitigation**: Use UInt128 carefully, provide Pcg32 as primary recommendation for 32-bit targets

### Risk: Floating-point Conversion Edge Cases
- **Mitigation**: Extensive testing of NextDouble/NextFloat against known values

### Risk: Endianness Issues
- **Mitigation**: All tests run on both little and big endian (CI matrix)

### Risk: API Breaking Changes
- **Mitigation**: Mark as 0.x version until API is stable

---

## Timeline Summary

| Phase | Description | Duration | Dependencies |
|-------|-------------|----------|--------------|
| 0 | Project Setup | 2-3 days | None |
| 1 | Core Infrastructure | 3-4 days | Phase 0 |
| 2 | pcg32 Implementation | 3-4 days | Phase 1 |
| 3 | Oracle Test Setup | 2-3 days | Phase 2 |
| 4 | pcg64 Implementation | 3-4 days | Phase 1, 3 |
| 5 | Variants & Extensions | 4-5 days | Phase 2, 4 |
| 6 | Polish & Package | 3-4 days | All |

**Total: ~4-6 weeks** (part-time development)

## Related RFCs

- RFC-001: Project Overview and Scope
- RFC-002: Architecture and Design
- RFC-003: Oracle Testing Strategy
