namespace VendingMachine.Core;

public class PurchaseResult
{
    public string ProductName { get; set; } = "";

    // Denomination in cents, how many coins.
    public Dictionary<int, int> Change { get; set; } = new();

    public int ChangeValue => Change.Sum(c => c.Key * c.Value);
}
