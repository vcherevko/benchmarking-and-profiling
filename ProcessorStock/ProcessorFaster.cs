using System.Globalization;

namespace ProcessorStock;

public class ProcessorFaster
{
	public Dictionary<string, Stock> Stocks { get; } = new();

	public void Initialize()
	{
		var dataPath = "./Data";
		foreach (var file in Directory.GetFiles(dataPath))
		{
			var content = File.ReadAllText(file);
			var lines = content.Split(Environment.NewLine);

			//foreach (var line in lines[1..]) // skip the first line (header)
			for (int i = 1; i < lines.Length; i++)
			{
				var line = lines[i];
				var isEmptyLine = string.IsNullOrWhiteSpace(line);
				if (isEmptyLine)
				{
					continue;
				}

				//var csv = line.Split(',');
				//if (csv.Length < 8) continue;
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

				// remove unused values from parsing
				var trade = new Trade(DateTime.MinValue,
					decimal.MinValue,
					decimal.Parse(value, CultureInfo.InvariantCulture),
					decimal.MinValue);

				if (!Stocks.ContainsKey(name))
				{
					Stocks[name] = new Stock(name);
				}

				for (int a = 0; a < 10; a++)
				{
					Stocks[name].Trades.Add(trade);
				}
			}
		}
	}

	public decimal Min(string ticker)
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
	}
}
