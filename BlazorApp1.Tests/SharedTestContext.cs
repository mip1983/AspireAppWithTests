using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace BlazorApp1.Tests;

/// <summary>
/// Spins up one instance of Playwright for all tests in a class.
/// </summary>
public class SharedTestContext : IAsyncLifetime
{
    public IPlaywright? PlaywrightInstance { get; private set; }

    public IBrowser Browser { get; set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        Console.WriteLine("info: SharedTestContext InitializeAsync called");

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddUserSecrets(typeof(SharedTestContext).Assembly)
            .AddEnvironmentVariables();

        PlaywrightInstance = await Playwright.CreateAsync();
        bool headless = false;

        var browserOptions = new BrowserTypeLaunchOptions
        {
            Headless = headless,
            SlowMo = 10,
            Timeout = 30000
        };

        Browser = await PlaywrightInstance.Chromium.LaunchAsync(browserOptions);

        Console.WriteLine("info: SharedTestContext InitializeAsync completed");
    }

    public virtual async ValueTask DisposeAsync()
    {
        Console.WriteLine("info: SharedTestContext DisposeAsync called");

        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        PlaywrightInstance?.Dispose();

        Console.WriteLine("info: SharedTestContext DisposeAsync completed");

        GC.SuppressFinalize(this);
    }

    public async Task Reset()
    {
        if (PlaywrightInstance == null)
        {
            return;
        }

        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 10,
            Timeout = 30000
        });
    }
}

