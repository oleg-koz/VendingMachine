using VendingMachine.Api;
using VendingMachine.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IChangeStrategy, MinimumCoinChangeStrategy>();
builder.Services.AddSingleton<IVendingMachine>(provider =>
    new SynchronizedMachine(
        new VendingMachineService(
            MachineSeed.OpeningState(),
            provider.GetRequiredService<IChangeStrategy>())));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
