using System.Collections;

namespace VendingMachine.Core;

// An immutable pile of coins - the float, a payment, and the change handed back are all one of these.
// Every operation returns a new bundle, which is what stops a failed purchase leaving the float half-updated.
public sealed class CoinBundle : IEnumerable<KeyValuePair<Denomination, int>>
{
    public static readonly CoinBundle Empty = new(new Dictionary<Denomination, int>());

    private readonly Dictionary<Denomination, int> _counts;

    // The change search walks denominations high to low repeatedly, so sort once.
    private readonly Denomination[] _descending;

    private CoinBundle(Dictionary<Denomination, int> counts)
    {
        _counts = counts;
        _descending = [.. counts.Keys.OrderByDescending(d => d.Cents)];
        TotalValue = counts.Sum(pair => pair.Key.Cents * pair.Value);
        TotalCoins = counts.Values.Sum();
    }

    public static CoinBundle Of(params (Denomination Denomination, int Count)[] coins) =>
        FromCounts(coins.Select(c => KeyValuePair.Create(c.Denomination, c.Count)));

    public static CoinBundle FromCounts(IEnumerable<KeyValuePair<Denomination, int>> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);

        var totals = new Dictionary<Denomination, int>();
        foreach (var (denomination, count) in counts)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (count == 0)
            {
                continue;
            }

            totals[denomination] = totals.GetValueOrDefault(denomination) + count;
        }

        return totals.Count == 0 ? Empty : new CoinBundle(totals);
    }

    public int this[Denomination denomination] => _counts.GetValueOrDefault(denomination);

    public int TotalValue { get; }

    public int TotalCoins { get; }

    public bool IsEmpty => TotalCoins == 0;

    public IReadOnlyList<Denomination> DenominationsDescending => _descending;

    public CoinBundle Add(CoinBundle other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other.IsEmpty)
        {
            return this;
        }

        var totals = new Dictionary<Denomination, int>(_counts);
        foreach (var (denomination, count) in other._counts)
        {
            totals[denomination] = totals.GetValueOrDefault(denomination) + count;
        }

        return new CoinBundle(totals);
    }

    // Try style because the caller has already established the coins are there.
    public bool TryRemove(CoinBundle other, out CoinBundle result)
    {
        ArgumentNullException.ThrowIfNull(other);

        var totals = new Dictionary<Denomination, int>(_counts);
        foreach (var (denomination, count) in other._counts)
        {
            var remaining = totals.GetValueOrDefault(denomination) - count;
            if (remaining < 0)
            {
                result = this;
                return false;
            }

            if (remaining == 0)
            {
                totals.Remove(denomination);
            }
            else
            {
                totals[denomination] = remaining;
            }
        }

        result = totals.Count == 0 ? Empty : new CoinBundle(totals);
        return true;
    }

    public IEnumerator<KeyValuePair<Denomination, int>> GetEnumerator()
    {
        foreach (var denomination in _descending)
        {
            yield return KeyValuePair.Create(denomination, _counts[denomination]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() =>
        IsEmpty ? "(no coins)" : string.Join(", ", this.Select(p => $"{p.Value} x {p.Key}"));
}
