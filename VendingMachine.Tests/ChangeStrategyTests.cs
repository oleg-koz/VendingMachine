using VendingMachine.Core;
using Xunit;

namespace VendingMachine.Tests;

public class ChangeStrategyTests
{
    private readonly IChangeStrategy _calculator = new MinimumCoinChangeStrategy();

    [Fact]
    public void Gives_nothing_back_when_no_change_is_due()
    {
        var change = _calculator.Calculate(0, new Dictionary<int, int> { [10] = 5 });

        Assert.NotNull(change);
        Assert.Empty(change);
    }

    [Fact]
    public void Gives_change_that_adds_up()
    {
        var available = new Dictionary<int, int> { [50] = 2, [20] = 3, [10] = 5, [5] = 5 };

        var change = _calculator.Calculate(55, available);

        Assert.NotNull(change);
        Assert.Equal(55, change.Sum(c => c.Key * c.Value));
    }

    [Fact]
    public void Uses_the_fewest_coins_it_can()
    {
        var available = new Dictionary<int, int> { [20] = 5, [10] = 5, [5] = 10 };

        var change = _calculator.Calculate(30, available);

        Assert.NotNull(change);
        Assert.Equal(2, change.Sum(c => c.Value));
    }

    [Fact]
    public void Returns_null_when_the_float_is_worth_less_than_the_change()
    {
        var change = _calculator.Calculate(100, new Dictionary<int, int> { [10] = 2 });

        Assert.Null(change);
    }

    [Fact]
    public void Only_uses_coins_the_machine_actually_has()
    {
        var available = new Dictionary<int, int> { [50] = 1, [10] = 1 };

        var change = _calculator.Calculate(60, available);

        Assert.NotNull(change);
        Assert.Equal(1, change[50]);
        Assert.Equal(1, change[10]);
    }

    [Fact]
    public void Finds_change_that_needs_the_smaller_coins()
    {
        // Owe 6c. Taking the biggest coin first grabs the 5c and then needs a 1c, which the machine doesn't have, but 3x2c makes 6c exactly
        var available = new Dictionary<int, int> { [5] = 1, [2] = 3 };

        var change = _calculator.Calculate(6, available);

        Assert.NotNull(change);
        Assert.Equal(6, change.Sum(c => c.Key * c.Value));
    }
}
