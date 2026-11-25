using Digital_Fridge.Tests.Infrastructure;
using Microsoft.Playwright;

namespace Digital_Fridge.Tests.E2E;

/// <summary>
/// Spins up the real Kestrel-hosted app (SQLite-backed) and a headless Chromium browser for E2E tests.
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public TestAppFactory AppFactory { get; } = new();
    public string BaseUrl { get; private set; } = string.Empty;
    private IPlaywright _playwright = default!;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await AppFactory.InitializeAsync();

        // Creating the first client triggers server startup; BaseAddress is then populated.
        _ = AppFactory.CreateClient();
        BaseUrl = AppFactory.Server.BaseAddress.ToString().TrimEnd('/');

        _playwright = await Playwright.CreateAsync();
        var executablePath = ResolveChromiumPath();

        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
            ExecutablePath = executablePath,
            Headless = true,
            Args = ["--no-sandbox", "--disable-dev-shm-usage"]
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }
        _playwright?.Dispose();
        await AppFactory.DisposeAsync();
    }

    private static string? ResolveChromiumPath()
    {
        string[] candidates = [
            "/usr/bin/chromium-browser",
            "/usr/bin/chromium",
            "/usr/bin/google-chrome"
        ];
        return candidates.FirstOrDefault(File.Exists);
    }
}
