using Microsoft.EntityFrameworkCore;
using WordLink.Models;
using System.Text.Json;

namespace WordLink.Data;

/// <summary>
/// The main database context for the WordLink application.
/// Manages the data access and ORM mapping for puzzles, clues, and statistics.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Database set representing crossword puzzles.</summary>
    public DbSet<Puzzle> Puzzles => Set<Puzzle>();

    /// <summary>Database set representing clues associated with puzzles.</summary>
    public DbSet<Clue> Clues => Set<Clue>();

    /// <summary>Database set representing global statistics for puzzles.</summary>
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
        // 5x5 grid: letters for cells, "#" for black cells
        var grid = new string[5, 5]
        {
            { "C", "R", "A", "N", "E" },
            { "R", "E", "L", "A", "Y" },
            { "A", "D", "A", "P", "T" },
            { "N", "I", "N", "E", "S" },
            { "E", "S", "S", "A", "Y" }
        };

        // Flatten to JSON
        var gridList = new List<List<string>>();
        for (int r = 0; r < 5; r++)
        {
            var row = new List<string>();
            for (int c = 0; c < 5; c++)
                row.Add(grid[r, c]);
            gridList.Add(row);
        }
        var gridJson = JsonSerializer.Serialize(gridList);

        modelBuilder.Entity<Puzzle>().HasData(new Puzzle
        {
            Id = 1,
            GridSize = 5,
            GridData = gridJson,
            ActiveDate = new DateTime(2024, 3, 21),
            Difficulty = "HARD"
        });

        modelBuilder.Entity<GlobalStats>().HasData(new GlobalStats
        {
            Id = 1,
            PuzzleId = 1,
            SolveCount = 0
        });

        var clues = new List<Clue>
        {
            // Across Clues
            new() { Id = 1, PuzzleId = 1, Number = 1, Direction = "Across", Text = "Large bird of the marsh.", Answer = "CRANE", StartRow = 0, StartCol = 0 },
            new() { Id = 2, PuzzleId = 1, Number = 6, Direction = "Across", Text = "Race where a baton is passed.", Answer = "RELAY", StartRow = 1, StartCol = 0 },
            new() { Id = 3, PuzzleId = 1, Number = 7, Direction = "Across", Text = "To adjust to new conditions.", Answer = "ADAPT", StartRow = 2, StartCol = 0 },
            new() { Id = 4, PuzzleId = 1, Number = 8, Direction = "Across", Text = "To the 'nines' is to perfection.", Answer = "NINES", StartRow = 3, StartCol = 0 },
            new() { Id = 5, PuzzleId = 1, Number = 9, Direction = "Across", Text = "A short piece of writing.", Answer = "ESSAY", StartRow = 4, StartCol = 0 },
        
            // Down Clues
            new() { Id = 6, PuzzleId = 1, Number = 1, Direction = "Down", Text = "Crane or egret.", Answer = "CRANE", StartRow = 0, StartCol = 0 },
            new() { Id = 7, PuzzleId = 1, Number = 2, Direction = "Down", Text = "Remote Dictionary Server.", Answer = "REDIS", StartRow = 0, StartCol = 1 },
            new() { Id = 8, PuzzleId = 1, Number = 3, Direction = "Down", Text = "A ancient, nomadic people.", Answer = "ALANS", StartRow = 0, StartCol = 2 }, 
            new() { Id = 9, PuzzleId = 1, Number = 4, Direction = "Down", Text = "A nymph of valleys, glens, and wooded areas.", Answer = "NAPEA", StartRow = 0, StartCol = 3 },
            new() { Id = 10, PuzzleId = 1, Number = 5, Direction = "Down", Text = "EYTSY (Not a Word)", Answer = "EYTSY", StartRow = 0, StartCol = 4 }
        };

        modelBuilder.Entity<Clue>().HasData(clues);
    }
}
