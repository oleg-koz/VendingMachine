using VendingMachine.Core;

namespace VendingMachine.Api;

public static class MachineSeed
{
    public static MachineState OpeningState() => new(OpeningFloat, Inventory.FromLines(Catalogue));

    private static readonly StockLine[] Catalogue =
    [
        new(new Product("A1", "Akvile 0.5l", 85), 10),
        new(new Product("A2", "Vytautas 0.5l", 95), 10),
        new(new Product("B1", "Cola 0.33l", 145), 8),
        new(new Product("B2", "Elmenhoster 0.33l", 145), 8),
        new(new Product("C1", "Nestea 0.5l", 165), 6),
        new(new Product("C2", "Redbull 0.33l", 190), 4)
    ];

    // Once the small coins run down the machine starts refusing sales it can't make change.
    private static readonly CoinBundle OpeningFloat = CoinBundle.Of(
        (EuroCoins.OneCent, 20),
        (EuroCoins.TwoCents, 20),
        (EuroCoins.FiveCents, 30),
        (EuroCoins.TenCents, 40),
        (EuroCoins.TwentyCents, 40),
        (EuroCoins.FiftyCents, 20),
        (EuroCoins.OneEuro, 10),
        (EuroCoins.TwoEuros, 5));
}
