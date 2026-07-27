namespace VendingMachine.Core;

public sealed class VendingMachineService : IVendingMachine
{
    private readonly IChangeStrategy _changeStrategy;
    private MachineState _state;

    public VendingMachineService(MachineState initialState, IChangeStrategy changeStrategy)
    {
        ArgumentNullException.ThrowIfNull(initialState);
        ArgumentNullException.ThrowIfNull(changeStrategy);

        _state = initialState;
        _changeStrategy = changeStrategy;
    }

    public MachineState State => _state;

    public PurchaseResult Purchase(string productId, CoinBundle payment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentNullException.ThrowIfNull(payment);

        var state = _state;

        if (!state.Inventory.TryGet(productId, out var line))
        {
            return PurchaseResult.Rejected(PurchaseFailure.UnknownProduct, payment);
        }

        if (line.Quantity == 0)
        {
            return PurchaseResult.Rejected(PurchaseFailure.OutOfStock, payment);
        }

        if (payment.TotalValue < line.Product.Price)
        {
            return PurchaseResult.Rejected(PurchaseFailure.InsufficientPayment, payment);
        }

        // The inserted coins join the float before change is worked out.
        var floatWithPayment = state.Float.Add(payment);
        var change = _changeStrategy.Calculate(payment.TotalValue - line.Product.Price, floatWithPayment);

        if (change is null)
        {
            return PurchaseResult.Rejected(PurchaseFailure.InsufficientChange, payment);
        }

        if (!floatWithPayment.TryRemove(change, out var remainingFloat))
        {
            throw new InvalidOperationException("Change strategy returned coins the machine does not hold.");
        }

        if (!state.Inventory.TryTakeOne(productId, out var remainingStock))
        {
            throw new InvalidOperationException($"Stock for '{productId}' vanished mid-purchase.");
        }

        // Every refusal returns before this line, so a failed purchase can't bank coins without dispensing.
        _state = state with { Float = remainingFloat, Inventory = remainingStock };

        return PurchaseResult.Dispensed(line.Product, change);
    }

    public void LoadCoins(CoinBundle coins)
    {
        ArgumentNullException.ThrowIfNull(coins);
        _state = _state with { Float = _state.Float.Add(coins) };
    }

    public void Restock(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        _state = _state with { Inventory = _state.Inventory.AddStock(product, quantity) };
    }
}
