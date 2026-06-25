using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlite("Data Source=pet.db"));

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseHttpsRedirection();

app.MapGet("/pet", async (PetService service) =>
{
    return await service.GetAsync();
});

app.MapPost("/pet/feed", async (PetService service) =>
{
    return await service.FeedAsync();
});

app.MapPost("/pet/play", async (PetService service) =>
{
    return await service.PlayAsync();
});

app.MapPost("/pet/sleep", async (PetService service) =>
{
    return await service.SleepAsync();
});

app.MapPost("/pet/wake", async (PetService service) =>
{
    return await service.WakeAsync();
});


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
