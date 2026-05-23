var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        service = "customer-service",
        status = "healthy",
        timestamp = DateTimeOffset.UtcNow
    });
});

app.Run();
