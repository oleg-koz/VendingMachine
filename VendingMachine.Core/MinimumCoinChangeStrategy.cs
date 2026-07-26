namespace VendingMachine.Core;

// Hands back the amount in as few coins as possible.
//
// The obvious approach - take the biggest coin that fits, repeat - only works when the supply
// of coins is unlimited. A vending machine's float is finite, so it fails: owe 6c from 1x5c and
// 3x2c and it grabs the 5c, then needs a 1c that isn't there, while 3x2c was available.
//
// So this is bounded coin change. dp[i, j] is the fewest coins that make j using the first i
// denominations, trying every feasible count of the current coin. Roughly
// O(amount * sum of min(count, amount / value)), which is nothing at the amounts a vending
// machine deals in.
public class MinimumCoinChangeStrategy : IChangeStrategy
{
    private const int Unreachable = int.MaxValue;

    public Dictionary<int, int>? Calculate(int amount, Dictionary<int, int> available)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (amount == 0)
        {
            return new Dictionary<int, int>();
        }

        var denominations = available
            .Where(coin => coin.Value > 0)
            .Select(coin => coin.Key)
            .OrderByDescending(value => value)
            .ToArray();

        var dp = new int[denominations.Length + 1, amount + 1];
        var taken = new int[denominations.Length + 1, amount + 1];

        for (var j = 1; j <= amount; j++)
        {
            dp[0, j] = Unreachable;
        }

        for (var i = 1; i <= denominations.Length; i++)
        {
            var value = denominations[i - 1];
            var stock = available[value];

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

        if (dp[denominations.Length, amount] == Unreachable)
        {
            return null;
        }

        // Walk the choices back out of the table.
        var change = new Dictionary<int, int>();
        var remaining = amount;
        for (var i = denominations.Length; i >= 1 && remaining > 0; i--)
        {
            var used = taken[i, remaining];
            if (used == 0)
            {
                continue;
            }

            change[denominations[i - 1]] = used;
            remaining -= used * denominations[i - 1];
        }

        return change;
    }
}
