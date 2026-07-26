namespace VendingMachine.Api.Contracts;

public class PurchaseRequest
{
    public string ProductId { get; set; } = "";

    // Denomination in cents, how many coins, e.g. { "100": 1, "50": 1 }
    public Dictionary<int, int> Coins { get; set; } = new();
}
