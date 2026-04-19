using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WordLink.Hubs;
using WordLink.Services;
using WordLink.ViewModels;
using System.Globalization;

namespace WordLink.Controllers;

/// <summary>
/// Controller responsible for handling the main puzzle view and related API endpoints.
/// </summary>
public class HomeController : Controller
{
    private readonly PuzzleService _puzzleService;
    private readonly IHubContext<PuzzleHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="puzzleService">Service for interacting with puzzles.</param>
    /// <param name="hubContext">SignalR hub context for real-time updates.</param>
    public HomeController(PuzzleService puzzleService, IHubContext<PuzzleHub> hubContext)
    {
        _puzzleService = puzzleService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Renders the main index view with the daily puzzle data.
    /// </summary>
    /// <returns>An IActionResult returning the index view with PuzzleViewModel.</returns>
    public async Task<IActionResult> Index()
    {
        var puzzle = await _puzzleService.GetDailyPuzzleAsync();
        if (puzzle == null)
            return View("Error");

        var solveCount = await _puzzleService.GetSolveCountAsync(puzzle.Id);

        var vm = new PuzzleViewModel
        {
            Puzzle = puzzle,
            AcrossClues = puzzle.Clues.Where(c => c.Direction == "Across").OrderBy(c => c.Number).ToList(),
            DownClues = puzzle.Clues.Where(c => c.Direction == "Down").OrderBy(c => c.Number).ToList(),
            GlobalSolveCount = solveCount,
            FormattedDate = DateTime.UtcNow.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant()
        };

        return View(vm);
    }

    /// <summary>
    /// Validates the user's puzzle solution submitted via an API call.
    /// Broadcasts an update via SignalR if the solution is completely correct.
    /// </summary>
    /// <param name="request">The payload containing puzzle ID and current user grid.</param>
    /// <returns>A JSON response indicating success or the coordinates of incorrect cells.</returns>
    [HttpPost]
    public async Task<IActionResult> CheckSolve([FromBody] CheckSolveRequest request)
    {
        var (isCorrect, incorrectCells) = await _puzzleService.CheckSolutionAsync(request.PuzzleId, request.UserGrid);

        if (isCorrect)
        {
            var newCount = await _puzzleService.IncrementSolveCountAsync(request.PuzzleId);
            await _hubContext.Clients.All.SendAsync("UpdateSolveCount", newCount);
            return Json(new { success = true, solveCount = newCount });
        }

        return Json(new { success = false, incorrectCells });
    }

    /// <summary>
    /// Retrieves the current solve count for a specific puzzle via an API call.
    /// </summary>
    /// <param name="puzzleId">The unique ID of the puzzle.</param>
    /// <returns>A JSON response with the current solve count.</returns>
    [HttpGet]
    public async Task<IActionResult> GetStats(int puzzleId)
    {
        var count = await _puzzleService.GetSolveCountAsync(puzzleId);
        return Json(new { solveCount = count });
    }
}

/// <summary>
/// Data transfer object used for validating user puzzle solutions.
/// </summary>
public class CheckSolveRequest
{
    public int PuzzleId { get; set; }
    public string[][] UserGrid { get; set; } = Array.Empty<string[]>();
}
