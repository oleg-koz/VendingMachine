namespace VendingMachine.Core;

public interface IChangeStrategy
{
    // Returns the coins to hand back, or null if the amount can't be made from what's available.
    Dictionary<int, int>? Calculate(int amount, Dictionary<int, int> available);
}
