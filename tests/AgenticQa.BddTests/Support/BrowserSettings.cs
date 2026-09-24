namespace AgenticQa.BddTests.Support;

public sealed class BrowserSettings
{
    public string Browser { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public string BaseUrl { get; set; } = "./TestApp/";
}
