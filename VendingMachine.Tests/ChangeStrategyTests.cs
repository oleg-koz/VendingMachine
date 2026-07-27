using VendingMachine.Core;
using Xunit;

namespace VendingMachine.Tests;

public class ChangeStrategyTests
{
    private readonly IChangeStrategy _strategy = new MinimumCoinChangeStrategy();

    [Fact]
    public void Gives_nothing_back_when_no_change_is_due()
    {
        var change = _strategy.Calculate(0, CoinBundle.Of((EuroCoins.TenCents, 5)));

        Assert.NotNull(change);
        Assert.True(change.IsEmpty);
    }

    [Fact]
    public void Gives_change_that_adds_up()
    {
        var available = CoinBundle.Of(
            (EuroCoins.FiftyCents, 2), (EuroCoins.TwentyCents, 3),
            (EuroCoins.TenCents, 5), (EuroCoins.FiveCents, 5));

        var change = _strategy.Calculate(55, available);

        Assert.NotNull(change);
        Assert.Equal(55, change.TotalValue);
    }

    [Fact]
    public void Uses_the_fewest_coins_it_can()
    {
        var available = CoinBundle.Of(
            (EuroCoins.TwentyCents, 5), (EuroCoins.TenCents, 5), (EuroCoins.FiveCents, 10));

        var change = _strategy.Calculate(30, available);

        Assert.NotNull(change);
        Assert.Equal(2, change.TotalCoins);
    }

    [Fact]
    public void Returns_null_when_the_float_is_worth_less_than_the_change()
    {
        var change = _strategy.Calculate(100, CoinBundle.Of((EuroCoins.TenCents, 2)));

        Assert.Null(change);
    }

    [Fact]
    public void Returns_null_when_the_amount_cannot_be_assembled()
    {
        var change = _strategy.Calculate(3, CoinBundle.Of((EuroCoins.FiveCents, 10)));

        Assert.Null(change);
    }

    [Fact]
    public void Finds_change_that_needs_the_smaller_coins()
    {
        // Owe 6c. Taking the biggest coin first grabs the 5c and then needs a 1c, which the machine doesn't have, but 3x2c makes 6c exactly
        var available = CoinBundle.Of((EuroCoins.FiveCents, 1), (EuroCoins.TwoCents, 3));

        var change = _strategy.Calculate(6, available);

        Assert.NotNull(change);
        Assert.Equal(6, change.TotalValue);
        Assert.Equal(3, change[EuroCoins.TwoCents]);
    }
}
