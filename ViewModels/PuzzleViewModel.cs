namespace WordLink.ViewModels;

using WordLink.Models;

/// <summary>
/// View model containing all necessary data to render the daily puzzle page.
/// </summary>
public class PuzzleViewModel
{
    /// <summary>The active puzzle entity.</summary>
    public Puzzle Puzzle { get; set; } = null!;

    /// <summary>List of clues with an "Across" direction, ordered by clue number.</summary>
    public List<Clue> AcrossClues { get; set; } = new();

    /// <summary>List of clues with a "Down" direction, ordered by clue number.</summary>
    public List<Clue> DownClues { get; set; } = new();

    /// <summary>The total number of global solves for this puzzle.</summary>
    public int GlobalSolveCount { get; set; }

    /// <summary>The pre-formatted active date for display (e.g., MONDAY, JANUARY 1, 2024).</summary>
    public string FormattedDate { get; set; } = string.Empty;
}
