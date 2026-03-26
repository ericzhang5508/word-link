using Microsoft.EntityFrameworkCore;
using WordLink.Data;
using WordLink.Models;
using System.Text.Json;

namespace WordLink.Services;

public class PuzzleService
{
    private readonly AppDbContext _db;

    public PuzzleService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets the daily puzzle. Cycles through available puzzles based on date.
    /// </summary>
    public async Task<Puzzle?> GetDailyPuzzleAsync()
    {
        var puzzleCount = await _db.Puzzles.CountAsync();
        if (puzzleCount == 0) return null;

        // Cycle through puzzles using day-of-year modulo
        var dayIndex = (DateTime.UtcNow.DayOfYear % puzzleCount) + 1;

        return await _db.Puzzles
            .Include(p => p.Clues)
            .Include(p => p.Stats)
            .FirstOrDefaultAsync(p => p.Id == dayIndex);
    }

    /// <summary>
    /// Checks user solution against stored answers.
    /// Returns (isCorrect, incorrectCells) where incorrectCells is a list of [row, col] pairs.
    /// </summary>
    public async Task<(bool IsCorrect, List<int[]> IncorrectCells)> CheckSolutionAsync(int puzzleId, string[][] userGrid)
    {
        var puzzle = await _db.Puzzles.FindAsync(puzzleId);
        if (puzzle == null)
            return (false, new List<int[]>());

        var solutionGrid = JsonSerializer.Deserialize<List<List<string>>>(puzzle.GridData);
        if (solutionGrid == null)
            return (false, new List<int[]>());

        var incorrectCells = new List<int[]>();

        for (int r = 0; r < puzzle.GridSize; r++)
        {
            for (int c = 0; c < puzzle.GridSize; c++)
            {
                var solution = solutionGrid[r][c];
                if (solution == "#") continue; // skip black cells

                var userAnswer = (r < userGrid.Length && c < userGrid[r].Length)
                    ? userGrid[r][c]?.ToUpperInvariant() ?? ""
                    : "";

                if (userAnswer != solution)
                {
                    incorrectCells.Add(new[] { r, c });
                }
            }
        }

        return (incorrectCells.Count == 0, incorrectCells);
    }

    /// <summary>
    /// Increments global solve count for a puzzle.
    /// </summary>
    public async Task<int> IncrementSolveCountAsync(int puzzleId)
    {
        var stats = await _db.GlobalStats.FirstOrDefaultAsync(s => s.PuzzleId == puzzleId);
        if (stats == null)
        {
            stats = new GlobalStats { PuzzleId = puzzleId, SolveCount = 1 };
            _db.GlobalStats.Add(stats);
        }
        else
        {
            stats.SolveCount++;
        }

        await _db.SaveChangesAsync();
        return stats.SolveCount;
    }

    public async Task<int> GetSolveCountAsync(int puzzleId)
    {
        var stats = await _db.GlobalStats.FirstOrDefaultAsync(s => s.PuzzleId == puzzleId);
        return stats?.SolveCount ?? 0;
    }
}
