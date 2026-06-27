using Microsoft.EntityFrameworkCore;

public class PetService
{
    private const int MaxFeedingsPerWindow = 3;
    private static readonly TimeSpan FeedingWindowDuration = TimeSpan.FromHours(12);

    private readonly PetDbContext _db;

    public PetService(PetDbContext db)
    {
        _db = db;
    }

    // Ensure the pet exists and return it
    private async Task<Pet> GetPetEntityAsync()
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

    public async Task<PetResponse> GetAsync()
    {
        var pet = await GetPetEntityAsync();
        var now = DateTime.UtcNow;

        if (ResetFeedingWindowIfExpired(pet, now))
        {
            await _db.SaveChangesAsync();
        }

        return ToResponse(pet, now);
    }

    // Feed the pet
    public async Task<FeedResult> FeedAsync()
    {
        var pet = await GetPetEntityAsync();
        var now = DateTime.UtcNow;

        ResetFeedingWindowIfExpired(pet, now);

        if (pet.FeedingsUsedInWindow >= MaxFeedingsPerWindow)
        {
            return new FeedResult
            {
                Succeeded = false,
                ErrorMessage = "Feed limit reached. Try again when the 12-hour window resets.",
                Pet = ToResponse(pet, now)
            };
        }

        pet.FeedingWindowStartUtc ??= now;
        pet.FeedingsUsedInWindow++;

        pet.Hunger = Math.Max(0, pet.Hunger - 20);
        pet.State = "eat";
        pet.LastUpdated = now;

        await _db.SaveChangesAsync();

        return new FeedResult
        {
            Succeeded = true,
            Pet = ToResponse(pet, now)
        };
    }

    // Play with the pet
    public async Task<PetResponse> PlayAsync()
    {
        var pet = await GetPetEntityAsync();
        var now = DateTime.UtcNow;

        pet.Happiness = Math.Min(100, pet.Happiness + 15);
        pet.Energy = Math.Max(0, pet.Energy - 10);
        pet.State = "happy";
        pet.LastUpdated = now;

        await _db.SaveChangesAsync();
        return ToResponse(pet, now);
    }

    // Put the pet to sleep
    public async Task<PetResponse> SleepAsync()
    {
        var pet = await GetPetEntityAsync();
        var now = DateTime.UtcNow;

        pet.State = "sleep";
        pet.LastUpdated = now;

        await _db.SaveChangesAsync();
        return ToResponse(pet, now);
    }

    // Wake the pet up
    public async Task<PetResponse> WakeAsync()
    {
        var pet = await GetPetEntityAsync();
        var now = DateTime.UtcNow;

        pet.State = "idle";
        pet.LastUpdated = now;

        await _db.SaveChangesAsync();
        return ToResponse(pet, now);
    }

    private static bool ResetFeedingWindowIfExpired(Pet pet, DateTime now)
    {
        if (!pet.FeedingWindowStartUtc.HasValue)
        {
            if (pet.FeedingsUsedInWindow == 0)
            {
                return false;
            }

            pet.FeedingsUsedInWindow = 0;
            return true;
        }

        if (now - pet.FeedingWindowStartUtc.Value < FeedingWindowDuration)
        {
            return false;
        }

        pet.FeedingWindowStartUtc = null;
        pet.FeedingsUsedInWindow = 0;
        return true;
    }

    private static PetResponse ToResponse(Pet pet, DateTime now)
    {
        var windowIsActive = pet.FeedingWindowStartUtc.HasValue
            && now - pet.FeedingWindowStartUtc.Value < FeedingWindowDuration;
        var usedFeedings = windowIsActive ? pet.FeedingsUsedInWindow : 0;

        return new PetResponse
        {
            Id = pet.Id,
            Name = pet.Name,
            Hunger = pet.Hunger,
            Energy = pet.Energy,
            Happiness = pet.Happiness,
            State = pet.State,
            LastUpdated = pet.LastUpdated,
            Mood = pet.Mood,
            RemainingFeedings = Math.Max(0, MaxFeedingsPerWindow - usedFeedings),
            FeedingsUsedInWindow = usedFeedings,
            FeedingWindowResetsAtUtc = windowIsActive
                ? pet.FeedingWindowStartUtc!.Value.Add(FeedingWindowDuration)
                : null
        };
    }
}
