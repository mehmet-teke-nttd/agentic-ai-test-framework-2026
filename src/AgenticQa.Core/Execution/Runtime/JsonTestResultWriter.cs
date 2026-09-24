using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class JsonTestResultWriter : ITestResultWriter
{
    private readonly string _outputDirectory;
    private readonly ILogger<JsonTestResultWriter> _logger;

    public JsonTestResultWriter(string outputDirectory, ILogger<JsonTestResultWriter> logger)
    {
        _outputDirectory = outputDirectory;
        _logger = logger;
    }

    public async Task<string> WriteAsync(TestExecutionResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(result.TestId);

        Directory.CreateDirectory(_outputDirectory);
        var filePath = Path.Combine(_outputDirectory, $"{result.TestId}-result.json");
        var payload = AgenticJsonSerializer.Serialize(result);

        await File.WriteAllTextAsync(filePath, payload, cancellationToken);
        _logger.LogInformation("Test result written to {Path}", filePath);
        return filePath;
    }
}
