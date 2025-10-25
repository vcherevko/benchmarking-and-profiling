namespace ProcessorStock;

class Program
{
	private static ProcessorFaster _processor;
	static void Main(string[] args)
	{
		_processor = new ProcessorFaster();
		_processor.Initialize();
		Processor();
	}

	public static List<string> Processor()
	{
		var result = new List<string>();

		foreach (var stock in _processor.Stocks)
		{
			var min = _processor.Min(stock.Key);
			var max = _processor.Max(stock.Key);
			var average = _processor.Average(stock.Key);

			result.Add($"{min} {max} {average}");
		}

		return result;
	}
}