namespace VendingMachine.Core;

// Serialises access to a machine.
public sealed class SynchronizedMachine : IVendingMachine
{
    private readonly Lock _gate = new();
    private readonly IVendingMachine _inner;

    public SynchronizedMachine(IVendingMachine inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    public MachineState State
    {
        get
        {
            lock (_gate)
            {
                return _inner.State;
            }
        }
    }

    public PurchaseResult Purchase(string productId, CoinBundle payment)
    {
        lock (_gate)
        {
            return _inner.Purchase(productId, payment);
        }
    }

    public void LoadCoins(CoinBundle coins)
    {
        lock (_gate)
        {
            _inner.LoadCoins(coins);
        }
    }

    public void Restock(Product product, int quantity)
    {
        lock (_gate)
        {
            _inner.Restock(product, quantity);
        }
    }
}
