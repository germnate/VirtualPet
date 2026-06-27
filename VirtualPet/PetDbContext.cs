using Microsoft.EntityFrameworkCore;

public class PetDbContext : DbContext
{
    public DbSet<Pet> Pets => Set<Pet>();

    public PetDbContext(DbContextOptions<PetDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pet>().ToTable("Pets");

        modelBuilder.Entity<Pet>().HasData(
            new Pet
            {
                Id = 1,
                Name = "Foxy",
                Hunger = 40,
                Energy = 80,
                Happiness = 65,
                FeedingsUsedInWindow = 0,
                FeedingWindowStartUtc = null,
                State = "idle",
                LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
