public class Pet
{
    public int Id { get; set; }

    public string Name { get; set; } = "Foxy";

    public int Hunger { get; set; } = 40;
    public int Energy { get; set; } = 80;
    public int Happiness { get; set; } = 65;

    public string State { get; set; } = "idle";

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
