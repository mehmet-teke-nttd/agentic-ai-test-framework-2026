using Microsoft.Playwright;

namespace AgenticQa.BddTests.Pages;

public sealed class LoginPage : BasePage
{
    public LoginPage(IPage page) : base(page)
    {
    }

    private ILocator UsernameInput => Page.GetByLabel("Username");
    private ILocator PasswordInput => Page.GetByLabel("Password");
    private ILocator LoginButton => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Login" });

    public async Task NavigateAsync(string baseUrl)
    {
        var targetUrl = new Uri(new Uri(baseUrl, UriKind.Absolute), "mock-login.html").ToString();
        await Page.GotoAsync(targetUrl);
    }

    public async Task LoginAsAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
