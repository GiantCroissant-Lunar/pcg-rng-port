# RFC-003: Oracle Testing Strategy

**Status:** Draft  
**Created:** 2024-12-02  
**Author:** Cascade AI

## Summary

This RFC defines the strategy for oracle testing - comparing our .NET implementation against the C++ reference implementation to ensure correctness.

## Goals

1. **Bit-exact correctness**: For any given seed, produce identical output sequences
2. **Comprehensive coverage**: Test all generator types and operations
3. **Automated verification**: Tests run in CI without requiring C++ compilation
4. **Reproducibility**: Test data is version-controlled and regeneratable

## Oracle Testing Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Oracle Test Pipeline                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────────┐   │
│  │ C++ Reference │───▶│ Oracle Data  │◀───│ .NET Unit Tests  │   │
│  │  Generator    │    │  (JSON/bin)  │    │ (Compare output) │   │
│  └──────────────┘    └──────────────┘    └──────────────────┘   │
│         │                    │                     │             │
│         │ One-time           │ Committed           │ Every build │
│         │ generation         │ to repo             │             │
│         ▼                    ▼                     ▼             │
│  tools/OracleGenerator  tests/.../OracleData   dotnet test      │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## Test Data Format

### JSON Schema

```json
{
  "generator": "pcg32",
  "description": "setseq_xsh_rr_64_32 - Default 32-bit generator",
  "testCases": [
    {
      "name": "default_constructor",
      "seed": null,
      "stream": null,
      "expectedState": "0x...",
      "sequence": [
        { "call": 1, "value": "0xa15c02b7" },
        { "call": 2, "value": "0x7b47f409" }
      ]
    },
    {
      "name": "seeded_42_54",
      "seed": "42",
      "stream": "54",
      "sequence": [
        { "call": 1, "value": "0xa15c02b7" },
        { "call": 2, "value": "0x7b47f409" },
        { "call": 3, "value": "0xba1d3330" }
      ]
    },
    {
      "name": "advance_backstep",
      "seed": "42",
      "stream": "54",
      "operations": [
        { "op": "next", "count": 10 },
        { "op": "backstep", "delta": 5 },
        { "op": "next", "expected": ["0x83d2f293", "0xbfa4784b", "0xcbed606e"] }
      ]
    },
    {
      "name": "bounded_random",
      "seed": "42", 
      "stream": "54",
      "bounds": [
        { "bound": 6, "expected": [2, 3, 0, 0, 1, 1] },
        { "bound": 100, "expected": [71, 47, 20, ...] }
      ]
    }
  ]
}
```

### Binary Format (Optional, for large sequences)

```
Header:
  4 bytes: Magic "PCGO" (PCG Oracle)
  4 bytes: Version (1)
  4 bytes: Generator type enum
  8 bytes: Seed
  8 bytes: Stream
  4 bytes: Count of values
  
Data:
  N × 4/8 bytes: Generated values
```

## Test Categories

### Category 1: Basic Sequence Generation

Test that generators produce expected sequences for known seeds.

```csharp
[Theory]
[MemberData(nameof(GetOracleData), "pcg32")]
public void Pcg32_ProducesExpectedSequence(OracleTestCase testCase)
{
    var rng = testCase.CreateGenerator<Pcg32>();
    
    foreach (var expected in testCase.Sequence)
    {
        Assert.Equal(expected, rng.Next());
    }
}
```

Test seeds:
- Default constructor (matches C++ `pcg32 rng;`)
- `(42, 54)` - standard test seed from pcg-cpp
- `(0, 0)` - edge case
- `(ulong.MaxValue, ulong.MaxValue)` - edge case
- Random seeds for broader coverage

### Category 2: Advance/Backstep

Test jump operations maintain correctness.

```csharp
[Fact]
public void Advance_ThenGenerate_MatchesSequentialGeneration()
{
    var rng1 = new Pcg32(42, 54);
    var rng2 = new Pcg32(42, 54);
    
    // Sequential generation
    for (int i = 0; i < 1000; i++) rng1.Next();
    
    // Advance
    rng2.Advance(1000);
    
    // Should now produce same values
    for (int i = 0; i < 100; i++)
    {
        Assert.Equal(rng1.Next(), rng2.Next());
    }
}
```

Test cases:
- Advance by 1, 10, 1000, 1_000_000
- Backstep by same amounts
- Advance then backstep returns to same position
- Negative delta (wrapping)

### Category 3: Bounded Random

Test rejection sampling produces correct distribution bounds.

```csharp
[Theory]
[InlineData(6)]   // Dice
[InlineData(2)]   // Coin flip
[InlineData(52)]  // Cards
[InlineData(100)] // Percentage
public void BoundedRandom_StaysWithinBounds(uint bound)
{
    var rng = new Pcg32(42, 54);
    
    for (int i = 0; i < 10000; i++)
    {
        uint value = rng.Next(bound);
        Assert.True(value < bound);
    }
}
```

### Category 4: Stream Behavior

Test stream variants behave correctly.

```csharp
[Fact]
public void DifferentStreams_ProduceDifferentSequences()
{
    var rng1 = new Pcg32(42, 1);
    var rng2 = new Pcg32(42, 2);
    
    // Same seed, different stream = different output
    Assert.NotEqual(rng1.Next(), rng2.Next());
}

[Fact]
public void OneSeq_AlwaysUsesDefaultStream()
{
    var rng1 = new Pcg32OneSeq(42);
    var rng2 = new Pcg32OneSeq(42);
    
    // Same output
    Assert.Equal(rng1.Next(), rng2.Next());
}
```

### Category 5: Extended Generators

Test k-dimensionally equidistributed generators.

```csharp
[Theory]
[MemberData(nameof(GetOracleData), "pcg32_k2")]
public void Pcg32K2_MatchesOracle(OracleTestCase testCase)
{
    // ...
}
```

### Category 6: 128-bit Generators

Test pcg64 family with 128-bit state.

```csharp
[Theory]
[MemberData(nameof(GetOracleData), "pcg64")]
public void Pcg64_ProducesExpectedSequence(OracleTestCase testCase)
{
    var rng = new Pcg64(testCase.Seed128, testCase.Stream128);
    
    foreach (var expected in testCase.Sequence)
    {
        Assert.Equal(expected, rng.Next());
    }
}
```

## C++ Oracle Generator

### Source Code

```cpp
// tools/OracleGenerator/generate_oracle.cpp

#include <iostream>
#include <fstream>
#include <iomanip>
#include "pcg_random.hpp"
#include "nlohmann/json.hpp"

using json = nlohmann::json;

template<typename RNG>
json generate_sequence(RNG& rng, size_t count) {
    json sequence = json::array();
    for (size_t i = 0; i < count; i++) {
        std::ostringstream ss;
        ss << "0x" << std::hex << std::setfill('0') 
           << std::setw(sizeof(typename RNG::result_type) * 2) 
           << rng();
        sequence.push_back({{"call", i + 1}, {"value", ss.str()}});
    }
    return sequence;
}

int main() {
    json output;
    
    // pcg32 tests
    {
        json pcg32_data;
        pcg32_data["generator"] = "pcg32";
        pcg32_data["description"] = "setseq_xsh_rr_64_32";
        
        json testCases = json::array();
        
        // Test case: seed 42, stream 54
        {
            pcg32 rng(42u, 54u);
            json tc;
            tc["name"] = "seeded_42_54";
            tc["seed"] = "42";
            tc["stream"] = "54";
            tc["sequence"] = generate_sequence(rng, 100);
            testCases.push_back(tc);
        }
        
        // ... more test cases
        
        pcg32_data["testCases"] = testCases;
        
        std::ofstream file("oracle_data/pcg32.json");
        file << std::setw(2) << pcg32_data << std::endl;
    }
    
    // pcg64 tests
    // ... similar pattern
    
    return 0;
}
```

### Build Instructions

```cmake
# tools/OracleGenerator/CMakeLists.txt

cmake_minimum_required(VERSION 3.14)
project(pcg_oracle_generator)

set(CMAKE_CXX_STANDARD 17)

# Include PCG headers
include_directories(${CMAKE_SOURCE_DIR}/../../ref-projects/pcg-cpp/include)

# JSON library
include(FetchContent)
FetchContent_Declare(json
    GIT_REPOSITORY https://github.com/nlohmann/json.git
    GIT_TAG v3.11.2)
FetchContent_MakeAvailable(json)

add_executable(generate_oracle generate_oracle.cpp)
target_link_libraries(generate_oracle nlohmann_json::nlohmann_json)
```

## .NET Oracle Test Infrastructure

### OracleTestCase Class

```csharp
public class OracleTestCase
{
    public string Name { get; set; }
    public string? Seed { get; set; }
    public string? Stream { get; set; }
    public List<SequenceEntry> Sequence { get; set; }
    public List<OperationEntry>? Operations { get; set; }
    
    public T CreateGenerator<T>() where T : struct
    {
        // Factory method to create generator with test parameters
    }
}

public class SequenceEntry
{
    public int Call { get; set; }
    public string Value { get; set; }
    
    public uint AsUInt32() => Convert.ToUInt32(Value, 16);
    public ulong AsUInt64() => Convert.ToUInt64(Value, 16);
}
```

### Test Data Loader

```csharp
public static class OracleData
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static IEnumerable<object[]> Load(string generatorName)
    {
        var path = Path.Combine("OracleData", $"{generatorName}.json");
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<OracleDataFile>(json, Options);
        
        foreach (var testCase in data.TestCases)
        {
            yield return new object[] { testCase };
        }
    }
}
```

## CI Integration

### GitHub Actions Workflow

```yaml
# .github/workflows/test.yml

name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore
        run: dotnet restore dotnet/Pcg.sln
      
      - name: Build
        run: dotnet build dotnet/Pcg.sln --no-restore
      
      - name: Test
        run: dotnet test dotnet/Pcg.sln --no-build --verbosity normal
  
  regenerate-oracle:
    runs-on: ubuntu-latest
    if: github.event_name == 'workflow_dispatch'
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Build Oracle Generator
        run: |
          cd tools/OracleGenerator
          cmake -B build
          cmake --build build
      
      - name: Generate Oracle Data
        run: |
          cd tools/OracleGenerator/build
          ./generate_oracle
          cp oracle_data/*.json ../../../tests/Pcg.Oracle.Tests/OracleData/
```

## Validation Checklist

For each generator type, verify:

- [ ] Default constructor produces expected first 10 values
- [ ] Seeded constructor (42, 54) produces expected first 100 values
- [ ] Seeded constructor (0, 0) produces expected values
- [ ] Seeded constructor (max, max) produces expected values
- [ ] Advance(N) matches N sequential calls
- [ ] Backstep(N) reverses Advance(N)
- [ ] Bounded random stays within bounds
- [ ] Bounded random matches expected sequence
- [ ] Different streams produce different sequences
- [ ] Generator state can be serialized/deserialized correctly

## Generators to Test

### Priority 1 (Core)
- `pcg32` (setseq_xsh_rr_64_32)
- `pcg64` (setseq_xsl_rr_128_64)
- `pcg32_oneseq`
- `pcg32_fast`

### Priority 2 (Variants)
- `pcg64_oneseq`
- `pcg64_fast`
- `pcg32_unique`
- `pcg64_unique`

### Priority 3 (Extended)
- `pcg32_k2`
- `pcg32_k64`
- `pcg64_k32`

### Priority 4 (Insecure/Special)
- `pcg32_once_insecure`
- `pcg64_once_insecure`
- Extended generators with larger state

## Related RFCs

- RFC-001: Project Overview and Scope
- RFC-002: Architecture and Design
- RFC-004: Implementation Phases
