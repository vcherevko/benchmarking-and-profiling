using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks;

[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
//[SimpleJob(RuntimeMoniker.Net80)] // have to add this version to .csproj file <TargetFrameworks> as well
//[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net90, iterationCount: 10)]
[MemoryDiagnoser]
public class SimpleBenchmark
{
	[Benchmark(Baseline = true)]
	[Arguments(3)]
	[Arguments(7)]
	//[ArgumentsSource(nameof(CreateData))]
	public string Calculate(int input)
	{
		Thread.Sleep(100 * input);

		return "Avoid JIT elemination";
	}

	[Benchmark]
	[Arguments(3)]
	[Arguments(7)]
	public string CalculateV2(int input)
	{
		Thread.Sleep(50 * input); // Improved

		return "Avoid JIT elemination";
	}

	private IEnumerable<int> CreateData()
	{
		yield return 3;
		yield return 7;
	}
}