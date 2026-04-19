namespace WordLink.Models;

/// <summary>
/// Represents a crossword puzzle entity.
/// </summary>
public class Puzzle
{
    /// <summary>Unique identifier for the puzzle.</summary>
    public int Id { get; set; }

    /// <summary>The size of the puzzle grid (e.g., 5 for a 5x5 grid).</summary>
    public int GridSize { get; set; } = 5;

    /// <summary>
    /// JSON serialized 2D array of the grid.
    /// Each cell is either a letter (solution) or "#" for black cells.
    /// </summary>
    public string GridData { get; set; } = string.Empty;

    /// <summary>The date this puzzle becomes active or was created for.</summary>
    public DateTime ActiveDate { get; set; }

    /// <summary>Determines the difficulty level of the puzzle (e.g., "EXPERT").</summary>
    public string Difficulty { get; set; } = "EXPERT";

    /// <summary>A collection of clues associated with this puzzle.</summary>
    public ICollection<Clue> Clues { get; set; } = new List<Clue>();

    /// <summary>Global statistics associated with this puzzle.</summary>
    public GlobalStats? Stats { get; set; }
}
