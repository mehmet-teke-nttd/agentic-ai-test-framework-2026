using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IElementRegistryService
{
    ElementRegistryEntry GetElement(string target);
}
