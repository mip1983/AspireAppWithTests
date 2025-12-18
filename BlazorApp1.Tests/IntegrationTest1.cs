using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace BlazorApp1.Tests;

[Collection("SharedTestContext")]
public class IntegrationTest1(SharedTestContext testContext)
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private readonly SharedTestContext testContext = testContext;

    private IPage? Page { get; set; }

    private IBrowserContext? BrowserContext { get; set; }

    [Fact]
    public async Task LoadSiteAndTypeInTextbox()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireAppWithTests_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("blazorapp1");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("blazorapp1", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        using var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var uri = app.GetEndpoint("blazorapp1", "https");

        BrowserContext ??= await this.testContext.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1280,
                Height = 720
            }
        });

        Page ??= await BrowserContext.NewPageAsync();
        await Page.GotoAsync(uri.ToString());
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var textboxLocator = Page.Locator("textarea#userInput");
        await textboxLocator.IsVisibleAsync();
        await textboxLocator.IsEditableAsync();
        await textboxLocator.FocusAsync();

        await textboxLocator.PressSequentiallyAsync(
        $"""
            Debugging this test in visual studio will eventually fail as a window will popup complaining that it can't 
            attach to the browser that also opened with an about blank page. The message will reference a 
            localhost url that isn't the same as the one spun up by the test host, which is '{uri}'.
        """
        , new()
        {
            Delay = 50,
            Timeout = 60000
        });

        var context = await textboxLocator.InputValueAsync();
        Assert.NotNull(context);
        Assert.Contains("Debugging this test in visual studio will eventually fail", context);
    }
}
