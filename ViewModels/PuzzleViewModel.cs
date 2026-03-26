namespace WordLink.ViewModels;

using WordLink.Models;

public class PuzzleViewModel
{
    public Puzzle Puzzle { get; set; } = null!;
    public List<Clue> AcrossClues { get; set; } = new();
    public List<Clue> DownClues { get; set; } = new();
    public int GlobalSolveCount { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
}
