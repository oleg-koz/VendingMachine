namespace VendingMachine.Core;

public class VendingMachineService
{
    // Denomination in cents, how many coins the machine holds.
    private readonly Dictionary<int, int> _coins = new()
    {
        [1] = 20,
        [2] = 20,
        [5] = 30,
        [10] = 40,
        [20] = 40,
        [50] = 20,
        [100] = 10,
        [200] = 5
    };

    private readonly Dictionary<string, Product> _products;

    public VendingMachineService()
    {
        // Hardcoded for now, this would come from a database or config eventually.
        var catalogue = new[]
        {
            new Product { Id = "A1", Name = "Akvile 0.5l", Price = 85, Quantity = 10 },
            new Product { Id = "A2", Name = "Vytautas 0.5l", Price = 95, Quantity = 10 },
            new Product { Id = "B1", Name = "Cola 0.33l", Price = 145, Quantity = 8 },
            new Product { Id = "B2", Name = "Elmenhoster 0.33l", Price = 145, Quantity = 8 },
            new Product { Id = "C1", Name = "Nestea 0.5l", Price = 165, Quantity = 6 },
            new Product { Id = "C2", Name = "Redbull 0.33l", Price = 190, Quantity = 4 }
        };

        _products = catalogue.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<Product> GetProducts() => _products.Values.OrderBy(p => p.Id);

    public PurchaseResult Purchase(string productId, Dictionary<int, int> payment)
    {
        if (!_products.TryGetValue(productId, out var product))
        {
            throw new ArgumentException($"No product '{productId}'.");
        }

        if (product.Quantity == 0)
        {
            throw new InvalidOperationException($"'{productId}' is sold out.");
        }

        var paid = payment.Sum(p => p.Key * p.Value);
        if (paid < product.Price)
        {
            throw new InvalidOperationException($"'{productId}' costs {product.Price}c, paid {paid}c.");
        }

        // The coins the customer inserted go into the machine, so they can be given out as
        // change to whoever comes next.
        foreach (var (denomination, count) in payment)
        {
            _coins[denomination] = _coins.GetValueOrDefault(denomination) + count;
        }

        var change = MakeChange(paid - product.Price);
        product.Quantity--;

        return new PurchaseResult { ProductName = product.Name, Change = change };
    }

    private Dictionary<int, int> MakeChange(int amount)
    {
        var change = new Dictionary<int, int>();

        foreach (var denomination in _coins.Keys.OrderByDescending(d => d))
        {
            while (amount >= denomination && _coins[denomination] > 0)
            {
                change[denomination] = change.GetValueOrDefault(denomination) + 1;
                _coins[denomination]--;
                amount -= denomination;
            }
        }

        if (amount > 0)
        {
            throw new InvalidOperationException("Cannot make change.");
        }

        return change;
    }
}
