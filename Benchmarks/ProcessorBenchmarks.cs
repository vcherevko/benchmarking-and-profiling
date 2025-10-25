using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ProcessorStock;

namespace Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class ProcessorBenchmarks
{
	[Benchmark]
	public List<string> ProcessorOriginal()
	{
		var processor = new Processor();
		processor.Initialize();
		var result = new List<string>();

		foreach (var stock in processor.Stocks)
		{
			var min = processor.Min(stock.Key);
			var max = processor.Max(stock.Key);
			var average = processor.Average(stock.Key);

			result.Add($"{min} {max} {average}");
		}

		return result;
	}

	[Benchmark]
	public List<string> ProcessorOriginalImproved()
	{
		// Changed an approach to make it faster by reducing the number of iterations
		// Using the GetReport method to get all three values at once
		var processor = new Processor();
		processor.Initialize();
		var result = new List<string>();

		foreach (var stock in processor.Stocks)
		{
			var (min, max, average) = processor.GetReport(stock.Key);
			result.Add($"{min} {max} {average}");
		}

		return result;
	}

	[Benchmark]
	public List<string> ProcessorFaster()
	{
		// Further improved version of Processor with optimized data structures
		var processor = new ProcessorFaster();
		processor.Initialize();
		var result = new List<string>();

		foreach (var stock in processor.Stocks)
		{
			var (min, max, average) = processor.GetReport(stock.Key);
			result.Add($"{min} {max} {average}");
		}

		return result;
	}

	[Benchmark]
	public List<string> ProcessorFasterV2()
	{
		// Improved version of ProcessorFaster with precomputed values
		var processor = new ProcessorFasterV2();
		processor.Initialize();
		var result = new List<string>();

		foreach (var stock in processor.Stocks)
		{
			result.Add($"{stock.Value.Min} {stock.Value.Max} {stock.Value.Average}");
		}

		return result;
	}
}