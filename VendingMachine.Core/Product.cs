namespace VendingMachine.Core;

public sealed record Product
{
    public Product(string id, string name, int price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        Id = id;
        Name = name;
        Price = price;
    }

    public string Id { get; }
    public string Name { get; }
    public int Price { get; }
    public override string ToString() => $"{Id} {Name} ({Price}c)";
}
