using VendingMachine.Core;

namespace VendingMachine.Api.Contracts;

public sealed record ProductResponse(string Id, string Name, int Price, int Quantity);

public sealed record PurchaseResponse(
    bool Dispensed,
    string? ProductName,
    Dictionary<int, int> CoinsReturned,
    int CoinsReturnedValue,
    string? Failure);

internal static class CoinMapping
{
    public static bool TryToBundle(Dictionary<int, int>? coins, out CoinBundle bundle, out string? error)
    {
        bundle = CoinBundle.Empty;
        error = null;

        if (coins is null || coins.Count == 0)
        {
            return true;
        }

        foreach (var (denomination, count) in coins)
        {
            if (denomination <= 0)
            {
                error = $"'{denomination}' is not a coin.";
                return false;
            }

            if (count < 0)
            {
                error = $"Count for {denomination}c cannot be negative.";
                return false;
            }
        }

        bundle = CoinBundle.FromCounts(
            coins.Select(pair => KeyValuePair.Create(new Denomination(pair.Key), pair.Value)));

        return true;
    }

    public static Dictionary<int, int> ToMap(CoinBundle bundle) =>
        bundle.ToDictionary(pair => pair.Key.Cents, pair => pair.Value);
}
