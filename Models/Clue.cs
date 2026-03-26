namespace WordLink.Models;

public class Clue
{
    public int Id { get; set; }
    public int PuzzleId { get; set; }
    public int Number { get; set; }
    public string Direction { get; set; } = "Across"; // "Across" or "Down"
    public string Text { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int StartRow { get; set; }
    public int StartCol { get; set; }

    public Puzzle? Puzzle { get; set; }
}
