using AgenticQa.Core.Models;

namespace AgenticQa.Core.ElementRegistry;

public sealed class ElementRegistryConfigurationException : Exception
{
    public ElementRegistryConfigurationException(
        string message,
        IReadOnlyList<ElementRegistryValidationError> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyList<ElementRegistryValidationError> Errors { get; }
}
