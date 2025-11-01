using Gamestore.Domain.Entities;

namespace GameStore.UnitTests.Domain;

public class GameTests
{
    [Theory]
    [InlineData("Halo", "halo")]
    [InlineData("The Witcher 3: Wild Hunt", "the-witcher-3-wild-hunt")]
    [InlineData("Call of Duty: Black Ops", "call-of-duty-black-ops")]
    [InlineData("Super@Mario!Brothers_01", "supermariobrothers01")] // Invalid characters removed, only hyphens kept if any
    [InlineData("  Space     Invaders  ", "space-invaders")]
    [InlineData("Half-Life---2", "half-life-2")]
    public void GenerateKeyFromNameShouldReturnExpectedKey(string inputName, string expectedKey)
    {
        // Arrange & Act
        var result = Game.GenerateKeyFromName(inputName);

        // Assert
        Assert.Equal(expectedKey, result);
    }
}
