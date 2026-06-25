using Microsoft.EntityFrameworkCore;

public class PetUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public PetUpdateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PetDbContext>();

            var pet = await db.Pets.FirstOrDefaultAsync(stoppingToken);
            if (pet != null)
            {
                UpdateStats(pet);
                await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private void UpdateStats(Pet pet)
    {
        // Time-based decay
        pet.Hunger = Math.Min(100, pet.Hunger + 1);
        pet.Energy = Math.Max(0, pet.Energy - 1);

        if (pet.Hunger > 80)
            pet.Happiness = Math.Max(0, pet.Happiness - 1);

        if (pet.Energy < 20)
            pet.Happiness = Math.Max(0, pet.Happiness - 1);
        
        if (pet.State == "sleep" && pet.Energy < 100)
            pet.Energy = Math.Min(100, pet.Energy + 2);

        pet.LastUpdated = DateTime.UtcNow;
    }
}
