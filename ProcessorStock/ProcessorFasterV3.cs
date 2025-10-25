using System.Globalization;

namespace ProcessorStock;

public class ProcessorFasterV3
{
	public Dictionary<string, (int Count, decimal Min, decimal Max, decimal Average, decimal Total)> Stocks { get; } = new();

	public void Initialize()
	{
		var dataPath = "./Data";
		foreach (var file in Directory.GetFiles(dataPath))
		{
			using var reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read));
			reader.ReadLine(); // skip header line

			while(reader.ReadLine() is { } line)
			{
				var isEmptyLine = string.IsNullOrWhiteSpace(line);
				if (isEmptyLine)
				{
					continue;
				}

				int startIndex = 0;
				int endIndex = 0;
				string name = string.Empty;
				string value = string.Empty;

				// we know the csv contains 8 columns
				for (int column = 0; column < 8; column++)
				{
					endIndex = line.IndexOf(',', startIndex);
					if (column == 0) // the stock name
					{
						name = line[startIndex..endIndex];
					}
					else if (column == 7) // the value we want
					{
						value = line[startIndex..endIndex];
					}

					startIndex = endIndex + 1;
				}

				var change = decimal.Parse(value, CultureInfo.InvariantCulture);

				// First time seeing this stock
				if (!Stocks.ContainsKey(name))
				{
					// Create it as a tuple, could use the other parsed data points here!
					Stocks[name] = (1, change, change, change, change);
				}
				else
				{
					var trade = Stocks[name];
					var min = decimal.Min(trade.Min, change);
					var max = decimal.Max(trade.Max, change);
					var total = trade.Total + change;
					var count = trade.Count + 1;
					var average = total / count;

					// Replace with a new tuple with updated values
					Stocks[name] = (count, min, max, average, total);
				}
			}
		}
	}
}
