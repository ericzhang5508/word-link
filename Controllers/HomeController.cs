using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WordLink.Hubs;
using WordLink.Services;
using WordLink.ViewModels;
using System.Globalization;

namespace WordLink.Controllers;

public class HomeController : Controller
{
    private readonly PuzzleService _puzzleService;
    private readonly IHubContext<PuzzleHub> _hubContext;

    public HomeController(PuzzleService puzzleService, IHubContext<PuzzleHub> hubContext)
    {
        _puzzleService = puzzleService;
        _hubContext = hubContext;
    }

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

    [HttpGet]
    public async Task<IActionResult> GetStats(int puzzleId)
    {
        var count = await _puzzleService.GetSolveCountAsync(puzzleId);
        return Json(new { solveCount = count });
    }
}

public class CheckSolveRequest
{
    public int PuzzleId { get; set; }
    public string[][] UserGrid { get; set; } = Array.Empty<string[]>();
}
