namespace WordLink.Models;

/// <summary>
/// Represents the global statistics for a specific puzzle.
/// </summary>
public class GlobalStats
{
    /// <summary>Unique identifier for the generic stats entry.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key indicating which puzzle these stats belong to.</summary>
    public int PuzzleId { get; set; }

    /// <summary>The total number of times the puzzle has been successfully solved globally.</summary>
    public int SolveCount { get; set; }

    /// <summary>Navigation property to the associated puzzle.</summary>
    public Puzzle? Puzzle { get; set; }
}
