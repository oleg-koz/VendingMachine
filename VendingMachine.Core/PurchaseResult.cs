namespace VendingMachine.Core;

public enum PurchaseFailure
{
    UnknownProduct,
    OutOfStock,
    InsufficientPayment,
    InsufficientChange
}

// Coins come back either way, as change on success, or as a refund on refusal, so they are one property.
public sealed record PurchaseResult
{
    private PurchaseResult(Product? product, CoinBundle coinsReturned, PurchaseFailure? failure)
    {
        Product = product;
        CoinsReturned = coinsReturned;
        Failure = failure;
    }

    public static PurchaseResult Dispensed(Product product, CoinBundle change) =>
        new(product, change, failure: null);

    public static PurchaseResult Rejected(PurchaseFailure reason, CoinBundle refund) =>
        new(product: null, refund, reason);

    public bool Succeeded => Failure is null;

    public Product? Product { get; }

    public CoinBundle CoinsReturned { get; }

    public PurchaseFailure? Failure { get; }
}
