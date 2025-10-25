using System.Globalization;
using ProcessorStock.Models;

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

			for (int i = 1; i < lines.Length; i++)
			{
				var line = lines[i];
				var isEmptyLine = string.IsNullOrWhiteSpace(line);
				if (isEmptyLine)
				{
					continue;
				}

				int startIndex = 0;
				string name = string.Empty;
				string value = string.Empty;

				// we know the csv contains 9 columns
				for (int column = 0; column < 9; column++)
				{
					var endIndex = line.IndexOf(',', startIndex);
					if (column == 0) // the stock name
					{
						name = line[startIndex..endIndex];
					}
					else if (column == 8) // the value we want
					{
						if (endIndex == -1)
						{
							endIndex = line.Length;
						}

						value = line[startIndex..endIndex];
					}

					startIndex = endIndex + 1;
				}

				// remove unused values from parsing
				var trade = new Trade(DateTime.MinValue,
					decimal.Zero,
					decimal.Zero,
					decimal.Parse(value, CultureInfo.InvariantCulture));

				if (!Stocks.ContainsKey(name))
				{
					Stocks[name] = new Stock(name);
				}

				Stocks[name].Trades.Add(trade);
			}
		}
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
