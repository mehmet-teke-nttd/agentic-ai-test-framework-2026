using System.Text.Json;
using System.Text.RegularExpressions;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Execution.Runtime;

public sealed partial class TestDataResolver : ITestDataResolver
{
    public TestDataResolutionResult Resolve(string value, TestIntent testIntent)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(testIntent);
        ArgumentNullException.ThrowIfNull(testIntent.TestData);

        var match = FullPlaceholderRegex().Match(value);
        if (!match.Success)
        {
            return new TestDataResolutionResult
            {
                IsSuccess = true,
                ResolvedValue = value
            };
        }

        var variableName = match.Groups["name"].Value;
        if (!testIntent.TestData.TryGetValue(variableName, out var rawValue))
        {
            return new TestDataResolutionResult
            {
                IsSuccess = false,
                ErrorCode = RuntimeExecutionErrorCode.TestDataReferenceNotFound,
                ErrorMessage = $"Test data variable '{variableName}' is not defined."
            };
        }

        return new TestDataResolutionResult
        {
            IsSuccess = true,
            ResolvedValue = ConvertToString(rawValue)
        };
    }

    private static string ConvertToString(object? value) =>
        value switch
        {
            null => string.Empty,
            JsonElement element => element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => element.ToString()
            },
            _ => value.ToString() ?? string.Empty
        };

    [GeneratedRegex(@"^\{\{\s*(?<name>[a-zA-Z0-9_]+)\s*\}\}$")]
    private static partial Regex FullPlaceholderRegex();
}
