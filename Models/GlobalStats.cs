namespace WordLink.Models;

public class GlobalStats
{
    public int Id { get; set; }
    public int PuzzleId { get; set; }
    public int SolveCount { get; set; }

    public Puzzle? Puzzle { get; set; }
}
