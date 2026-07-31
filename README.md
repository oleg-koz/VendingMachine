# Vending Machine

Models a vending machine that sells items and gives change from a limited float.
.NET 10, ASP.NET Core MVC.

| Project | Contents |
|---|---|
| `VendingMachine.Core` | Domain model and change algorithm. No framework dependencies. |
| `VendingMachine.Api` | Controllers over the domain. |
| `VendingMachine.Tests` | xUnit tests. |

## Running

```
dotnet test
dotnet run --project VendingMachine.Api
```

```powershell
# catalogue with remaining stock
Invoke-RestMethod http://localhost:5177/api/vendingmachine/products | Format-Table

# 150c for a 145c cola - 5c back
$body = @{ productId = "B1"; coins = @{ "100" = 1; "50" = 1 } } | ConvertTo-Json
Invoke-RestMethod http://localhost:5177/api/vendingmachine/purchase -Method Post -Body $body -ContentType application/json
```
Sample requests for every path, including the failures, are in `VendingMachine.Api.http`.

Coins go in and come back as denomination-in-cents to count: `{"100": 1, "50": 1}`.
`404` unknown selection, `400` not enough money, `409` out of stock or no change available.
A refusal returns the refund in the body, so the caller settles up either way.

## Design notes

**Change is dynamic programming, not greedy.** The float is finite, which breaks it: owing 6c from `1x5c, 3x2c`, greedy takes the 5c, then needs a 1c that isn't there, having missed `3x2c`.
The bounded coin change DP finds a solution whenever one exists and minimises the coins dispensed. It sits behind `IChangeStrategy` so the machine keeps giving change longer.

**Coins and stock are immutable and live in one `MachineState`.** A purchase replaces both as a single value at the end, after every check has passed, so it can't bank a payment without dispensing.
Sold out, not enough money and no change available are returned as results.

**`SynchronizedMachine` wraps the machine for the web case.** The singleton is shared across requests, and without the lock two purchases can read the same state and one overwrites the other.
A plain lock rather than compare and swap. One machine serves one customer at a time.

## Assumptions

- Coins only, because notes would need a separate store since they can't be dispensed as change.
- Payment arrives already counted.
- Inserted coins join the float before change is calculated, so they can be handed straight back.
- A refused purchase refunds in full. No balance is held between attempts.
- An emptied slot stays at zero so the machine can say "out of stock".

## Limitations

- State is in memory and dies with the process one machine per process (several would need a registry key by id). 
- No HTTP-level integration tests, since the controller is a thin mapping layer over a domain that is covered directly. The change strategy also optimizes one sale at a time, with no view of the float's long-term health.
