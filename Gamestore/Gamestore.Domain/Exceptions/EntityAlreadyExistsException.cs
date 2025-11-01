namespace Gamestore.Domain.Exceptions;

public class EntityAlreadyExistsException(string entityName, string field, object value)
    : Exception($"{entityName} with {field} '{value}' already exists.")
{
}
