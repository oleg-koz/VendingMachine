using VendingMachine.Core;
using Xunit;

namespace VendingMachine.Tests;

public class ConcurrencyTests
{
    [Fact]
    public void A_synchronized_machine_never_oversells()
    {
        const int stock = 10;
        const int attempts = 200;

        var product = new Product("A1", "Akvile 0.5l", 50);
        var inner = new VendingMachineService(
            new MachineState(CoinBundle.Empty, Inventory.FromLines([new StockLine(product, stock)])),
            new MinimumCoinChangeStrategy());

        IVendingMachine machine = new SynchronizedMachine(inner);
        var sold = 0;

        Parallel.For(0, attempts, _ =>
        {
            var result = machine.Purchase("A1", CoinBundle.Of((EuroCoins.FiftyCents, 1)));
            if (result.Succeeded)
            {
                Interlocked.Increment(ref sold);
            }
        });

        Assert.Equal(stock, sold);
        Assert.True(machine.State.Inventory.TryGet("A1", out var line));
        Assert.Equal(0, line.Quantity);
    }
}
