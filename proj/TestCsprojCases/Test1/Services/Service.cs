namespace Test1.Services;

public class DataService : IService
{
	public void Execute()
	{
		Console.WriteLine("Executing service");
	}

	public string GetResult()
	{
		return "Service result";
	}

	public event EventHandler Completed;

	protected virtual void OnCompleted()
	{
		Completed?.Invoke(this, EventArgs.Empty);
	}
}

public class ValidationService
{
	public bool ValidateInput(string input)
	{
		return !string.IsNullOrWhiteSpace(input);
	}

	public void ProcessData(string data)
	{
		if (ValidateInput(data))
		{
			Console.WriteLine($"Processing: {data}");
		}
	}

	public event Action<string> DataProcessed;
}
