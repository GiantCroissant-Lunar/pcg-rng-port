cmake_minimum_required(VERSION 3.14)
project(pcg_oracle_generator CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Include PCG headers from reference project
set(PCG_INCLUDE_DIR "${CMAKE_SOURCE_DIR}/../../../ref-projects/pcg-cpp/include")
include_directories(${PCG_INCLUDE_DIR})

# Fetch nlohmann/json for JSON output
include(FetchContent)
FetchContent_Declare(json
    GIT_REPOSITORY https://github.com/nlohmann/json.git
    GIT_TAG v3.11.2
)
FetchContent_MakeAvailable(json)

add_executable(generate_oracle generate_oracle.cpp)
target_link_libraries(generate_oracle nlohmann_json::nlohmann_json)

# Create output directory
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/oracle_data)
