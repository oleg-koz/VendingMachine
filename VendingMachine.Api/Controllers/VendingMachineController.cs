using Microsoft.AspNetCore.Mvc;
using VendingMachine.Api.Contracts;
using VendingMachine.Core;

namespace VendingMachine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendingMachineController : ControllerBase
{
    private readonly VendingMachineService _service;

    public VendingMachineController(VendingMachineService service)
    {
        _service = service;
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        return Ok(_service.GetProducts());
    }

    [HttpPost("purchase")]
    public IActionResult Purchase([FromBody] PurchaseRequest request)
    {
        // TODO: the service throws for sold out / not enough money / no change available
        var result = _service.Purchase(request.ProductId, request.Coins);
        return Ok(result);
    }
}
