using System.Globalization;
using System.Text;

namespace DataGenerator;

class Program
{
	static readonly int FileCount = 5;
	static readonly int RecordsPerFile = 1_000_000;
	static readonly string OutputPath = "C:\\Learning\\benchmarking-and-profiling\\ProcessorStock\\Data";

	static void Main(string[] args)
	{
		GenerateData(FileCount, RecordsPerFile, OutputPath);
	}

	static void GenerateData(int fileCount, int recordsPerFile, string outputPath)
	{
		if (!Directory.Exists(outputPath))
		{
			Directory.CreateDirectory(outputPath);
		}

		var tickers = new[] { "AAPL", "MSFT", "GOOGL", "AMZN", "META", "TSLA", "NVDA", "JPM", "V", "WMT", "SKE", "QQW", "NJXE", "KJKLSJ", "PLMNO", "ZXCVB", "QWERTY", "ASDFG", "HJKLZ", "POIUY", "MNBVC" };
		var random = new Random();
		var baseDate = new DateTime(2024, 1, 1);

		Console.WriteLine($"Generating {fileCount} file(s) with {recordsPerFile} records each...");

		for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
		{
			var fileName = Path.Combine(outputPath, $"stocks_{fileIndex + 1}.csv");
			var sb = new StringBuilder();

			sb.AppendLine("Ticker,Date,Open,High,Low,Close,Volume,Adj Close,Change");

			for (int recordIndex = 0; recordIndex < recordsPerFile; recordIndex++)
			{
				// Generate a random 4-byte array and convert to base64 for a unique ticker
				var tickerName = random.Next(tickers.Length);
				var ticker = tickers[tickerName];
				var date = baseDate.AddDays(recordIndex % 365);

				var open = Math.Round((decimal)(random.NextDouble() * 300 + 100), 2);
				var volatility = (decimal)(random.NextDouble() * 10);
				var high = Math.Round(open + volatility, 2);
				var low = Math.Round(open - volatility, 2);
				var close = Math.Round((decimal)(random.NextDouble() * (double)(high - low)) + low, 2);
				var volume = random.Next(10000000, 100000000);
				var adjClose = close;
				var change = Math.Round(close - open, 2);

				sb.AppendLine($"{ticker},{date:yyyy-MM-dd},{open.ToString(CultureInfo.InvariantCulture)},{high.ToString(CultureInfo.InvariantCulture)},{low.ToString(CultureInfo.InvariantCulture)},{close.ToString(CultureInfo.InvariantCulture)},{volume},{adjClose.ToString(CultureInfo.InvariantCulture)},{change.ToString(CultureInfo.InvariantCulture)}");
			}

			File.WriteAllText(fileName, sb.ToString());

			var fileInfo = new FileInfo(fileName);
			Console.WriteLine($"Generated: {fileName} ({fileInfo.Length / 1024.0:F2} KB)");
		}

		var totalSize = Directory.GetFiles(outputPath)
			.Sum(f => new FileInfo(f).Length);

		Console.WriteLine();
		Console.WriteLine($"Total records: {fileCount * recordsPerFile:N0}");
		Console.WriteLine($"Total size: {totalSize / 1024.0 / 1024.0:F2} MB");
		Console.WriteLine($"Output directory: {Path.GetFullPath(outputPath)}");
	}
}
