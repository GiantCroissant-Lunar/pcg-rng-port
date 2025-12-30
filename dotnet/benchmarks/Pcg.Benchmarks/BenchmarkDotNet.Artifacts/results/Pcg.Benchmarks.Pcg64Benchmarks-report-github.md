```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.26200.7171)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2


```
| Method              | Mean       | Error      | StdDev     | Median     | Gen0   | Allocated |
|-------------------- |-----------:|-----------:|-----------:|-----------:|-------:|----------:|
| Pcg64_Next          |   2.006 ns |  0.2265 ns |  0.6678 ns |   1.909 ns |      - |         - |
| Random_Next         |   2.660 ns |  0.1327 ns |  0.3913 ns |   2.569 ns |      - |         - |
| Pcg64_Next_Bounded  |   1.666 ns |  0.0630 ns |  0.1382 ns |   1.645 ns |      - |         - |
| Random_Next_Bounded |   3.536 ns |  0.1288 ns |  0.3797 ns |   3.581 ns |      - |         - |
| Pcg64_NextDouble    |  10.586 ns |  0.4068 ns |  1.1930 ns |  10.714 ns | 0.0029 |      48 B |
| Random_NextDouble   |   3.386 ns |  0.2038 ns |  0.6008 ns |   3.518 ns |      - |         - |
| Pcg64_NextBytes     | 262.505 ns | 15.2404 ns | 44.9367 ns | 249.492 ns | 0.0029 |      48 B |
| Pcg64_Shuffle       | 167.136 ns |  5.1913 ns | 15.3066 ns | 163.265 ns | 0.0081 |     136 B |
| Random_Shuffle      | 126.250 ns |  2.5177 ns |  3.2737 ns | 124.984 ns | 0.0052 |      88 B |
