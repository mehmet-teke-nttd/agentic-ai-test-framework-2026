using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IElementRegistryValidator
{
    ElementRegistryValidationResult Validate(Models.ElementRegistry registry);
}
