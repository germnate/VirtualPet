using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PetDb") ?? "Data Source=pet.db";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<PetService>();
builder.Services.AddHostedService<PetUpdateService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PetDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

if (builder.Configuration.GetValue("UseHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/pet", async (PetService service) =>
{
    return Results.Ok(await service.GetAsync());
});

app.MapPost("/pet/feed", async (PetService service) =>
{
    var result = await service.FeedAsync();

    if (result.Succeeded)
    {
        return Results.Ok(result.Pet);
    }

    return Results.Json(
        new { message = result.ErrorMessage, pet = result.Pet },
        statusCode: StatusCodes.Status429TooManyRequests);
});

app.MapPost("/pet/play", async (PetService service) =>
{
    return Results.Ok(await service.PlayAsync());
});

app.MapPost("/pet/sleep", async (PetService service) =>
{
    return Results.Ok(await service.SleepAsync());
});

app.MapPost("/pet/wake", async (PetService service) =>
{
    return Results.Ok(await service.WakeAsync());
});

app.MapFallbackToFile("index.html");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
