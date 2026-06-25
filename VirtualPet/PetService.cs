using Microsoft.EntityFrameworkCore;

public class PetService
{
    private readonly PetDbContext _db;

    public PetService(PetDbContext db)
    {
        _db = db;
    }

    // Ensure the pet exists and return it
    public async Task<Pet> GetAsync()
    {
        var pet = await _db.Pets.FirstOrDefaultAsync();

        if (pet == null)
        {
            pet = new Pet();
            _db.Pets.Add(pet);
            await _db.SaveChangesAsync();
        }

        return pet;
    }

    // Feed the pet
    public async Task<Pet> FeedAsync()
    {
        var pet = await GetAsync();

        pet.Hunger = Math.Max(0, pet.Hunger - 20);
        pet.State = "eat";
        pet.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return pet;
    }

    // Play with the pet
    public async Task<Pet> PlayAsync()
    {
        var pet = await GetAsync();

        pet.Happiness = Math.Min(100, pet.Happiness + 15);
        pet.Energy = Math.Max(0, pet.Energy - 10);
        pet.State = "happy";
        pet.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return pet;
    }

    // Put the pet to sleep
    public async Task<Pet> SleepAsync()
    {
        var pet = await GetAsync();

        pet.State = "sleep";
        pet.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return pet;
    }

    // Wake the pet up
    public async Task<Pet> WakeAsync()
    {
        var pet = await GetAsync();

        pet.State = "idle";
        pet.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return pet;
    }
}
