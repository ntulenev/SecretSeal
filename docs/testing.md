# Testing and coverage

## Coverage audit (30 October 2026)

The solution has unit tests for models, transport validation, cryptography, note handlers, cleanup, and use cases. Integration tests exercise the HTTP endpoints and static assets with `WebApplicationFactory`, plus repository operations and database creation on SQL Server LocalDB. Browser smoke tests start the real app on a loopback port and use Playwright to exercise the JavaScript UI.

| Layer | Main scenarios | Test methods |
| --- | --- | ---: |
| Unit | Validation, identifiers, encryption, note handlers, cleanup, use cases | 147 |
| Integration | API responses, one-time consumption, static pages and scripts, SQL Server persistence | 18 |
| Browser smoke | Shared link, browser-side encryption and decryption, one-time read, home-page read, incomplete link and missing key | 4 |

The complete solution run passed **200 test cases**. The previous solution had **193** and no browser smoke tests. The `XPlat Code Coverage` reports were merged by source file and line number, excluding generated `obj` code. They show **587 of 592 instrumented C# lines (99.2%)** covered. By project: Cryptography 85/86, Logic 93/94, Models 25/25, SecretSeal 229/230, Storage 71/71, and Transport 84/86. This is C# line coverage; it does not measure JavaScript statements or prove that all failure modes are tested.

The important functional gap was the user flow in the browser. Server-side API tests could pass even if the UI failed to encrypt a secret, build a usable link, decrypt it, or prevent a second read. The new browser tests cover that path. The new integration tests verify static pages and scripts are served and that two clients observe the same one-time secret.

Remaining areas for later work include concurrent consumers against SQL Server, cleanup running over time in a hosted process, and browser behavior under network failure. These need dedicated fixtures or timing controls; current tests cover their lower-level components.

## Run locally

From the repository root:

```sh
dotnet test src/SecretSeal.slnx
```

The full suite uses SQL Server LocalDB and, on Windows, Microsoft Edge for the browser smoke tests. To run only the browser tests:

```sh
dotnet test src/SecretSeal.BrowserTests/SecretSeal.BrowserTests.csproj
```

On Linux, build the browser test project and install Playwright Chromium before running it:

```sh
dotnet build src/SecretSeal.BrowserTests/SecretSeal.BrowserTests.csproj
pwsh src/SecretSeal.BrowserTests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
dotnet test src/SecretSeal.BrowserTests/SecretSeal.BrowserTests.csproj --no-build
```

The browser smoke workflow runs this sequence on pull requests and pushes. To collect C# coverage locally:

```sh
dotnet test src/SecretSeal.slnx --collect:"XPlat Code Coverage" --results-directory TestResults
```

The collector creates one Cobertura XML report per test assembly under `TestResults/`. Browser JavaScript execution is verified by the smoke tests but is outside Coverlet's C# coverage metric.
