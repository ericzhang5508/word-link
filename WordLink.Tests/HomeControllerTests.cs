using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WordLink.Controllers;
using WordLink.Data;
using WordLink.Hubs;
using WordLink.Services;
using WordLink.ViewModels;
using Xunit;

namespace WordLink.Tests;

public class HomeControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
            
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private HomeController CreateController(AppDbContext context, out Mock<IHubContext<PuzzleHub>> mockHubContext, out Mock<IClientProxy> mockClientProxy)
    {
        var puzzleService = new PuzzleService(context);
        
        mockClientProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        mockHubContext = new Mock<IHubContext<PuzzleHub>>();
        mockHubContext.Setup(hc => hc.Clients).Returns(mockClients.Object);
        
        return new HomeController(puzzleService, mockHubContext.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithPuzzleViewModel()
    {
        // Arrange
        using var context = GetDbContext();
        var controller = CreateController(context, out var _, out var _);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PuzzleViewModel>(viewResult.Model);
        Assert.NotNull(model.Puzzle);
        Assert.Equal(5, model.AcrossClues.Count);
        Assert.Equal(5, model.DownClues.Count);
        Assert.Equal(0, model.GlobalSolveCount);
        Assert.False(string.IsNullOrEmpty(model.FormattedDate));
    }

    [Fact]
    public async Task CheckSolve_ReturnsSuccessTrue_WhenSolutionIsCorrect()
    {
        // Arrange
        using var context = GetDbContext();
        var controller = CreateController(context, out var mockHubContext, out var mockClientProxy);

        var request = new CheckSolveRequest
        {
            PuzzleId = 1,
            UserGrid = new string[][]
            {
                new string[] { "C", "R", "A", "N", "E" },
                new string[] { "R", "E", "L", "A", "Y" },
                new string[] { "A", "D", "A", "P", "T" },
                new string[] { "N", "I", "N", "E", "S" },
                new string[] { "E", "S", "S", "A", "Y" }
            }
        };

        // Act
        var result = await controller.CheckSolve(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        dynamic data = jsonResult.Value!;
        
        // Assert json value has success = true
        var successProperty = data.GetType().GetProperty("success");
        Assert.NotNull(successProperty);
        Assert.True((bool)successProperty.GetValue(data, null));
        
        // Assert solveCount is returned and equals 1 (because it defaults to 0 and increments)
        var solveCountProperty = data.GetType().GetProperty("solveCount");
        Assert.NotNull(solveCountProperty);
        Assert.Equal(1, (int)solveCountProperty.GetValue(data, null));

        // Verify that SignalR broadcast was called
        mockClientProxy.Verify(
            clientProxy => clientProxy.SendCoreAsync(
                "UpdateSolveCount",
                It.Is<object[]>(o => o != null && o.Length == 1 && (int)o[0] == 1),
                default(CancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task CheckSolve_ReturnsSuccessFalse_WhenSolutionIsIncorrect()
    {
        // Arrange
        using var context = GetDbContext();
        var controller = CreateController(context, out var mockHubContext, out var mockClientProxy);

        var request = new CheckSolveRequest
        {
            PuzzleId = 1,
            UserGrid = new string[][]
            {
                new string[] { "C", "R", "A", "N", "E" },
                new string[] { "R", "E", "L", "A", "Y" },
                new string[] { "A", "B", "A", "P", "T" }, // Incorrect! 'B' instead of 'D'
                new string[] { "N", "I", "N", "E", "S" },
                new string[] { "E", "S", "S", "A", "Y" }
            }
        };

        // Act
        var result = await controller.CheckSolve(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        dynamic data = jsonResult.Value!;
        
        // Assert json value has success = false
        var successProperty = data.GetType().GetProperty("success");
        Assert.NotNull(successProperty);
        Assert.False((bool)successProperty.GetValue(data, null));
        
        // Assert incorrectCells array
        var incorrectCellsProperty = data.GetType().GetProperty("incorrectCells");
        Assert.NotNull(incorrectCellsProperty);
        var incorrectCells = (List<int[]>)incorrectCellsProperty.GetValue(data, null);
        Assert.Single(incorrectCells);
        Assert.Equal(2, incorrectCells[0][0]); // Row 2
        Assert.Equal(1, incorrectCells[0][1]); // Col 1

        // Verify that SignalR broadcast was NOT called
        mockClientProxy.Verify(
            clientProxy => clientProxy.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetStats_ReturnsCorrectSolveCount()
    {
        // Arrange
        using var context = GetDbContext();
        var controller = CreateController(context, out var _, out var _);
        
        // Pre-increment count to ensure it works
        var puzzleService = new PuzzleService(context);
        await puzzleService.IncrementSolveCountAsync(1);
        await puzzleService.IncrementSolveCountAsync(1); // Call twice, count should be 2

        // Act
        var result = await controller.GetStats(1);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        dynamic data = jsonResult.Value!;
        
        var solveCountProperty = data.GetType().GetProperty("solveCount");
        Assert.NotNull(solveCountProperty);
        Assert.Equal(2, (int)solveCountProperty.GetValue(data, null));
    }
}
