using System.Text.Json;

namespace Pcg.Oracle.Tests;

/// <summary>
/// Loads oracle test data from JSON files.
/// </summary>
public static class OracleDataLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Loads test cases for the specified generator.
    /// </summary>
    public static IEnumerable<object[]> Load(string generatorName)
    {
        var path = Path.Combine("OracleData", $"{generatorName}.json");
        
        if (!File.Exists(path))
        {
            yield break;
        }

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<OracleDataFile>(json, Options);

        if (data?.TestCases == null)
        {
            yield break;
        }

        foreach (var testCase in data.TestCases)
        {
            yield return new object[] { testCase };
        }
    }

    /// <summary>
    /// Gets all test cases for a generator.
    /// </summary>
    public static List<OracleTestCase> GetTestCases(string generatorName)
    {
        var path = Path.Combine("OracleData", $"{generatorName}.json");
        
        if (!File.Exists(path))
        {
            return new List<OracleTestCase>();
        }

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<OracleDataFile>(json, Options);

        return data?.TestCases ?? new List<OracleTestCase>();
    }
}
