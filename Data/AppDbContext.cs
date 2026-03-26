using Microsoft.EntityFrameworkCore;
using WordLink.Models;
using System.Text.Json;

namespace WordLink.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Puzzle> Puzzles => Set<Puzzle>();
    public DbSet<Clue> Clues => Set<Clue>();
    public DbSet<GlobalStats> GlobalStats => Set<GlobalStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Puzzle>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasMany(p => p.Clues).WithOne(c => c.Puzzle).HasForeignKey(c => c.PuzzleId);
            e.HasOne(p => p.Stats).WithOne(s => s.Puzzle).HasForeignKey<GlobalStats>(s => s.PuzzleId);
        });

        modelBuilder.Entity<Clue>(e =>
        {
            e.HasKey(c => c.Id);
        });

        modelBuilder.Entity<GlobalStats>(e =>
        {
            e.HasKey(s => s.Id);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // 15x15 grid: letters for cells, "#" for black cells
        // This puzzle is inspired by the design mockup
        var grid = new string[15, 15]
        {
            // Row 0:  S  I  L  E  N  #  K  A  I  Z  E  #  A  R  K
            { "S","I","L","E","N","#","K","A","I","Z","E","#","A","R","K" },
            // Row 1:  T  N  I  G  M  A  #  B  A  S  H  #  W  A  Y
            { "T","N","I","G","M","A","#","B","A","S","H","#","W","A","Y" },
            // Row 2:  A  U  T  H  O  R  I  T  Y  #  O  S  L  O  W
            { "A","U","T","H","O","R","I","T","Y","#","O","S","L","O","W" },
            // Row 3:  #  #  E  #  N  #  T  #  #  #  L  #  U  #  #  
            { "#","#","E","#","N","#","T","#","#","#","L","#","U","#","#" },
            // Row 4:  C  U  B  E  #  G  E  A  R  #  D  #  T  E  A
            { "C","U","B","E","#","G","E","A","R","#","D","#","T","E","A" },
            // Row 5:  #  #  #  L  A  R  G  E  #  W  #  P  E  #  #
            { "#","#","#","L","A","R","G","E","#","W","#","P","E","#","#" },
            // Row 6:  D  R  A  I  N  #  #  #  S  I  N  E  #  H  O
            { "D","R","A","I","N","#","#","#","S","I","N","E","#","H","O" },
            // Row 7:  E  #  N  #  #  P  R  I  M  E  #  A  K  #  R
            { "E","#","N","#","#","P","R","I","M","E","#","A","K","#","R" },
            // Row 8:  #  #  #  V  A  L  E  T  #  #  C  R  E  A  M
            { "#","#","#","V","A","L","E","T","#","#","C","R","E","A","M" },
            // Row 9:  S  P  A  N  #  #  #  #  B  R  A  V  E  #  #
            { "S","P","A","N","#","#","#","#","B","R","A","V","E","#","#" },
            // Row 10: T  #  #  #  C  O  R  A  L  #  N  #  #  #  #
            { "T","#","#","#","C","O","R","A","L","#","N","#","#","#","#" },
            // Row 11: A  G  E  #  H  A  Z  E  #  F  E  W  #  #  #
            { "A","G","E","#","H","A","Z","E","#","F","E","W","#","#","#" },
            // Row 12: R  #  #  O  I  L  #  D  G  E  #  #  #  #  #
            { "R","#","#","O","I","L","#","D","G","E","#","#","#","#","#" },
            // Row 13: E  D  G  E  #  L  O  O  P  #  #  #  #  #  #
            { "E","D","G","E","#","L","O","O","P","#","#","#","#","#","#" },
            // Row 14: #  #  #  #  #  #  #  #  #  #  #  #  #  #  #
            { "#","#","#","#","#","#","#","#","#","#","#","#","#","#","#" },
        };

        // Flatten to JSON
        var gridList = new List<List<string>>();
        for (int r = 0; r < 15; r++)
        {
            var row = new List<string>();
            for (int c = 0; c < 15; c++)
                row.Add(grid[r, c]);
            gridList.Add(row);
        }
        var gridJson = JsonSerializer.Serialize(gridList);

        modelBuilder.Entity<Puzzle>().HasData(new Puzzle
        {
            Id = 1,
            GridSize = 15,
            GridData = gridJson,
            ActiveDate = new DateTime(2024, 3, 21),
            Difficulty = "EXPERT"
        });

        modelBuilder.Entity<GlobalStats>().HasData(new GlobalStats
        {
            Id = 1,
            PuzzleId = 1,
            SolveCount = 12402
        });

        // Across clues
        var clues = new List<Clue>
        {
            new() { Id = 1,  PuzzleId = 1, Number = 1,  Direction = "Across", Text = "Quiet and motionless; making little or no noise.", Answer = "SILEN", StartRow = 0, StartCol = 0 },
            new() { Id = 2,  PuzzleId = 1, Number = 6,  Direction = "Across", Text = "A continuous transformation or improvement.", Answer = "KAIZE", StartRow = 0, StartCol = 6 },
            new() { Id = 3,  PuzzleId = 1, Number = 14, Direction = "Across", Text = "Unsolved mystery; a riddle or puzzle that is difficult to understand.", Answer = "TNIGMA", StartRow = 1, StartCol = 0 },
            new() { Id = 4,  PuzzleId = 1, Number = 16, Direction = "Across", Text = "A celebratory social gathering.", Answer = "BASH", StartRow = 1, StartCol = 7 },
            new() { Id = 5,  PuzzleId = 1, Number = 17, Direction = "Across", Text = "A path or method of travel.", Answer = "WAY", StartRow = 1, StartCol = 12 },
            new() { Id = 6,  PuzzleId = 1, Number = 18, Direction = "Across", Text = "Commanding presence or specialized knowledge.", Answer = "AUTHORITY", StartRow = 2, StartCol = 0 },
            new() { Id = 7,  PuzzleId = 1, Number = 20, Direction = "Across", Text = "Capital city of Norway.", Answer = "OSLO", StartRow = 2, StartCol = 10 },
            new() { Id = 8,  PuzzleId = 1, Number = 23, Direction = "Across", Text = "A geometric solid with six square faces.", Answer = "CUBE", StartRow = 4, StartCol = 0 },
            new() { Id = 9,  PuzzleId = 1, Number = 25, Direction = "Across", Text = "Equipment or mechanism for a task.", Answer = "GEAR", StartRow = 4, StartCol = 5 },
            new() { Id = 10, PuzzleId = 1, Number = 28, Direction = "Across", Text = "Of considerable size or extent.", Answer = "LARGE", StartRow = 5, StartCol = 3 },
            new() { Id = 11, PuzzleId = 1, Number = 30, Direction = "Across", Text = "To draw off liquid gradually.", Answer = "DRAIN", StartRow = 6, StartCol = 0 },
            new() { Id = 12, PuzzleId = 1, Number = 33, Direction = "Across", Text = "A trigonometric function.", Answer = "SINE", StartRow = 6, StartCol = 8 },
            new() { Id = 13, PuzzleId = 1, Number = 35, Direction = "Across", Text = "A number divisible only by 1 and itself.", Answer = "PRIME", StartRow = 7, StartCol = 5 },
            new() { Id = 14, PuzzleId = 1, Number = 40, Direction = "Across", Text = "A personal attendant.", Answer = "VALET", StartRow = 8, StartCol = 3 },
            new() { Id = 15, PuzzleId = 1, Number = 41, Direction = "Across", Text = "A smooth rich dairy product.", Answer = "CREAM", StartRow = 8, StartCol = 10 },
            new() { Id = 16, PuzzleId = 1, Number = 44, Direction = "Across", Text = "The extent of a bridge.", Answer = "SPAN", StartRow = 9, StartCol = 0 },
            new() { Id = 17, PuzzleId = 1, Number = 47, Direction = "Across", Text = "Showing courage and fearlessness.", Answer = "BRAVE", StartRow = 9, StartCol = 8 },
            new() { Id = 18, PuzzleId = 1, Number = 49, Direction = "Across", Text = "Marine organism that forms reefs.", Answer = "CORAL", StartRow = 10, StartCol = 4 },
            new() { Id = 19, PuzzleId = 1, Number = 53, Direction = "Across", Text = "A period of time in history.", Answer = "AGE", StartRow = 11, StartCol = 0 },
            new() { Id = 20, PuzzleId = 1, Number = 54, Direction = "Across", Text = "A blurry atmospheric phenomenon.", Answer = "HAZE", StartRow = 11, StartCol = 4 },
            new() { Id = 21, PuzzleId = 1, Number = 58, Direction = "Across", Text = "Not many.", Answer = "FEW", StartRow = 11, StartCol = 9 },
            new() { Id = 22, PuzzleId = 1, Number = 59, Direction = "Across", Text = "A viscous liquid fuel.", Answer = "OIL", StartRow = 12, StartCol = 3 },
            new() { Id = 23, PuzzleId = 1, Number = 61, Direction = "Across", Text = "The outer boundary of something.", Answer = "EDGE", StartRow = 13, StartCol = 0 },
            new() { Id = 24, PuzzleId = 1, Number = 64, Direction = "Across", Text = "A shape that curves back on itself.", Answer = "LOOP", StartRow = 13, StartCol = 5 },
        };

        // Down clues
        clues.AddRange(new[]
        {
            new Clue { Id = 25, PuzzleId = 1, Number = 1,  Direction = "Down", Text = "A specific point or level on a scale.", Answer = "STARE", StartRow = 0, StartCol = 0 },
            new Clue { Id = 26, PuzzleId = 1, Number = 2,  Direction = "Down", Text = "Popular Japanese noodle dish.", Answer = "INUPAGE", StartRow = 0, StartCol = 1 },
            new Clue { Id = 27, PuzzleId = 1, Number = 3,  Direction = "Down", Text = "Units of musical rhythm.", Answer = "LITERATE", StartRow = 0, StartCol = 2 },
            new Clue { Id = 28, PuzzleId = 1, Number = 4,  Direction = "Down", Text = "Ancient currency of Greece.", Answer = "EHLIVEN", StartRow = 0, StartCol = 3 },
            new Clue { Id = 29, PuzzleId = 1, Number = 5,  Direction = "Down", Text = "A brief, forceful summary.", Answer = "NMONACO", StartRow = 0, StartCol = 4 },
            new Clue { Id = 30, PuzzleId = 1, Number = 11, Direction = "Down", Text = "A curved structure spanning an opening.", Answer = "AROLDCANE", StartRow = 0, StartCol = 12 },
            new Clue { Id = 31, PuzzleId = 1, Number = 19, Direction = "Down", Text = "A small and determined creature.", Answer = "ITEG", StartRow = 2, StartCol = 6 },
            new Clue { Id = 32, PuzzleId = 1, Number = 21, Direction = "Down", Text = "The season of falling leaves.", Answer = "SLDR", StartRow = 2, StartCol = 11 },
        });

        modelBuilder.Entity<Clue>().HasData(clues);
    }
}
