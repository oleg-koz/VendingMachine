using VendingMachine.Core;
using Xunit;

namespace VendingMachine.Tests;

public class VendingMachineServiceTests
{
    private static readonly Product Cola = new("B1", "Cola 0.33l", 145);
    private static readonly Product Akvile = new("A1", "Akvile 0.5l", 85);

    private static VendingMachineService Build(CoinBundle float_, params StockLine[] stock) =>
        new(new MachineState(float_, Inventory.FromLines(stock)), new MinimumCoinChangeStrategy());

    [Fact]
    public void Sells_an_item_and_gives_the_right_change()
    {
        // 1 EUR for an 85c item and 15c back, in as few coins as possible.
        var machine = Build(
            CoinBundle.Of((EuroCoins.TenCents, 5), (EuroCoins.FiveCents, 5)),
            new StockLine(Akvile, 3));

        var result = machine.Purchase("A1", CoinBundle.Of((EuroCoins.OneEuro, 1)));

        Assert.True(result.Succeeded);
        Assert.Equal(15, result.CoinsReturned.TotalValue);
        Assert.Equal(1, result.CoinsReturned[EuroCoins.TenCents]);
        Assert.Equal(1, result.CoinsReturned[EuroCoins.FiveCents]);
    }

    [Fact]
    public void Banks_the_payment_and_takes_one_off_the_shelf()
    {
        var machine = Build(
            CoinBundle.Of((EuroCoins.TenCents, 5), (EuroCoins.FiveCents, 5)),
            new StockLine(Akvile, 3));
        var floatBefore = machine.State.FloatValue;

        machine.Purchase("A1", CoinBundle.Of((EuroCoins.OneEuro, 1)));

        Assert.True(machine.State.Inventory.TryGet("A1", out var line));
        Assert.Equal(2, line.Quantity);
        Assert.Equal(floatBefore + 85, machine.State.FloatValue);
    }

    [Fact]
    public void Exact_money_gets_no_change_back()
    {
        var machine = Build(CoinBundle.Empty, new StockLine(Akvile, 1));

        var result = machine.Purchase("A1", CoinBundle.Of(
            (EuroCoins.FiftyCents, 1), (EuroCoins.TwentyCents, 1),
            (EuroCoins.TenCents, 1), (EuroCoins.FiveCents, 1)));

        Assert.True(result.Succeeded);
        Assert.True(result.CoinsReturned.IsEmpty);
    }

    [Fact]
    public void Change_can_come_from_the_coins_just_inserted()
    {
        // Empty float, so the 20c handed back can only be one of the two just inserted.
        var drink = new Product("D1", "Monster", 20);
        var machine = Build(CoinBundle.Empty, new StockLine(drink, 1));

        var result = machine.Purchase("D1", CoinBundle.Of((EuroCoins.TwentyCents, 2)));

        Assert.True(result.Succeeded);
        Assert.Equal(20, result.CoinsReturned.TotalValue);
    }

    [Fact]
    public void Refuses_an_unknown_selection_and_refunds()
    {
        var machine = Build(CoinBundle.Empty, new StockLine(Akvile, 1));

        var result = machine.Purchase("Z9", CoinBundle.Of((EuroCoins.OneEuro, 1)));

        Assert.Equal(PurchaseFailure.UnknownProduct, result.Failure);
        Assert.Equal(100, result.CoinsReturned.TotalValue);
    }

    [Fact]
    public void Refuses_an_empty_slot()
    {
        var machine = Build(CoinBundle.Empty, new StockLine(Akvile, 0));

        var result = machine.Purchase("A1", CoinBundle.Of((EuroCoins.OneEuro, 1)));

        Assert.Equal(PurchaseFailure.OutOfStock, result.Failure);
    }

    [Fact]
    public void Refuses_when_the_money_is_short()
    {
        var machine = Build(CoinBundle.Empty, new StockLine(Cola, 1));

        var result = machine.Purchase("B1", CoinBundle.Of((EuroCoins.OneEuro, 1)));

        Assert.Equal(PurchaseFailure.InsufficientPayment, result.Failure);
        Assert.Equal(100, result.CoinsReturned.TotalValue);
    }

    [Fact]
    public void A_refused_purchase_keeps_none_of_the_money()
    {
        // Owes 55c and nothing in the machine or the payment makes 55c.
        var machine = Build(CoinBundle.Of((EuroCoins.TwoEuros, 3)), new StockLine(Cola, 1));
        var floatBefore = machine.State.FloatValue;

        var result = machine.Purchase("B1", CoinBundle.Of((EuroCoins.TwoEuros, 1)));

        Assert.Equal(PurchaseFailure.InsufficientChange, result.Failure);
        Assert.Equal(200, result.CoinsReturned.TotalValue);
        Assert.Equal(floatBefore, machine.State.FloatValue);
        Assert.True(machine.State.Inventory.TryGet("B1", out var line));
        Assert.Equal(1, line.Quantity);
    }

    [Fact]
    public void Selection_codes_are_case_insensitive()
    {
        var machine = Build(CoinBundle.Empty, new StockLine(Akvile, 1));

        var result = machine.Purchase("a1", CoinBundle.Of(
            (EuroCoins.FiftyCents, 1), (EuroCoins.TwentyCents, 1),
            (EuroCoins.TenCents, 1), (EuroCoins.FiveCents, 1)));

        Assert.True(result.Succeeded);
    }
}
