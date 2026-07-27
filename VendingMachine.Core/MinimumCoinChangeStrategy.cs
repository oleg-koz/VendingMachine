namespace VendingMachine.Core;

// Hands back the amount in as few coins as possible.
// So this is bounded coin change. dp[i, j] is the fewest coins making j from the first i denominations,
// trying every feasible count of the current coin.
// Roughly O(amount * sum of min(count, amount / value)) - nothing at vending machine amounts.
public sealed class MinimumCoinChangeStrategy : IChangeStrategy
{
    private const int Unreachable = int.MaxValue;

    public CoinBundle? Calculate(int amount, CoinBundle available)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentNullException.ThrowIfNull(available);

        if (amount == 0)
        {
            return CoinBundle.Empty;
        }

        // Cheap rejection before allocating the table.
        if (amount > available.TotalValue)
        {
            return null;
        }

        var denominations = available.DenominationsDescending;
        var count = denominations.Count;

        var dp = new int[count + 1, amount + 1];
        var taken = new int[count + 1, amount + 1];

        for (var j = 1; j <= amount; j++)
        {
            dp[0, j] = Unreachable;
        }

        for (var i = 1; i <= count; i++)
        {
            var denomination = denominations[i - 1];
            var value = denomination.Cents;
            var stock = available[denomination];

            for (var j = 0; j <= amount; j++)
            {
                var best = Unreachable;
                var bestCount = 0;

                var maxUsable = Math.Min(stock, j / value);
                for (var k = 0; k <= maxUsable; k++)
                {
                    var previous = dp[i - 1, j - (k * value)];
                    if (previous == Unreachable)
                    {
                        continue;
                    }

                    if (previous + k < best)
                    {
                        best = previous + k;
                        bestCount = k;
                    }
                }

                dp[i, j] = best;
                taken[i, j] = bestCount;
            }
        }

        if (dp[count, amount] == Unreachable)
        {
            return null;
        }

        // Walk the choices back out of the table.
        var coins = new List<KeyValuePair<Denomination, int>>();
        var remaining = amount;
        for (var i = count; i >= 1 && remaining > 0; i--)
        {
            var used = taken[i, remaining];
            if (used == 0)
            {
                continue;
            }

            coins.Add(KeyValuePair.Create(denominations[i - 1], used));
            remaining -= used * denominations[i - 1].Cents;
        }

        return CoinBundle.FromCounts(coins);
    }
}
