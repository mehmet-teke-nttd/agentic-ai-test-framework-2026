using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IComponentRegistryLoader
{
    ComponentRegistry Load(string filePath);
}
