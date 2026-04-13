using Microsoft.EntityFrameworkCore;
using WordLink.Data;
using WordLink.Models;
using WordLink.Services;
using Xunit;
using System;
using System.Threading.Tasks;

namespace WordLink.Tests;

public class PuzzleServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CheckSolutionAsync_ValidatesCorrectSolution()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new PuzzleService(context);

        var correctGrid = new string[][]
        {
            new string[] { "C", "R", "A", "N", "E" },
            new string[] { "R", "E", "L", "A", "Y" },
            new string[] { "A", "D", "A", "P", "T" },
            new string[] { "N", "I", "N", "E", "S" },
            new string[] { "E", "S", "S", "A", "Y" }
        };

        // Act
        var result = await service.CheckSolutionAsync(1, correctGrid);

        // Assert
        Assert.True(result.IsCorrect);
        Assert.Empty(result.IncorrectCells);
    }
    
    [Fact]
    public async Task CheckSolutionAsync_ValidatesIncorrectSolution()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new PuzzleService(context);

        var incorrectGrid = new string[][]
        {
            new string[] { "C", "R", "A", "N", "E" },
            new string[] { "R", "E", "L", "A", "Y" },
            new string[] { "A", "B", "A", "P", "T" }, // 'B' instead of 'D' row 2 col 1
            new string[] { "N", "I", "N", "E", "S" },
            new string[] { "E", "S", "S", "A", "X" }  // 'X' instead of 'Y' row 4 col 4
        };

        // Act
        var result = await service.CheckSolutionAsync(1, incorrectGrid);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(2, result.IncorrectCells.Count);
        
        // Assert first wrong cell
        Assert.Equal(2, result.IncorrectCells[0][0]);
        Assert.Equal(1, result.IncorrectCells[0][1]);
        
        // Assert second wrong cell
        Assert.Equal(4, result.IncorrectCells[1][0]);
        Assert.Equal(4, result.IncorrectCells[1][1]);
    }
    
    [Fact]
    public async Task IncrementSolveCountAsync_IncrementsStats()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new PuzzleService(context);

        // Act
        var initialCount = await service.GetSolveCountAsync(1);
        var newCount = await service.IncrementSolveCountAsync(1);

        // Assert
        Assert.Equal(0, initialCount); // Should be 0 initially due to seed data
        Assert.Equal(1, newCount);     // Should be incremented by 1
    }
}
