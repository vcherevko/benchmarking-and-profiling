using BenchmarkDotNet.Running;

namespace Benchmarks;

public static class Program
{
	public static void Main()
	{
		var summary = BenchmarkRunner.Run<ProcessorBenchmarks>();
		Console.WriteLine(summary);
	}
}
