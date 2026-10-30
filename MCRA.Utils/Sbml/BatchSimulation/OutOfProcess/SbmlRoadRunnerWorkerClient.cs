using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto;

namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess {

    /// <summary>
    /// Wraps a single, persistent out-of-process RoadRunner worker. A <see cref="SbmlRoadRunnerWorkerClient"/> is
    /// shared by the pool across many leases (potentially for different kinetic models and different
    /// users), so it re-initializes the worker with the requested model whenever it differs from the
    /// model that is currently loaded in the worker process.
    /// </summary>
    internal sealed class SbmlRoadRunnerWorkerClient : IDisposable {

        private const int FirstWorkerPort = 46000;
        private static int _nextWorkerPort = FirstWorkerPort;
        private static readonly JsonSerializerOptions JsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };
        private readonly Process _process;
        private readonly HttpClient _httpClient;
        private readonly string _workerDirectory;
        private readonly string _endpoint;
        private readonly StringBuilder _diagnostics = new();
        private readonly Lock _diagnosticsLock = new();
        private readonly SemaphoreSlim _gate = new(1, 1);
        private string _currentModelSignature;

        public SbmlRoadRunnerWorkerClient(
            bool showConsoleWindow,
            bool extensiveConsoleLogging
        ) {
            _workerDirectory = resolveWorkerDirectory();
            var port = Interlocked.Increment(ref _nextWorkerPort) - 1;
            var workerName = "MCRA.Simulation.SbmlRoadRunner.Worker";
            var isWindows = OperatingSystem.IsWindows();
            var startInfo = new ProcessStartInfo {
                FileName = isWindows ? Path.Combine(_workerDirectory, workerName + ".exe") : "dotnet",
                WorkingDirectory = _workerDirectory,
                UseShellExecute = isWindows && showConsoleWindow,
                WindowStyle = showConsoleWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                CreateNoWindow = !showConsoleWindow
            };
            if (!showConsoleWindow) {
                // Drain both pipes asynchronously: otherwise verbose native/Python output can fill a
                // pipe and block the worker. Forward diagnostics to the host trace listeners and keep
                // a bounded tail for exceptions when the worker fails.
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }
            if (!isWindows) {
                if (!OperatingSystem.IsLinux()) {
                    throw new PlatformNotSupportedException("The RoadRunner worker is supported only on Windows and Linux.");
                }
                startInfo.ArgumentList.Add(Path.Combine(_workerDirectory, workerName + ".dll"));
            }
            startInfo.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(extensiveConsoleLogging.ToString());
            _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start RoadRunner worker.");
            _process.EnableRaisingEvents = true;
            _process.Exited += (_, _) => Exited?.Invoke(this);
            if (!showConsoleWindow) {
                _process.OutputDataReceived += (_, e) => captureDiagnostic("stdout", e.Data);
                _process.ErrorDataReceived += (_, e) => captureDiagnostic("stderr", e.Data);
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            _endpoint = $"http://localhost:{port}";
            _httpClient = new HttpClient { BaseAddress = new Uri(_endpoint), Timeout = TimeSpan.FromMinutes(30) };
        }

        public event Action<SbmlRoadRunnerWorkerClient> Exited;

        public bool IsAlive {
            get {
                try { return !_process.HasExited; } catch (InvalidOperationException) { return false; }
            }
        }

        /// <summary>
        /// Runs a batch of simulation jobs on this worker in a single request, (re)initializing the
        /// worker with the given model first if it is not already the model currently loaded in the
        /// worker process. Access to the worker is serialized: only one initialize/simulate request is
        /// in flight at a time, since a single worker process can only usefully run one request at a
        /// time and may be shared by multiple concurrent leases from the pool.
        /// </summary>
        public async Task<List<WorkerJobResultDto>> RunAsync(
            SbmlSimulationBatchJobDto batch,
            SbmlSimulationInitializationDto init
        ) {
            await _gate.WaitAsync().ConfigureAwait(false);
            try {
                if (_process.HasExited) {
                    throw new InvalidOperationException($"RoadRunner worker exited ({_process.ExitCode}).{getDiagnostics()}");
                }

                var signature = computeModelSignature(init);
                if (signature != _currentModelSignature) {
                    var ready = await sendInitializeWhenReady(init).ConfigureAwait(false);
                    if (ready.Type != "ready" || ready.ProtocolVersion != 1) {
                        throw new InvalidOperationException($"RoadRunner worker failed to initialize: {formatError(ready.Error)}");
                    }
                    _currentModelSignature = signature;
                }

                var response = await sendRequest<WorkerResponseDto>("simulate", batch).ConfigureAwait(false);
                if (response.Type != "result") {
                    throw new InvalidOperationException($"RoadRunner worker failed batch of {batch.Jobs.Count} job(s): {formatError(response.Error)}");
                }
                return response.Results ?? [];
            } finally {
                _gate.Release();
            }
        }

        private void captureDiagnostic(string stream, string line) {
            if (line == null) return;
            var message = $"[SBML worker {_process.Id} {stream}] {line}";
            Trace.WriteLine(message);
            lock (_diagnosticsLock) {
                _diagnostics.AppendLine(message);
                if (_diagnostics.Length > 16_000) _diagnostics.Remove(0, _diagnostics.Length - 16_000);
            }
        }

        private string getDiagnostics() {
            lock (_diagnosticsLock) return _diagnostics.ToString();
        }

        /// <summary>
        /// Resolves the build output directory for the stand-alone SbmlRoadrunnerWorker executable.
        /// The worker's output should live next to this project's own output, under the subfolder SbmlRoadrunnerWorker.<Configuration>".
        /// </summary>
        private static string resolveWorkerDirectory() {
            const string workerProjectName = "MCRA.Simulation.SbmlRoadRunner.Worker";
            const string workerDllName = workerProjectName + ".dll";

            // AppContext.BaseDirectory is this project's own "bin\<Configuration>\" output folder.
            var ownBinDirectory = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var configurationName = ownBinDirectory.Name;

            var workerDirectory = Path.Combine(ownBinDirectory.FullName, "SbmlRoadrunnerWorker");
            if (!File.Exists(Path.Combine(workerDirectory, workerDllName))) {
                throw new FileNotFoundException(
                    $"Could not find {workerDllName} in '{workerDirectory}'. Make sure the {workerProjectName} " +
                    $"project has been built with the '{configurationName}' configuration.",
                    workerDllName
                );
            }
            return workerDirectory;
        }

        private async Task<TResponse> sendRequest<TResponse>(string endpoint, object request) {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, request, JsonOptions).ConfigureAwait(false);
            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<TResponse>(payload, JsonOptions)
                ?? throw new InvalidDataException("RoadRunner worker returned an empty response.");
            if (!response.IsSuccessStatusCode && result is WorkerResponseDto workerResponse) {
                var exception = new InvalidOperationException($"RoadRunner worker request failed: {formatError(workerResponse.Error)}");
                if (!string.IsNullOrWhiteSpace(workerResponse.JobId)) {
                    exception.Data["JobId"] = workerResponse.JobId;
                }
                throw exception;
            }
            return result;
        }

        private string formatError(string error) => string.IsNullOrWhiteSpace(error)
            ? $"No error details were returned by the worker.{Environment.NewLine}{getDiagnostics()}"
            : $"{error}{Environment.NewLine}{getDiagnostics()}";

        private static string computeModelSignature(SbmlSimulationInitializationDto init) {
            return init.ModelFileName;
        }

        private async Task<WorkerResponseDto> sendInitializeWhenReady(SbmlSimulationInitializationDto initialization) {
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (true) {
                if (_process.HasExited) {
                    throw new InvalidOperationException($"RoadRunner worker exited ({_process.ExitCode}).{getDiagnostics()}");
                }
                try {
                    return await sendRequest<WorkerResponseDto>("initialize", initialization).ConfigureAwait(false);
                } catch (HttpRequestException) when (DateTime.UtcNow < deadline) {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
        }

        public void Dispose() {
            try {
                if (!_process.WaitForExit(2000)) {
                    _process.Kill(true);
                }
            } catch { }
            try { _process.Dispose(); } catch { }
            try { _httpClient.Dispose(); } catch { }
            _gate.Dispose();
        }
    }
}
