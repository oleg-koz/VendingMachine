using Microsoft.AspNetCore.Mvc;
using VendingMachine.Api.Contracts;
using VendingMachine.Core;

namespace VendingMachine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendingMachineController : ControllerBase
{
    private readonly IVendingMachine _service;

    public VendingMachineController(IVendingMachine service)
    {
        _service = service;
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        var products = _service.State.Inventory
            .Select(line => new ProductResponse(line.Product.Id, line.Product.Name, line.Product.Price, line.Quantity));

        return Ok(products);
    }

    [HttpPost("purchase")]
    public IActionResult Purchase([FromBody] PurchaseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId))
        {
            return BadRequest(new ProblemDetails { Title = "A product id is required." });
        }

        if (!CoinMapping.TryToBundle(request.Coins, out var payment, out var error))
        {
            return BadRequest(new ProblemDetails { Title = error });
        }

        var result = _service.Purchase(request.ProductId, payment);

        // The refusal body carries the refund too.
        var body = new PurchaseResponse(
            result.Succeeded,
            result.Product?.Name,
            CoinMapping.ToMap(result.CoinsReturned),
            result.CoinsReturned.TotalValue,
            result.Failure?.ToString());

        return result.Failure switch
        {
            null => Ok(body),
            PurchaseFailure.UnknownProduct => NotFound(body),
            PurchaseFailure.InsufficientPayment => BadRequest(body),
            PurchaseFailure.OutOfStock => Conflict(body),
            PurchaseFailure.InsufficientChange => Conflict(body),
            _ => StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
