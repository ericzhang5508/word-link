namespace WordLink.Models;

/// <summary>
/// Represents a single crossword clue, including its position and answer.
/// </summary>
public class Clue
{
    /// <summary>Unique identifier for the clue.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the associated puzzle.</summary>
    public int PuzzleId { get; set; }

    /// <summary>The display number of the clue on the puzzle grid.</summary>
    public int Number { get; set; }

    /// <summary>The direction of the clue on the grid (e.g., "Across" or "Down").</summary>
    public string Direction { get; set; } = "Across"; // "Across" or "Down"

    /// <summary>The descriptive text of the clue.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>The expected word answer for this clue.</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>The zero-based row index where the answer starts on the grid.</summary>
    public int StartRow { get; set; }

    /// <summary>The zero-based column index where the answer starts on the grid.</summary>
    public int StartCol { get; set; }

    /// <summary>Navigation property to the parent puzzle.</summary>
    public Puzzle? Puzzle { get; set; }
}
