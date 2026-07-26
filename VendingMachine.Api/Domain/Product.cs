namespace VendingMachine.Api.Domain;

public class Product
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public int Quantity { get; set; }
}
