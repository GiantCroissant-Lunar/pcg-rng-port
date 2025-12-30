```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.26200.7171)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2


```
| Method              | Mean        | Error      | StdDev     | Median      | Gen0   | Allocated |
|-------------------- |------------:|-----------:|-----------:|------------:|-------:|----------:|
| Pcg32_Next          |   0.5043 ns |  0.0419 ns |  0.1216 ns |   0.4965 ns |      - |         - |
| Random_Next         |   2.6352 ns |  0.0847 ns |  0.2483 ns |   2.6231 ns |      - |         - |
| Pcg32_Next_Bounded  |   0.8852 ns |  0.0450 ns |  0.0909 ns |   0.8787 ns |      - |         - |
| Random_Next_Bounded |   3.0326 ns |  0.1139 ns |  0.3232 ns |   2.9674 ns |      - |         - |
| Pcg32_NextDouble    |   4.6204 ns |  0.1527 ns |  0.4281 ns |   4.5906 ns | 0.0019 |      32 B |
| Random_NextDouble   |   2.9733 ns |  0.1576 ns |  0.4648 ns |   2.9319 ns |      - |         - |
| Pcg32_NextBytes     | 262.4496 ns | 16.5283 ns | 48.7341 ns | 247.2055 ns | 0.0019 |      32 B |
| Random_NextBytes    | 732.6916 ns | 16.5657 ns | 48.3231 ns | 741.5986 ns |      - |         - |
| Pcg32_Shuffle       | 158.6352 ns |  6.3735 ns | 18.7925 ns | 159.2481 ns | 0.0072 |     120 B |
| Random_Shuffle      | 124.1412 ns |  2.4745 ns |  2.6476 ns | 124.1043 ns | 0.0052 |      88 B |
