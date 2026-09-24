using AgenticQa.Core.Enums;
using Microsoft.Playwright;

namespace AgenticQa.Core.Playwright;

internal static class PlaywrightAriaRoleParser
{
    public static bool TryParse(string roleValue, out AriaRole role)
    {
        switch (roleValue.Trim().ToLowerInvariant())
        {
            case "button":
                role = AriaRole.Button;
                return true;
            case "textbox":
                role = AriaRole.Textbox;
                return true;
            case "checkbox":
                role = AriaRole.Checkbox;
                return true;
            case "link":
                role = AriaRole.Link;
                return true;
            case "heading":
                role = AriaRole.Heading;
                return true;
            default:
                role = default;
                return false;
        }
    }

    public static LocatorResolutionException CreateInvalidRoleException(string roleValue) =>
        new(
            LocatorValidationErrorCode.InvalidLocatorConfig,
            $"Unsupported aria role value: {roleValue}");
}
