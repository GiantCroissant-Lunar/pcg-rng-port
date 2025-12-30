```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.26200.7171)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2


```
| Method                      | Mean        | Error      | StdDev     | Median      | Gen0   | Allocated |
|---------------------------- |------------:|-----------:|-----------:|------------:|-------:|----------:|
| Pcg32_Next                  |   0.6273 ns |  0.0870 ns |  0.2565 ns |   0.6486 ns |      - |         - |
| Pcg32Fast_Next              |   0.3191 ns |  0.0720 ns |  0.2123 ns |   0.3778 ns |      - |         - |
| Pcg32K64_Next               |   1.6507 ns |  0.0967 ns |  0.2852 ns |   1.5991 ns |      - |         - |
| Pcg32K64Oneseq_Next         |   1.4677 ns |  0.0955 ns |  0.2816 ns |   1.5012 ns |      - |         - |
| Pcg32K64Fast_Next           |   1.5069 ns |  0.0831 ns |  0.2449 ns |   1.5695 ns |      - |         - |
| Pcg32C64_Next               |   1.3109 ns |  0.0726 ns |  0.2140 ns |   1.2968 ns |      - |         - |
| Pcg32C64Oneseq_Next         |   1.6831 ns |  0.0669 ns |  0.1642 ns |   1.6911 ns |      - |         - |
| Pcg32C64Fast_Next           |   1.6173 ns |  0.0849 ns |  0.2503 ns |   1.6249 ns |      - |         - |
| Pcg32_Next_Bounded          |   0.7298 ns |  0.0717 ns |  0.2115 ns |   0.7137 ns |      - |         - |
| Pcg32Fast_Next_Bounded      |   0.8117 ns |  0.0565 ns |  0.1666 ns |   0.8189 ns |      - |         - |
| Pcg32K64_Next_Bounded       |   2.2723 ns |  0.1006 ns |  0.2967 ns |   2.2830 ns |      - |         - |
| Pcg32K64Oneseq_Next_Bounded |   1.6914 ns |  0.1023 ns |  0.3017 ns |   1.6187 ns |      - |         - |
| Pcg32K64Fast_Next_Bounded   |   1.8203 ns |  0.1419 ns |  0.4183 ns |   1.7368 ns |      - |         - |
| Pcg32C64_Next_Bounded       |   2.0470 ns |  0.1312 ns |  0.3869 ns |   2.0151 ns |      - |         - |
| Pcg32C64Oneseq_Next_Bounded |   2.3323 ns |  0.1011 ns |  0.2980 ns |   2.3762 ns |      - |         - |
| Pcg32C64Fast_Next_Bounded   |   1.8806 ns |  0.1239 ns |  0.3653 ns |   1.8599 ns |      - |         - |
| Pcg32_NextDouble            |   6.5332 ns |  0.2794 ns |  0.8237 ns |   6.7211 ns | 0.0019 |      32 B |
| Pcg32Fast_NextDouble        |   5.5813 ns |  0.1999 ns |  0.5895 ns |   5.7045 ns | 0.0014 |      24 B |
| Pcg32K64_NextDouble         |  11.1660 ns |  0.3743 ns |  1.1037 ns |  11.2352 ns | 0.0024 |      40 B |
| Pcg32K64Oneseq_NextDouble   |   9.7767 ns |  0.3323 ns |  0.9799 ns |   9.8085 ns | 0.0019 |      32 B |
| Pcg32K64Fast_NextDouble     |  10.9110 ns |  0.3915 ns |  1.1544 ns |  11.1552 ns | 0.0019 |      32 B |
| Pcg32C64_NextDouble         |  12.2978 ns |  0.2765 ns |  0.6407 ns |  12.2744 ns | 0.0024 |      40 B |
| Pcg32C64Oneseq_NextDouble   |  10.0456 ns |  0.4235 ns |  1.2487 ns |  10.0292 ns | 0.0019 |      32 B |
| Pcg32C64Fast_NextDouble     |  10.4313 ns |  0.4908 ns |  1.4471 ns |  10.3983 ns | 0.0019 |      32 B |
| Pcg32_NextBytes             | 331.9682 ns |  9.5780 ns | 28.2410 ns | 332.2034 ns | 0.0019 |      32 B |
| Pcg32Fast_NextBytes         | 268.3006 ns | 10.8794 ns | 32.0782 ns | 266.6202 ns | 0.0014 |      24 B |
| Pcg32K64_NextBytes          | 379.3400 ns |  8.0979 ns | 23.8767 ns | 378.5090 ns | 0.0024 |      40 B |
| Pcg32K64Oneseq_NextBytes    | 367.3665 ns | 14.6563 ns | 43.2145 ns | 359.9645 ns | 0.0019 |      32 B |
| Pcg32K64Fast_NextBytes      | 294.6368 ns | 13.8855 ns | 40.9417 ns | 303.8714 ns | 0.0019 |      32 B |
| Pcg32C64_NextBytes          | 340.3664 ns | 15.3831 ns | 45.3574 ns | 352.9996 ns | 0.0024 |      40 B |
| Pcg32C64Oneseq_NextBytes    | 251.3415 ns |  6.0168 ns | 17.6462 ns | 246.1150 ns | 0.0019 |      32 B |
| Pcg32C64Fast_NextBytes      | 282.7792 ns | 11.7941 ns | 34.5901 ns | 277.3761 ns | 0.0019 |      32 B |
| Pcg32_Shuffle               | 172.2280 ns |  7.3650 ns | 21.7160 ns | 172.1450 ns | 0.0072 |     120 B |
| Pcg32Fast_Shuffle           | 143.1806 ns |  5.4239 ns | 15.8218 ns | 138.1608 ns | 0.0067 |     112 B |
| Pcg32K64_Shuffle            | 149.6813 ns |  3.4622 ns |  9.7651 ns | 149.8287 ns | 0.0076 |     128 B |
| Pcg32K64Oneseq_Shuffle      | 172.4830 ns |  6.4192 ns | 18.9270 ns | 172.9923 ns | 0.0072 |     120 B |
| Pcg32K64Fast_Shuffle        | 175.1536 ns |  4.1857 ns | 12.2760 ns | 174.2796 ns | 0.0072 |     120 B |
| Pcg32C64_Shuffle            | 169.1025 ns |  4.8718 ns | 14.3645 ns | 168.7465 ns | 0.0076 |     128 B |
| Pcg32C64Oneseq_Shuffle      | 177.3303 ns |  3.5884 ns |  9.0027 ns | 177.4117 ns | 0.0072 |     120 B |
| Pcg32C64Fast_Shuffle        | 180.0948 ns |  4.8996 ns | 14.4466 ns | 177.2893 ns | 0.0072 |     120 B |
