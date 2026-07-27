namespace VendingMachine.Core;

// Coins and stock in one record so a purchase replaces both as a single value.
public sealed record MachineState(CoinBundle Float, Inventory Inventory)
{
    public int FloatValue => Float.TotalValue;
}
