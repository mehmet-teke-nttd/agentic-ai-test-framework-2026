using System.Text.Json;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugDraftResponseValidator : IBugDraftResponseValidator
{
    public bool TryParse(string aiResponse, out BugDraftPolishResult result, out string validationError)
    {
        result = new BugDraftPolishResult();
        validationError = string.Empty;

        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            validationError = "AI response is empty.";
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(aiResponse);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                validationError = "Response must be a JSON object.";
                return false;
            }

            if (!TryReadString(root, "title", out var title) || string.IsNullOrWhiteSpace(title))
            {
                validationError = "title is required.";
                return false;
            }

            if (!TryReadString(root, "summary", out var summary) || string.IsNullOrWhiteSpace(summary))
            {
                validationError = "summary is required.";
                return false;
            }

            if (!TryReadString(root, "actualResult", out var actualResult) || string.IsNullOrWhiteSpace(actualResult))
            {
                validationError = "actualResult is required.";
                return false;
            }

            result = new BugDraftPolishResult
            {
                Title = title,
                Summary = summary,
                ActualResult = actualResult
            };
            return true;
        }
        catch (JsonException ex)
        {
            validationError = $"Invalid JSON response: {ex.Message}";
            return false;
        }
    }

    private static bool TryReadString(JsonElement root, string key, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(key, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return true;
    }
}
