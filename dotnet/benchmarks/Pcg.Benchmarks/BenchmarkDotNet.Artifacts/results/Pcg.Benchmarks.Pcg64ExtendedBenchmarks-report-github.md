```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.26200.7171)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2


```
| Method                      | Mean       | Error      | StdDev     | Median     | Gen0   | Allocated |
|---------------------------- |-----------:|-----------:|-----------:|-----------:|-------:|----------:|
| Pcg64K32Oneseq_Next         |   2.123 ns |  0.1244 ns |  0.3628 ns |   2.086 ns |      - |         - |
| Pcg64K32Fast_Next           |   2.786 ns |  0.0875 ns |  0.2438 ns |   2.747 ns |      - |         - |
| Pcg64C32Oneseq_Next         |   1.143 ns |  0.0557 ns |  0.1457 ns |   1.120 ns |      - |         - |
| Pcg64C32Fast_Next           |   1.034 ns |  0.0468 ns |  0.1086 ns |   1.006 ns |      - |         - |
| Pcg64K32Oneseq_Next_Bounded |   2.936 ns |  0.1761 ns |  0.5192 ns |   3.002 ns |      - |         - |
| Pcg64K32Fast_Next_Bounded   |   2.348 ns |  0.1429 ns |  0.4214 ns |   2.309 ns |      - |         - |
| Pcg64C32Oneseq_Next_Bounded |   2.084 ns |  0.0683 ns |  0.1543 ns |   2.042 ns |      - |         - |
| Pcg64C32Fast_Next_Bounded   |   2.098 ns |  0.1557 ns |  0.4592 ns |   2.167 ns |      - |         - |
| Pcg64K32Oneseq_NextDouble   |  13.197 ns |  0.3703 ns |  1.0917 ns |  13.222 ns | 0.0029 |      48 B |
| Pcg64K32Fast_NextDouble     |  12.423 ns |  0.3569 ns |  1.0411 ns |  12.317 ns | 0.0029 |      48 B |
| Pcg64C32Oneseq_NextDouble   |  13.326 ns |  0.4442 ns |  1.3099 ns |  13.093 ns | 0.0029 |      48 B |
| Pcg64C32Fast_NextDouble     |  11.812 ns |  0.4824 ns |  1.4224 ns |  11.547 ns | 0.0029 |      48 B |
| Pcg64K32Oneseq_NextBytes    | 252.912 ns | 10.5586 ns | 31.1322 ns | 254.687 ns | 0.0029 |      48 B |
| Pcg64K32Fast_NextBytes      | 247.486 ns | 11.0208 ns | 32.3222 ns | 240.907 ns | 0.0029 |      48 B |
| Pcg64C32Oneseq_NextBytes    | 287.296 ns | 13.8620 ns | 40.8723 ns | 288.632 ns | 0.0029 |      48 B |
| Pcg64C32Fast_NextBytes      | 267.664 ns |  9.0273 ns | 26.6172 ns | 270.388 ns | 0.0029 |      48 B |
| Pcg64K32Oneseq_Shuffle      | 181.013 ns |  8.6307 ns | 25.4478 ns | 174.713 ns | 0.0081 |     136 B |
| Pcg64K32Fast_Shuffle        | 175.534 ns |  6.1581 ns | 18.1572 ns | 169.686 ns | 0.0081 |     136 B |
| Pcg64C32Oneseq_Shuffle      | 147.892 ns |  2.9878 ns |  5.8276 ns | 146.458 ns | 0.0081 |     136 B |
| Pcg64C32Fast_Shuffle        | 139.938 ns |  2.6729 ns |  4.2394 ns | 138.907 ns | 0.0081 |     136 B |
