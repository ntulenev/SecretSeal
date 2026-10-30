using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace SecretSeal.BrowserTests;

public sealed class BrowserSmokeTests : IClassFixture<RunningAppFixture>
{
    private readonly RunningAppFixture _app;

    public BrowserSmokeTests(RunningAppFixture app)
    {
        _app = app;
    }

    [Fact(DisplayName = "A shared link reveals the secret once, then becomes unavailable")]
    [Trait("Category", "SmokeE2E")]
    public async Task SharedLinkCanBeOpenedOnlyOnceAsync()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        var page = await browser.NewPageAsync();
        const string secret = "Only the recipient sees this 🔐";
        string? postedBody = null;
        page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/notes", StringComparison.Ordinal))
            {
                postedBody = request.PostData;
            }
        };

        _ = await page.GotoAsync(_app.BaseUrl);
        await page.Locator("#createNote").FillAsync(secret);
        await page.Locator("#createBtn").ClickAsync();
        await Expect(page.Locator("#createResult")).ToContainTextAsync("Secret sealed");

        var link = (await page.Locator("#linkBox").TextContentAsync())!.Trim();
        Assert.StartsWith($"{_app.BaseUrl}read.html?id=", link, StringComparison.Ordinal);
        Assert.Contains("#k=", link, StringComparison.Ordinal);
        Assert.NotNull(postedBody);
        Assert.DoesNotContain(secret, postedBody, StringComparison.Ordinal);

        var recipient = await browser.NewPageAsync();
        _ = await recipient.GotoAsync(link);
        await recipient.Locator("#openBtn").ClickAsync();
        await Expect(recipient.Locator("#result")).ToContainTextAsync(secret);
        await Expect(recipient.Locator("#openBtn")).ToBeDisabledAsync();

        var secondVisit = await browser.NewPageAsync();
        _ = await secondVisit.GotoAsync(link);
        await secondVisit.Locator("#openBtn").ClickAsync();
        await Expect(secondVisit.Locator("#result")).ToContainTextAsync("Note not found (or already consumed).");
    }

    [Fact(DisplayName = "The home page reads a composite secret ID")]
    [Trait("Category", "SmokeE2E")]
    public async Task HomePageReadsCompositeIdAsync()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        var page = await browser.NewPageAsync();
        const string secret = "home page recipient";

        _ = await page.GotoAsync(_app.BaseUrl);
        await page.Locator("#createNote").FillAsync(secret);
        await page.Locator("#createBtn").ClickAsync();
        await Expect(page.Locator("#createResult")).ToContainTextAsync("Secret sealed");

        var compositeId = (await page.Locator("#guidBox").TextContentAsync())!.Trim();
        await page.Locator("#readId").FillAsync(compositeId);
        await page.Locator("#readBtn").ClickAsync();

        await Expect(page.Locator("#readResult")).ToContainTextAsync(secret);
    }

    [Fact(DisplayName = "An incomplete recipient link never offers to consume a secret")]
    [Trait("Category", "SmokeE2E")]
    public async Task MissingIdDisablesOpenButtonAsync()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        var page = await browser.NewPageAsync();

        _ = await page.GotoAsync($"{_app.BaseUrl}read.html");

        await Expect(page.Locator("#openBtn")).ToBeDisabledAsync();
        await Expect(page.Locator("#result")).ToContainTextAsync("No secret ID provided in URL.");
    }

    private static async Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright) =>
        await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
}

public sealed class RunningAppFixture : IAsyncLifetime
{
    private Process? _process;

    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var repository = FindRepositoryRoot();
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        var appDirectory = Path.Combine(repository, "src", "SecretSeal");
        var assembly = Path.Combine(appDirectory, "bin", configuration, "net10.0", "SecretSeal.dll");

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        BaseUrl = $"http://127.0.0.1:{port}/";

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = appDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(assembly);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(BaseUrl);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        startInfo.Environment["Storage__Mode"] = "InMemory";
        startInfo.Environment["Crypto__Key"] = "12345678901234567890123456789012";
        startInfo.Environment["Validation__MaxNoteLength"] = "4000";
        startInfo.Environment["Logging__LogLevel__Default"] = "Warning";
        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("The app process did not start.");

        using var client = new HttpClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!timeout.IsCancellationRequested)
        {
            if (_process.HasExited)
            {
                throw new InvalidOperationException($"The app exited with code {_process.ExitCode}.");
            }

            try
            {
                using var response = await client.GetAsync($"{BaseUrl}hc", timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // The server has not bound its port yet.
            }

            await Task.Delay(100, timeout.Token);
        }

        throw new TimeoutException("The app did not become healthy within 30 seconds.");
    }

    public Task DisposeAsync()
    {
        if (_process is not null)
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit();
            }

            _process.Dispose();
        }

        return Task.CompletedTask;
    }

    private static string FindRepositoryRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "SecretSeal", "SecretSeal.csproj")))
            {
                return current.FullName;
            }
        }

        throw new DirectoryNotFoundException("The SecretSeal repository root was not found.");
    }
}
