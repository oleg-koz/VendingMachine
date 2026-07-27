using System.Collections;

namespace VendingMachine.Core;

public sealed record StockLine(Product Product, int Quantity);

// Immutable for the same reason as CoinBundle, a purchase either moves the machine to a consistent new state or leaves the old one alone.
public sealed class Inventory : IEnumerable<StockLine>
{
    public static readonly Inventory Empty = new(new Dictionary<string, StockLine>(StringComparer.OrdinalIgnoreCase));

    private readonly Dictionary<string, StockLine> _lines;

    private Inventory(Dictionary<string, StockLine> lines) => _lines = lines;

    public static Inventory FromLines(IEnumerable<StockLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        // Case insensitive, so a1 and A1 are the same slot
        var byId = new Dictionary<string, StockLine>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(line.Quantity);

            if (!byId.TryAdd(line.Product.Id, line))
            {
                throw new ArgumentException($"Duplicate product id '{line.Product.Id}'.", nameof(lines));
            }
        }

        return byId.Count == 0 ? Empty : new Inventory(byId);
    }

    public bool TryGet(string productId, out StockLine line) => _lines.TryGetValue(productId, out line!);

    // Rereads the product, so a restock can also change the price
    public Inventory AddStock(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        var lines = new Dictionary<string, StockLine>(_lines, StringComparer.OrdinalIgnoreCase);
        var existing = lines.GetValueOrDefault(product.Id);
        lines[product.Id] = new StockLine(product, (existing?.Quantity ?? 0) + quantity);

        return new Inventory(lines);
    }

    public bool TryTakeOne(string productId, out Inventory result)
    {
        if (!_lines.TryGetValue(productId, out var line) || line.Quantity == 0)
        {
            result = this;
            return false;
        }

        var lines = new Dictionary<string, StockLine>(_lines, StringComparer.OrdinalIgnoreCase)
        {
            [productId] = line with { Quantity = line.Quantity - 1 }
        };

        result = new Inventory(lines);
        return true;
    }

    public IEnumerator<StockLine> GetEnumerator() =>
        _lines.Values.OrderBy(l => l.Product.Id, StringComparer.OrdinalIgnoreCase).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
