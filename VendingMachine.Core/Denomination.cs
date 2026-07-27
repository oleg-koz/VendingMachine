namespace VendingMachine.Core;

// A coin denomination in cents. A struct rather than an enum so the coin set stays data.
public readonly record struct Denomination : IComparable<Denomination>
{
    public Denomination(int cents)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cents);
        Cents = cents;
    }

    public int Cents { get; }

    public int CompareTo(Denomination other) => Cents.CompareTo(other.Cents);

    public static bool operator <(Denomination left, Denomination right) => left.CompareTo(right) < 0;
    public static bool operator >(Denomination left, Denomination right) => left.CompareTo(right) > 0;
    public static bool operator <=(Denomination left, Denomination right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Denomination left, Denomination right) => left.CompareTo(right) >= 0;

    public override string ToString() => Cents >= 100 ? $"{Cents / 100m:0.00}EUR" : $"{Cents}c";
}
