public class PetResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Hunger { get; init; }

    public int Energy { get; init; }

    public int Happiness { get; init; }

    public string State { get; init; } = string.Empty;

    public DateTime LastUpdated { get; init; }

    public string Mood { get; init; } = string.Empty;

    public int RemainingFeedings { get; init; }

    public int FeedingsUsedInWindow { get; init; }

    public DateTime? FeedingWindowResetsAtUtc { get; init; }
}

public class FeedResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public PetResponse Pet { get; init; } = new();
}