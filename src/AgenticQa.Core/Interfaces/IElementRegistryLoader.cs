using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IElementRegistryLoader
{
    Models.ElementRegistry Load(string filePath);
}
