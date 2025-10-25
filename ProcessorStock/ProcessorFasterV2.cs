using System.Globalization;

namespace ProcessorStock;

public class ProcessorFasterV2
{
	public Dictionary<string, (int Count, decimal Min, decimal Max, decimal Average, decimal Total)> Stocks { get; } = new();

	public void Initialize()
	{
		var dataPath = "./Data";
		foreach (var file in Directory.GetFiles(dataPath))
		{
			var content = File.ReadAllText(file);
			var lines = content.Split(Environment.NewLine);

			for (int i = 1; i < lines.Length; i++)
			{
				var line = lines[i];
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

	// These are no longer needed as we calculate all in one pass
	/*public decimal Min(string ticker)
	{
		decimal min = decimal.MaxValue;

		foreach (var trade in Stocks[ticker].Trades)
		{
			if (trade.Change < min)
			{
				min = trade.Change;
			}
		}

		return min;
	}

	public decimal Max(string ticker)
	{
		decimal max = decimal.MinValue;

		foreach (var trade in Stocks[ticker].Trades)
		{
			if (trade.Change > max)
			{
				max = trade.Change;
			}
		}

		return max;
	}

	public decimal Average(string ticker)
	{
		decimal total = 0;

		foreach (var trade in Stocks[ticker].Trades)
		{
			total += trade.Change;
		}

		return total / Stocks[ticker].Trades.Count;
	}

	public (decimal min, decimal max, decimal average) GetReport(string ticker)
	{
		decimal min = decimal.MaxValue;
		decimal max = decimal.MinValue;
		decimal total = 0;
		int count = 0;

		foreach (var trade in Stocks[ticker].Trades)
		{
			if (trade.Change < min)
			{
				min = trade.Change;
			}
			if (trade.Change > max)
			{
				max = trade.Change;
			}
			total += trade.Change;
			count++;
		}

		decimal average = total / count;
		return (min, max, average);
	}*/
}
