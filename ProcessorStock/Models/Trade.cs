namespace ProcessorStock.Models;

public class Trade
{
	public Trade(DateTime date, decimal volume, decimal price, decimal change)
	{
		Date = date;
		Volume = volume;
		Price = price;
		Change = change;
	}

	public DateTime Date { get; set; }
	public decimal Volume { get; set; }
	public decimal Price { get; set; }
	public decimal Change { get; set; }
}