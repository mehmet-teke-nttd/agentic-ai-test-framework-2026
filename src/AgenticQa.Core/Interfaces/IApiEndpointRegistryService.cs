using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IApiEndpointRegistryService
{
    ApiEndpointEntry GetEndpoint(string name);
    IReadOnlyList<ApiEndpointEntry> GetAll();
}
