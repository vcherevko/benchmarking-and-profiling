namespace ProcessorStock.Models;

public class Stock
{
	public Stock(string name)
	{
		Name = name;
	}

	public string Name { get; set; }

	public IList<Trade> Trades { get; } = new List<Trade>();

	public decimal Min { get; set; } = decimal.MinValue;
	public decimal Max { get; set; } = decimal.MaxValue;
	public decimal Total { get; set; } = 0;
	public decimal Average => Total / Trades.Count;
}