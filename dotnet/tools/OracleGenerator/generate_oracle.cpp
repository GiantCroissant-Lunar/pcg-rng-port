/*
 * PCG Oracle Data Generator
 * 
 * This program generates JSON test data from the C++ reference implementation
 * to verify the .NET port produces identical output.
 */

#include <iostream>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <string>
#include <vector>

#include "pcg_random.hpp"
#include <nlohmann/json.hpp>

using json = nlohmann::json;
using pcg_extras::operator<<;

// Helper to format a value as hex string
template<typename T>
std::string to_hex(T value) {
    std::ostringstream ss;
    ss << "0x" << std::hex << std::setfill('0') 
       << std::setw(sizeof(T) * 2) << value;
    return ss.str();
}

// Generate sequence of values
template<typename RNG>
json generate_sequence(RNG& rng, size_t count) {
    json sequence = json::array();
    for (size_t i = 0; i < count; i++) {
        json entry;
        entry["call"] = i + 1;
        entry["value"] = to_hex(rng());
        sequence.push_back(entry);
    }
    return sequence;
}

// Generate test cases for pcg32
json generate_pcg32_tests() {
    json result;
    result["generator"] = "pcg32";
    result["description"] = "setseq_xsh_rr_64_32 - Default 32-bit generator with settable stream";
    
    json testCases = json::array();
    
    // Test case 1: Standard test seed (42, 54)
    {
        pcg32 rng(42u, 54u);
        json tc;
        tc["name"] = "seed_42_stream_54";
        tc["seed"] = "42";
        tc["stream"] = "54";
        tc["sequence"] = generate_sequence(rng, 100);
        testCases.push_back(tc);
    }
    
    // Test case 2: Zero seed
    {
        pcg32 rng(0u, 0u);
        json tc;
        tc["name"] = "seed_0_stream_0";
        tc["seed"] = "0";
        tc["stream"] = "0";
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    // Test case 3: Max values
    {
        pcg32 rng(UINT64_MAX, UINT64_MAX);
        json tc;
        tc["name"] = "seed_max_stream_max";
        tc["seed"] = std::to_string(UINT64_MAX);
        tc["stream"] = std::to_string(UINT64_MAX);
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    // Test case 4: Various seeds
    {
        pcg32 rng(12345u, 67890u);
        json tc;
        tc["name"] = "seed_12345_stream_67890";
        tc["seed"] = "12345";
        tc["stream"] = "67890";
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    // Test case 5: Single arg constructor
    {
        pcg32 rng(42u);
        json tc;
        tc["name"] = "seed_42_default_stream";
        tc["seed"] = "42";
        tc["stream"] = std::to_string(1442695040888963407ULL >> 1);  // Default stream
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    result["testCases"] = testCases;
    return result;
}

// Generate test cases for pcg32_oneseq
json generate_pcg32_oneseq_tests() {
    json result;
    result["generator"] = "pcg32_oneseq";
    result["description"] = "oneseq_xsh_rr_64_32 - 32-bit generator with fixed stream";
    
    json testCases = json::array();
    
    {
        pcg32_oneseq rng(42u);
        json tc;
        tc["name"] = "seed_42";
        tc["seed"] = "42";
        tc["sequence"] = generate_sequence(rng, 100);
        testCases.push_back(tc);
    }
    
    {
        pcg32_oneseq rng(0u);
        json tc;
        tc["name"] = "seed_0";
        tc["seed"] = "0";
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    result["testCases"] = testCases;
    return result;
}

// Generate test cases for pcg32_fast
json generate_pcg32_fast_tests() {
    json result;
    result["generator"] = "pcg32_fast";
    result["description"] = "mcg_xsh_rs_64_32 - Fast 32-bit MCG generator";
    
    json testCases = json::array();
    
    {
        pcg32_fast rng(42u);
        json tc;
        tc["name"] = "seed_42";
        tc["seed"] = "42";
        tc["sequence"] = generate_sequence(rng, 100);
        testCases.push_back(tc);
    }
    
    {
        pcg32_fast rng(0u);
        json tc;
        tc["name"] = "seed_0";
        tc["seed"] = "0";
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    result["testCases"] = testCases;
    return result;
}

// Generate test cases for pcg64
json generate_pcg64_tests() {
    json result;
    result["generator"] = "pcg64";
    result["description"] = "setseq_xsl_rr_128_64 - Default 64-bit generator with settable stream";
    
    json testCases = json::array();
    
    // Test case 1: Simple seed (42, 54)
    {
        pcg64 rng(42u, 54u);
        json tc;
        tc["name"] = "seed_42_stream_54";
        tc["seedHi"] = "0";
        tc["seedLo"] = "42";
        tc["streamHi"] = "0";
        tc["streamLo"] = "54";
        tc["sequence"] = generate_sequence(rng, 100);
        testCases.push_back(tc);
    }
    
    // Test case 2: Zero seed
    {
        pcg64 rng(0u, 0u);
        json tc;
        tc["name"] = "seed_0_stream_0";
        tc["seedHi"] = "0";
        tc["seedLo"] = "0";
        tc["streamHi"] = "0";
        tc["streamLo"] = "0";
        tc["sequence"] = generate_sequence(rng, 50);
        testCases.push_back(tc);
    }
    
    result["testCases"] = testCases;
    return result;
}

int main() {
    std::cout << "Generating PCG Oracle Test Data..." << std::endl;
    
    // Generate pcg32 tests
    {
        json data = generate_pcg32_tests();
        std::ofstream file("oracle_data/pcg32.json");
        file << std::setw(2) << data << std::endl;
        std::cout << "  Generated pcg32.json" << std::endl;
    }
    
    // Generate pcg32_oneseq tests
    {
        json data = generate_pcg32_oneseq_tests();
        std::ofstream file("oracle_data/pcg32_oneseq.json");
        file << std::setw(2) << data << std::endl;
        std::cout << "  Generated pcg32_oneseq.json" << std::endl;
    }
    
    // Generate pcg32_fast tests
    {
        json data = generate_pcg32_fast_tests();
        std::ofstream file("oracle_data/pcg32_fast.json");
        file << std::setw(2) << data << std::endl;
        std::cout << "  Generated pcg32_fast.json" << std::endl;
    }
    
    // Generate pcg64 tests
    {
        json data = generate_pcg64_tests();
        std::ofstream file("oracle_data/pcg64.json");
        file << std::setw(2) << data << std::endl;
        std::cout << "  Generated pcg64.json" << std::endl;
    }
    
    std::cout << "Done!" << std::endl;
    return 0;
}
