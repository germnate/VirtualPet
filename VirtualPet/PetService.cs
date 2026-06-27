using Microsoft.EntityFrameworkCore;

public class PetService
{
    private const int MaxFeedingsPerWindow = 3;
    private const int HungerReductionPerFeeding = 15;

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

        if (ResetFeedingWindowIfNeeded(pet, now))
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

        ResetFeedingWindowIfNeeded(pet, now);

        if (pet.FeedingsUsedInWindow >= MaxFeedingsPerWindow)
        {
            return new FeedResult
            {
                Succeeded = false,
                ErrorMessage = "Feed limit reached. Try again after the next feeding reset.",
                Pet = ToResponse(pet, now)
            };
        }

        pet.FeedingWindowStartUtc = GetFeedingWindow(now).WindowStartUtc;
        pet.FeedingsUsedInWindow++;

        pet.Hunger = Math.Max(0, pet.Hunger - HungerReductionPerFeeding);
        if (pet.Hunger == 0)
        {
            pet.Health = Pet.MaxHealth;
        }

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

    private static bool ResetFeedingWindowIfNeeded(Pet pet, DateTime now)
    {
        var window = GetFeedingWindow(now);

        if (!pet.FeedingWindowStartUtc.HasValue)
        {
            pet.FeedingWindowStartUtc = window.WindowStartUtc;
            pet.FeedingsUsedInWindow = 0;
            return true;
        }

        if (pet.FeedingWindowStartUtc.Value == window.WindowStartUtc)
        {
            return false;
        }

        ApplyWindowHealthPenalty(pet);
        pet.FeedingWindowStartUtc = window.WindowStartUtc;
        pet.FeedingsUsedInWindow = 0;
        return true;
    }

    private static void ApplyWindowHealthPenalty(Pet pet)
    {
        if (pet.Hunger < 100 || pet.Health == 0)
        {
            return;
        }

        pet.Health--;
    }

    private static PetResponse ToResponse(Pet pet, DateTime now)
    {
        var window = GetFeedingWindow(now);
        var usedFeedings = pet.FeedingWindowStartUtc == window.WindowStartUtc
            ? pet.FeedingsUsedInWindow
            : 0;

        return new PetResponse
        {
            Id = pet.Id,
            Name = pet.Name,
            Hunger = pet.Hunger,
            Energy = pet.Energy,
            Happiness = pet.Happiness,
            Health = pet.Health,
            State = pet.State,
            LastUpdated = pet.LastUpdated,
            Mood = pet.Mood,
            RemainingFeedings = Math.Max(0, MaxFeedingsPerWindow - usedFeedings),
            FeedingsUsedInWindow = usedFeedings,
            FeedingWindowResetsAtUtc = window.NextResetUtc
        };
    }

    private static FeedingWindow GetFeedingWindow(DateTime nowUtc)
    {
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZoneInfo.Local);
        var currentDate = localNow.Date;
        var eightAm = currentDate.AddHours(8);
        var noon = currentDate.AddHours(12);
        var sixPm = currentDate.AddHours(18);

        DateTime windowStartLocal;
        DateTime nextResetLocal;

        if (localNow < eightAm)
        {
            windowStartLocal = currentDate.AddDays(-1).AddHours(18);
            nextResetLocal = eightAm;
        }
        else if (localNow < noon)
        {
            windowStartLocal = eightAm;
            nextResetLocal = noon;
        }
        else if (localNow < sixPm)
        {
            windowStartLocal = noon;
            nextResetLocal = sixPm;
        }
        else
        {
            windowStartLocal = sixPm;
            nextResetLocal = currentDate.AddDays(1).AddHours(8);
        }

        return new FeedingWindow(
            TimeZoneInfo.ConvertTimeToUtc(windowStartLocal, TimeZoneInfo.Local),
            TimeZoneInfo.ConvertTimeToUtc(nextResetLocal, TimeZoneInfo.Local));
    }

    private sealed record FeedingWindow(DateTime WindowStartUtc, DateTime NextResetUtc);
}
