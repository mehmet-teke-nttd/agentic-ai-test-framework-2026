using Microsoft.Playwright;

namespace AgenticQa.BddTests.Pages;

public sealed class SauceDemoLoginPage : BasePage
{
    public SauceDemoLoginPage(IPage page) : base(page)
    {
    }

    private ILocator UsernameInput => Page.GetByPlaceholder("Username");
    private ILocator PasswordInput => Page.GetByPlaceholder("Password");
    private ILocator LoginButton => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Login" });
    private ILocator ErrorMessage => Page.Locator("[data-test='error']");

    public Task NavigateAsync(string url) => Page.GotoAsync(url);

    public async Task LoginAsAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public Task<string> GetTitleAsync() => Page.TitleAsync();

    public Task<bool> IsOnInventoryPageAsync()
    {
        var isInventoryUrl = Page.Url.Contains("/inventory.html", StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(isInventoryUrl);
    }

    public Task<string?> GetErrorMessageAsync() => ErrorMessage.TextContentAsync();
}
