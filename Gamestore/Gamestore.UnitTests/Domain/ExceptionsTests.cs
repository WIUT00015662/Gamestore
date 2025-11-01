using Gamestore.Domain.Exceptions;

namespace GameStore.UnitTests.Domain;

public class ExceptionsTests
{
    [Fact]
    public void EntityNotFoundExceptionShouldFormatMessageCorrectly()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("Game", "halo-key");

        // Assert
        Assert.Equal("Game with key 'halo-key' was not found.", exception.Message);
    }

    [Fact]
    public void EntityNotFoundExceptionShouldFormatMessageCorrectlyWithGuid()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var exception = new EntityNotFoundException("Genre", id);

        // Assert
        Assert.Equal($"Genre with key '{id}' was not found.", exception.Message);
    }

    [Fact]
    public void EntityAlreadyExistsExceptionShouldFormatMessageCorrectly()
    {
        // Arrange & Act
        var exception = new EntityAlreadyExistsException("Game", "Key", "existing-key");

        // Assert
        Assert.Equal("Game with Key 'existing-key' already exists.", exception.Message);
    }
}
