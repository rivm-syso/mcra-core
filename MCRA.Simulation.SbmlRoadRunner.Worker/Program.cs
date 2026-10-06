using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker;

namespace MCRA.Simulation.SbmlRoadRunner.Worker;

/// <summary>
/// Important: keep the Main and the static Program class in the traditional namespace and not as new top-level statements,
/// to avoid conflicts with other top-level Program.cs definitions, since this assembly can be imported in external projects.
/// </summary>
internal static class Program {

    public static async Task Main(string[] args) {
        try {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddJsonFile($"appsettings.user.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.user.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            var config = confBuilder.Build();

            // The first numeric argument overrides the configured application URL (used by the worker pool).
            // When no port is supplied, ASP.NET Core uses the applicationUrl from launchSettings.json.
            var portArgument = args.FirstOrDefault(a => int.TryParse(a, out var value) && value is > 0 and <= 65535);
            var ownerPidArgument = args.SkipWhile(a => a != portArgument).Skip(1).FirstOrDefault();
            var ownerStartTimeArgument = args.SkipWhile(a => a != portArgument).Skip(2).FirstOrDefault();
            var extensiveConsoleLoggingArgument = args.SkipWhile(a => a != portArgument).Skip(3).FirstOrDefault();
            var hostArgs = args.Where(a => a != portArgument && a != ownerPidArgument && a != ownerStartTimeArgument && a != extensiveConsoleLoggingArgument).ToArray();
            var extensiveConsoleLogging = bool.TryParse(extensiveConsoleLoggingArgument, out var parsedLogging) && parsedLogging;

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = hostArgs });
            if (int.TryParse(portArgument, out var requestedPort)) {
                builder.WebHost.UseUrls($"http://localhost:{requestedPort}");
            }

            JsonSerializerOptions JsonOptions = new() {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            // Add services to the container.
            builder.Services.AddOptions();
            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var app = builder.Build();

            // The worker is owned by the MCRA process that launched it. This also covers abrupt
            // console closure, where the owner's normal shutdown callbacks may not complete.
            if (int.TryParse(ownerPidArgument, out var ownerPid) &&
                long.TryParse(ownerStartTimeArgument, out var ownerStartTimeTicks)) {
                _ = monitorOwnerAsync(ownerPid, ownerStartTimeTicks, app.Lifetime);
            }

            // Set python path
            var pythonPath = config.GetSection("SimulationServiceOptions").GetValue<string>("PythonDllPath");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonPath);

            RoadRunnerWorker runner = null;
            var gate = new SemaphoreSlim(1, 1);
            app.MapPost("/initialize", (WorkerInitializationDto init) => {
                try {
                    bool interactive = config.GetSection("SimulationServiceOptions").GetValue<bool>("InteractiveMode");
                    if (interactive) {
                        Console.WriteLine("Interactive mode - start of execution halted, press enter to continue...");
                        Console.ReadKey();
                        Console.WriteLine();
                    }

                    if (init == null || init.Type != "initialize") {
                        throw new InvalidDataException("Expected initialize request.");
                    }
                    gate.Wait();
                    try {
                        runner?.Dispose();
                        runner = new RoadRunnerWorker(init, extensiveConsoleLogging);
                    } finally {
                        gate.Release();
                    }
                    return Results.Json(new WorkerResponseDto { Type = "ready", ProtocolVersion = 1 }, JsonOptions);
                } catch (Exception ex) {
                    return Results.Json(new WorkerResponseDto { Type = "error", ProtocolVersion = 1, Error = ex.ToString() }, JsonOptions, statusCode: 500);
                }
            });

            app.MapPost("/simulate", async (SimulationJobBatch batch) => {
                await gate.WaitAsync().ConfigureAwait(false);
                try {
                    if (runner == null) throw new InvalidOperationException("Worker has not been initialized.");
                    if (batch == null || batch.Type != "simulate") throw new InvalidDataException("Expected simulate request.");

                    var results = runner.RunBatch(batch.Jobs);

                    var type = results.Any(r => r.Error != null) ? "error" : "result";
                    var statusCode = type == "error" ? 500 : 200;
                    var msg = results.FirstOrDefault(r => r.Error != null)?.Error;
                    var jobId = results.FirstOrDefault(r => r.Error != null)?.JobId;
                    return Results.Json(new WorkerResponseDto {
                        Type = type,
                        ProtocolVersion = 1,
                        JobId = jobId,
                        Error = msg,
                        Results = results
                    }, JsonOptions, statusCode: statusCode);
                } catch (Exception ex) {
                    return Results.Json(new WorkerResponseDto {
                        Type = "error",
                        ProtocolVersion = 1,
                        Error = ex.Message,
                    }, JsonOptions, statusCode: 500);
                } finally {
                    gate.Release();
                }
            });

            //use HTTPS in a non-development environment
            if (config.GetValue<bool>("UseHttpsRedirection")) {
                app.UseHttpsRedirection();
            }
            app.MapControllers();

            app.Run();
            runner?.Dispose();
        } catch (Exception ex) {
            Console.Error.WriteLine($"Worker process {Environment.ProcessId} exiting with error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            throw;
        }
    }

    private static async Task monitorOwnerAsync(int ownerPid, long ownerStartTimeTicks, IHostApplicationLifetime lifetime) {
        while (!lifetime.ApplicationStopping.IsCancellationRequested) {
            if (!isOwnerAlive(ownerPid, ownerStartTimeTicks)) {
                lifetime.StopApplication();
                return;
            }
            try {
                await Task.Delay(TimeSpan.FromSeconds(2), lifetime.ApplicationStopping).ConfigureAwait(false);
            } catch (OperationCanceledException) when (lifetime.ApplicationStopping.IsCancellationRequested) {
                return;
            }
        }
    }

    private static bool isOwnerAlive(int ownerPid, long ownerStartTimeTicks) {
        try {
            using var owner = System.Diagnostics.Process.GetProcessById(ownerPid);
            if (owner.HasExited) {
                return false;
            }

            // Process start times can be rounded to the OS clock's resolution, so an exact
            // tick comparison can incorrectly identify the live owner as a recycled PID.
            var actualStartTimeTicks = owner.StartTime.ToUniversalTime().Ticks;
            return Math.Abs(actualStartTimeTicks - ownerStartTimeTicks) <= TimeSpan.TicksPerSecond;
        } catch (ArgumentException) {
            return false;
        } catch (InvalidOperationException) {
            return false;
        } catch (Win32Exception) {
            // If the process exists but its metadata cannot be read, do not terminate a worker
            // based on an inconclusive owner check.
            return true;
        } catch (UnauthorizedAccessException) {
            return true;
        }
    }
}
