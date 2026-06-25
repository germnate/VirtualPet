public class Pet
{
    public int Id { get; set; }

    public string Name { get; set; } = "Foxy";

    public int Hunger { get; set; } = 40;
    public int Energy { get; set; } = 80;
    public int Happiness { get; set; } = 65;

    public string State { get; set; } = "idle";

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public string Mood => CalculateMood();

    private string CalculateMood()
    {
        if (Hunger > 90) return "miserable";
        if (Hunger > 75) return "hungry";

        if (Energy < 10) return "exhausted";
        if (Energy < 25) return "tired";

        if (Happiness < 20) return "sad";
        if (Happiness < 50) return "bored";

        if (Happiness > 80 && Energy > 60) return "excited";

        return "content";
    }

}
