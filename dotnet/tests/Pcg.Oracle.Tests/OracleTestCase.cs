using System.Text.Json.Serialization;

namespace Pcg.Oracle.Tests;

/// <summary>
/// Represents a test case loaded from oracle test data.
/// </summary>
public class OracleTestCase
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("seed")]
    public string? Seed { get; set; }

    [JsonPropertyName("stream")]
    public string? Stream { get; set; }

    [JsonPropertyName("seedHi")]
    public string? SeedHi { get; set; }

    [JsonPropertyName("seedLo")]
    public string? SeedLo { get; set; }

    [JsonPropertyName("streamHi")]
    public string? StreamHi { get; set; }

    [JsonPropertyName("streamLo")]
    public string? StreamLo { get; set; }

    [JsonPropertyName("sequence")]
    public List<SequenceEntry> Sequence { get; set; } = new();

    [JsonPropertyName("operations")]
    public List<OperationEntry>? Operations { get; set; }

    public override string ToString() => Name;
}

/// <summary>
/// A single value in a sequence.
/// </summary>
public class SequenceEntry
{
    [JsonPropertyName("call")]
    public int Call { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    public uint AsUInt32()
    {
        string hex = Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
            ? Value[2..] 
            : Value;
        return Convert.ToUInt32(hex, 16);
    }

    public ulong AsUInt64()
    {
        string hex = Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
            ? Value[2..] 
            : Value;
        return Convert.ToUInt64(hex, 16);
    }

    public UInt128 AsUInt128()
    {
        string hex = Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? Value[2..]
            : Value;

        if (hex.Length > 16)
        {
            string hiHex = hex[..(hex.Length - 16)];
            string loHex = hex[(hex.Length - 16)..];
            ulong hi = Convert.ToUInt64(hiHex, 16);
            ulong lo = Convert.ToUInt64(loHex, 16);
            return new UInt128(hi, lo);
        }
        else
        {
            ulong lo = Convert.ToUInt64(hex, 16);
            return new UInt128(0UL, lo);
        }
    }
}

/// <summary>
/// An operation in an operation sequence test.
/// </summary>
public class OperationEntry
{
    [JsonPropertyName("op")]
    public string Op { get; set; } = "";

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("delta")]
    public ulong? Delta { get; set; }

    [JsonPropertyName("expected")]
    public List<string>? Expected { get; set; }
}

/// <summary>
/// Root structure for oracle data file.
/// </summary>
public class OracleDataFile
{
    [JsonPropertyName("generator")]
    public string Generator { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("testCases")]
    public List<OracleTestCase> TestCases { get; set; } = new();
}
