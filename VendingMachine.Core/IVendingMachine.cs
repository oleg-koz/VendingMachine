namespace VendingMachine.Core;

public interface IVendingMachine
{
    MachineState State { get; }

    PurchaseResult Purchase(string productId, CoinBundle payment);

    void LoadCoins(CoinBundle coins);

    void Restock(Product product, int quantity);
}
