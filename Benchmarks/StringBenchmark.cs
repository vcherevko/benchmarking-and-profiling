using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks;

[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[MemoryDiagnoser]
public class StringBenchmark
{
	[Benchmark]
	public string BuildString()
	{
		string result = "";
		for (int i = 0; i < 50_000; i++)
		{
			result += i;
			result += Environment.NewLine;
		}

		return result;
	}

	[Benchmark]
	public string BuildStringInterpolation()
	{
		string result = "";
		for (int i = 0; i < 50_000; i++)
		{
			result = $"{result}{i}{Environment.NewLine}";
		}

		return result;
	}

	[Benchmark]
	public string BuildStringBuilder()
	{
		var builder = new StringBuilder();
		for (int i = 0; i < 50_000; i++)
		{
			builder.Append(i);
			builder.AppendLine();
		}

		return builder.ToString();
	}
}