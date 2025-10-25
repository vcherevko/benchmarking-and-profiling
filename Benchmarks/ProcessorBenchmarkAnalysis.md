# Benchmark Comparison Analysis

This document compares several implementations measured by BenchmarkDotNet. Each section explains the real changes made and their direct impact on performance and memory usage, with code examples for clarity.

## 1. ProcessorOriginal
- **CSV Parsing:** Used `string.Split(',')` for every line, creating multiple string allocations per row.
```csharp
// Example:
var parts = line.Split(',');
var trade = new Trade {
    Name = parts[0],
    Change = decimal.Parse(parts[7])
};
stocks[name].Trades.Add(trade);
```
- **Trade Storage:** Created and stored full `Trade` objects for each row in a list per stock.
- **Aggregate Calculation:** Aggregates (min, max, average) were calculated by iterating over stored trades after loading.
```csharp
// Example:
var min = stocks[name].Trades.Min(t => t.Change);
var max = stocks[name].Trades.Max(t => t.Change);
var avg = stocks[name].Trades.Average(t => t.Change);
```
- **Impact:**
    - High memory usage (4.58 GB allocated)
    - High Gen0/Gen1 collections (445,000 / 236,000)
    - Slowest mean time (5.102s)

## 2. ProcessorOriginalImproved
- **Change:** Combined separate aggregate calculations into a single method, so min, max, and average are calculated in one pass over the data.
```csharp
// Example:
public (decimal min, decimal max, decimal avg) GetReport(List<Trade> trades) {
    decimal min = decimal.MaxValue, max = decimal.MinValue, sum = 0;
    foreach (var t in trades) {
        if (t.Change < min) min = t.Change;
        if (t.Change > max) max = t.Change;
        sum += t.Change;
    }
    return (min, max, sum / trades.Count);
}
```
- **Impact:**
    - No change in memory usage or allocations (4.58 GB)
    - No change in Gen0/Gen1 collections
    - Mean time and error similar to original (5.163s)
    - The improvement only affects post-load queries, not the loading phase measured by the benchmark

## 3. ProcessorFaster
- **Change:** Switched from `string.Split(',')` to manual CSV parsing using `IndexOf` and string slicing, extracting only the needed columns.
```csharp
// Example:
int comma0 = line.IndexOf(',');
string name = line.Substring(0, comma0);
int comma7 = line.LastIndexOf(',');
string changeStr = line.Substring(comma7 + 1);
var trade = new Trade {
    Name = name,
    Change = decimal.Parse(changeStr)
};
stocks[name].Trades.Add(trade);
```
- **Trade Storage:** Continued to store full `Trade` objects in lists per stock (no change in aggregate storage).
- **Impact:**
    - Significant reduction in memory usage (2.75 GB allocated)
    - Fewer Gen0/Gen1 collections (240,000 / 204,000)
    - Faster mean time (3.857s)
    - Improvement is due to reduced string allocations during parsing

## 4. ProcessorFasterV2
- **Change:** Further optimized manual CSV parsing loop for efficiency, skipping unnecessary columns more directly.
```csharp
// Example:
int start = 0;
string name = null, changeStr = null;
for (int col = 0; col < 8; col++) {
    int end = line.IndexOf(',', start);
    if (col == 0) name = line.Substring(start, end - start);
    if (col == 7) changeStr = line.Substring(start, end - start);
    start = end + 1;
}
```
- **Aggregate Calculation:** Calculated aggregates (min, max, average, total) during parsing, storing them as tuples per stock instead of keeping all trade objects.
```csharp
// Example:
if (!stocks.TryGetValue(name, out var agg))
    agg = (0, decimal.MaxValue, decimal.MinValue, 0m, 0m);
decimal change = decimal.Parse(changeStr);
agg.Count++;
agg.Min = Math.Min(agg.Item2, change);
agg.Max = Math.Max(agg.Item3, change);
agg.Total += change;
agg.Average = agg.Total / agg.Count;
stocks[name] = agg;
```
- **Impact:**
    - Further reduction in memory usage (2.34 GB allocated)
    - Fewer Gen0/Gen1 collections (223,000 / 187,000)
    - Fastest mean time (2.575s)
    - Improvement is due to both more efficient parsing and storing only aggregate values, not all trades
    - Gen2 collections increased (32,000), but this is not a major concern given overall improvements

## 5. ProcessorFasterV3
- **Change:** Switched from loading entire file into memory to using `StreamReader` for line-by-line reading.
```csharp
// V2: Load entire file into memory
var content = File.ReadAllText(file);
var lines = content.Split(Environment.NewLine);
for (int i = 1; i < lines.Length; i++) {
    var line = lines[i];
    // process line
}

// V3: Stream line-by-line
using var reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read));
reader.ReadLine(); // skip header
while(reader.ReadLine() is { } line) {
    // process line
}
```
- **Aggregate Calculation:** Same as V2 - calculates aggregates during parsing and stores tuples.
- **Impact:**
    - Massive reduction in memory usage (1.05 GB allocated, down from 2.34 GB)
    - Gen0 collections cut in half (120,000 vs 223,000)
    - Fastest mean time (1.384s, ~46% faster than V2)
    - Lower StdDev (0.0735s) shows consistent performance
    - No longer allocates memory for entire file content or string arrays from Split()

## 6. ProcessorFasterV3Async
- **Change:** Uses asynchronous file reading with `StreamReader.ReadLineAsync()` and `await`.
```csharp
public async Task InitializeAsync()
{
    var dataPath = "./Data";
    foreach (var file in Directory.GetFiles(dataPath))
    {
        using var reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read));
        await reader.ReadLineAsync(); // skip header line
        while(await reader.ReadLineAsync() is { } line)
        {
            // ...process line as before...
        }
    }
}
```
- **Impact:**
    - Mean: 1.679s (slower than sync V3)
    - Allocated: 1.4 GB
    - Gen0: 160,000
    - Slightly higher memory and GC pressure than sync version
    - Async overhead (Task/state machine allocations) outweighs benefits for sequential file processing

## 7. ProcessorFasterV3Async2
- **Change:** Uses `File.ReadLinesAsync()` with `await foreach` for asynchronous line enumeration.
```csharp
public async Task InitializeAsync2()
{
    var dataPath = "./Data";
    foreach (var file in Directory.GetFiles(dataPath))
    {
        await foreach (var line in File.ReadLinesAsync(file))
        {
            if (line.StartsWith("Ticker")) continue; // skip header
            // ...process line as before...
        }
    }
}
```
- **Impact:**
    - Mean: 5.028s (much slower than both sync and other async version)
    - Allocated: 1.4 GB
    - Gen0: 160,000
    - High StdDev (0.4170s) indicates inconsistent performance
    - Despite similar logic, much slower due to extra abstraction and iterator overhead in `IAsyncEnumerable`

---

## Async Context and Observations
- **Async can be useful** when you want to process multiple files in parallel, handle concurrent I/O, or keep UI/server threads responsive. In this benchmark, all processing is sequential, so async adds overhead without benefit.
- **Key advantages of async:**
    - Enables parallel/concurrent file processing (e.g., `Task.WhenAll`)
    - Improves scalability in server or UI scenarios
    - Frees up threads during I/O waits
- **Surprising result:** The two async approaches look almost identical in code, but `StreamReader.ReadLineAsync()` is much faster than `File.ReadLinesAsync()` with `await foreach`. The latter introduces more abstraction and iterator overhead, resulting in a 3x slower execution.
- **Lesson:** Not all async implementations are equal. Always benchmark real-world usage, especially when choosing between similar APIs.

---

## Updated Summary Table
| Method                    | Mean    | Allocated | Gen0        | Gen1        | Gen2       |
|-------------------------- |--------:|----------:|------------:|------------:|-----------:|
| ProcessorOriginal         | 5.102 s |   4.58 GB | 445,000     | 236,000     | 16,000     |
| ProcessorOriginalImproved | 5.163 s |   4.58 GB | 445,000     | 236,000     | 16,000     |
| ProcessorFaster           | 3.857 s |   2.75 GB | 240,000     | 204,000     | 15,000     |
| ProcessorFasterV2         | 2.575 s |   2.34 GB | 223,000     | 187,000     | 32,000     |
| ProcessorFasterV3         | 1.384 s |   1.05 GB | 120,000     | N/A         | N/A        |
| ProcessorFasterV3Async    | 1.679 s |   1.4 GB  | 160,000     | N/A         | N/A        |
| ProcessorFasterV3Async2   | 5.028 s |   1.4 GB  | 160,000     | N/A         | N/A        |

---

## Key Takeaways
- **ProcessorFaster** improved performance by reducing string allocations during parsing, not by changing aggregate storage.
- **ProcessorFasterV2** achieved the best results by optimizing both parsing and aggregate storage.
- **ProcessorOriginalImproved** did not affect the measured benchmark phase, so results are unchanged.
- **ProcessorFasterV3** achieved breakthrough performance by eliminating file-to-memory loading, using streaming instead.
- **Async versions** show that async is not always faster for sequential file processing, but can be valuable for parallel/concurrent scenarios. The choice of async API can have a dramatic impact on performance.
