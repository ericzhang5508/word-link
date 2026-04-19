using Microsoft.AspNetCore.SignalR;

namespace WordLink.Hubs;

/// <summary>
/// SignalR Hub for handling real-time puzzle interactions and updates.
/// </summary>
public class PuzzleHub : Hub
{
    /// <summary>
    /// Broadcasts updated solve count to all connected clients.
    /// </summary>
    public async Task BroadcastSolveCount(int solveCount)
    {
        await Clients.All.SendAsync("UpdateSolveCount", solveCount);
    }
}
