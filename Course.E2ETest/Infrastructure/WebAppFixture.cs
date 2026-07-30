using System.Diagnostics;

namespace Course.E2ETest.Infrastructure;

/// <summary>
/// Levanta Course.Api y Course.Web como procesos reales (a partir de sus .dll ya compilados)
/// para poder ejecutar pruebas E2E con Playwright contra el flujo completo del Home.
/// </summary>
public sealed class WebAppFixture : IAsyncLifetime
{
    private const string ApiUrl = "http://localhost:5151";
    private const string WebUrl = "http://localhost:5206";

    private static readonly HttpClient HealthCheckClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private Process? _apiProcess;
    private Process? _webProcess;

    public string ApiBaseUrl => ApiUrl;

    public string WebBaseUrl => WebUrl;

    public async Task InitializeAsync()
    {
        var repositoryRoot = FindRepositoryRoot();

        _apiProcess = StartServer(
            dllPath: Path.Combine(repositoryRoot, "src", "Course.Api", "bin", "Debug", "net10.0", "Course.Api.dll"),
            urls: ApiUrl,
            extraEnvironmentVariables: null);

        await WaitForHealthyAsync($"{ApiUrl}/health", "Course.Api");

        _webProcess = StartServer(
            dllPath: Path.Combine(repositoryRoot, "src", "Course.Web", "bin", "Debug", "net10.0", "Course.Web.dll"),
            urls: WebUrl,
            extraEnvironmentVariables: new Dictionary<string, string>
            {
                ["CourseApi__BaseUrl"] = ApiUrl
            });

        await WaitForHealthyAsync($"{WebUrl}/health", "Course.Web");
    }

    public Task DisposeAsync()
    {
        KillProcess(_webProcess);
        KillProcess(_apiProcess);
        return Task.CompletedTask;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException(
                "No se pudo localizar la raiz del repositorio (archivo .sln) a partir del directorio de salida de las pruebas.");
        }

        return directory.FullName;
    }

    private static Process StartServer(string dllPath, string urls, IDictionary<string, string>? extraEnvironmentVariables)
    {
        if (!File.Exists(dllPath))
        {
            throw new InvalidOperationException(
                $"No se encontro el ensamblado '{dllPath}'. Compila la solucion (incluyendo Course.Api y Course.Web) antes de ejecutar las pruebas E2E.");
        }

        var startInfo = new ProcessStartInfo("dotnet", $"\"{dllPath}\"")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(dllPath)
        };

        startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = urls;
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";

        if (extraEnvironmentVariables is not null)
        {
            foreach (var (key, value) in extraEnvironmentVariables)
            {
                startInfo.EnvironmentVariables[key] = value;
            }
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"No se pudo iniciar el proceso para '{dllPath}'.");

        return process;
    }

    private static async Task WaitForHealthyAsync(string healthUrl, string serverName)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await HealthCheckClient.GetAsync(healthUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // El servidor aun no esta listo, se reintenta.
            }
            catch (TaskCanceledException)
            {
                // Timeout de la peticion de healthcheck, se reintenta.
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"{serverName} no respondio exitosamente en '{healthUrl}' dentro del tiempo esperado.");
    }

    private static void KillProcess(Process? process)
    {
        if (process is null || process.HasExited)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }
        catch (InvalidOperationException)
        {
            // El proceso ya finalizo.
        }
        finally
        {
            process.Dispose();
        }
    }
}
