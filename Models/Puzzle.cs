namespace WordLink.Models;

public class Puzzle
{
    public int Id { get; set; }
    public int GridSize { get; set; } = 15;

    /// <summary>
    /// JSON serialized 2D array of the grid.
    /// Each cell is either a letter (solution) or "#" for black cells.
    /// </summary>
    public string GridData { get; set; } = string.Empty;

    public DateTime ActiveDate { get; set; }
    public string Difficulty { get; set; } = "EXPERT";

    public ICollection<Clue> Clues { get; set; } = new List<Clue>();
    public GlobalStats? Stats { get; set; }
}
