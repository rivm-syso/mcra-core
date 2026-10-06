using System.Globalization;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker;
using Python.Runtime;

namespace MCRA.Simulation.SbmlRoadRunner.Worker {
    /// <summary>
    /// Runs RoadRunner SBML simulations in this (out-of-process) worker. Mirrors the structure of
    /// <see cref="MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.SbmlModelRunner"/>
    /// (the in-process equivalent), except that the model is loaded once, at construction time, from
    /// the initialization payload, rather than lazily on first use.
    /// <para>
    /// IMPORTANT (GIL/threading): unlike <c>SbmlModelRunner</c>, this worker is driven by ASP.NET Core
    /// request handlers. Its "/initialize" and "/simulate" endpoints can (and, because "/simulate" uses
    /// <c>ConfigureAwait(false)</c>, very likely will) execute on different OS threads than one another,
    /// and even different threads between successive "/simulate" calls. The Python GIL
    /// (<see cref="Py.GIL"/>/<c>PyGILState_Ensure</c>) is acquired per-thread: if it were acquired once
    /// and held for the lifetime of this object (as a field), any Python call made from a different
    /// thread than the one that acquired it would block forever, because the holding thread never
    /// releases it. To avoid this, the GIL is acquired and released around each individual
    /// Python-touching operation (constructor and <see cref="RunBatch"/>) instead, which is safe to
    /// do from any thread.
    /// </para>
    /// </summary>
    internal sealed class RoadRunnerWorker : IDisposable {

        public static bool ReuseEvents = true;

        private readonly WorkerInitializationDto _initialization;
        private readonly string _idBodyWeightParameter;

        private dynamic _roadrunner;
        private dynamic _model;
        private readonly Dictionary<string, string> _eventCache = [];
        private readonly bool _extensiveConsoleLogging;

        public RoadRunnerWorker(WorkerInitializationDto initialization, bool extensiveConsoleLogging) {
            _initialization = initialization;
            _extensiveConsoleLogging = extensiveConsoleLogging;

            if (_extensiveConsoleLogging) {
                Console.Error.WriteLine($"Worker process {Environment.ProcessId} starting up with model '{initialization.ModelFileName}'...");
            }

            // Body weight parameter. Prefer the body weight parameter id resolved from the full PBK
            // model specification snapshot (if supplied), falling back to the flat BodyWeightParameterId
            // field for backward compatibility with older callers that do not populate PbkModelSpecification.
            _idBodyWeightParameter = _initialization.PbkModelSpecification
                ?.GetParameterDefinitionByType(PbkModelParameterType.BodyWeight)?.Id
                ?? _initialization.BodyWeightParameterId;

            if (string.IsNullOrEmpty(_initialization.ModelFileContent)) {
                throw new InvalidDataException($"No SBML model content supplied (ModelFileName: '{_initialization.ModelFileName}').");
            }

            // Initialize python once, for the lifetime of the process. Note we deliberately do NOT
            // acquire/hold the GIL here (see class remarks above) - only the Python engine itself is
            // initialized; the GIL is acquired/released per call, below and in Run().
            if (!PythonEngine.IsInitialized) {
                var pythonDll = Environment.GetEnvironmentVariable("PYTHONNET_PYDLL");
                if (!string.IsNullOrWhiteSpace(pythonDll)) {
                    Runtime.PythonDLL = pythonDll;
                }
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            using (Py.GIL()) {
                // Import roadrunner and read model. RoadRunner accepts either a file path or the raw SBML
                // XML content; the model's XML content is transferred inline from the caller (MCRA.Simulation.Web)
                // because this worker process does not necessarily share a file system with it.
                _roadrunner = Py.Import("roadrunner");
                _model = _roadrunner.RoadRunner(_initialization.ModelFileContent);
                _model.integrator.maximum_num_steps = _initialization.MaximumNumSteps;

                // Set boundary condition for inputs (for discrete/bolus dosing events)
                foreach (var input in _initialization.RouteInputMappings.Values.Distinct()) {
                    _model.setInitAmount(input, 0);
                    _model.setConstant(input, false);
                    _model.setBoundary(input, false);
                }
            }
        }

        /// <summary>Resets the model to its origin, ready for a new simulation run.</summary>
        private void resetModel() {
            _model.resetToOrigin();
            _model.resetAll();
            _model.integrator.maximum_num_steps = 1000000;
        }

        public List<WorkerJobResultDto> RunBatch(List<SimulationJobDto> jobs) {
            if (_extensiveConsoleLogging) {
                Console.Error.WriteLine($"Worker process {Environment.ProcessId} running batch of {jobs.Count} simulation(s)");
            }

            // Acquire the GIL once for the whole batch (see class remarks: this method may be invoked
            // from a different thread than the constructor, or than a previous call to RunBatch).
            // Holding it for the duration of the batch (rather than per job) also avoids repeated
            // GIL acquire/release overhead across many jobs.
            using (Py.GIL()) {
                var results = new List<WorkerJobResultDto>(jobs.Count);
                foreach (var job in jobs) {
                    try {
                        results.Add(new WorkerJobResultDto {
                            JobId = job.JobId,
                            Result = runSingle(job)
                        });
                    } catch (InvalidOperationException ex) {
                        // Design choice: stop batch immediately when one job fails, to prevent waiting to finish
                        // long calculations of the batch and still fail the complete batch afterall.
                        Console.Error.WriteLine($"Simulation job '{job.JobId}' failed:{Environment.NewLine}{ex}");
                        results.Add(new WorkerJobResultDto {
                            JobId = job.JobId,
                            Error = ex.Message
                        });
                        return results;
                    } catch (Exception ex) {
                        // Design choice: stop batch immediately when one job fails, to prevent waiting to finish
                        // long calculations of all jobs in the batch and still decide to fail the complete batch afterall.
                        Console.Error.WriteLine($"Simulation job '{job.JobId}' failed:{Environment.NewLine}{ex}");
                        throw;
                    }
                }
                return results;
            }
        }

        private WorkerSimulationResultDto runSingle(SimulationJobDto job) {
            if (_extensiveConsoleLogging) {
                Console.WriteLine($"Worker process {Environment.ProcessId} running simulation in runSingle: {job.JobId}");
            }
            resetModel();

            // Create model exposure events
            var eventsChanged = setExposureEvents(
                _initialization.RouteInputMappings,
                job.Events,
                job.ApplyBodyWeightScaling
            );

            // Set model parameters
            if (job.Parameters != null) {
                foreach (var parameter in job.Parameters) {
                    _model.__setattr__(parameter.Key, parameter.Value);
                }
            }

            // Regenerate model only if events changed or not reusing
            if (eventsChanged) {
                _model.regenerateModel(true, true);
            }

            // Specify selected output for roadrunner, including time
            var outputSelection = new List<string>() { "time" }.Union(job.OutputTimeSeries).ToArray();

            // Determine total evaluation period and output steps and run the model
            var simulationOutput = _model.simulate(0, job.Duration, job.Steps, outputSelection);

            // Get output data and store in result
            var output = (double[][])simulationOutput.T.As<double[][]>();
            var result = new WorkerSimulationResultDto {
                Time = output[0],
                OutputTimeSeries = [],
                OutputStates = []
            };
            for (var i = 0; i < job.OutputTimeSeries.Count; i++) {
                // Trim concentration brackets. The nature of the output (i.e. concentration
                // or amount) should be known by the calling function.
                var idOutput = job.OutputTimeSeries[i].Trim('[', ']');
                result.OutputTimeSeries[idOutput] = output[i + 1];
            }

            // Get selected output states at end of simulation
            foreach (var id in job.OutputStates) {
                result.OutputStates[id] = (double)_model[id];
            }

            return result;
        }

        private bool setExposureEvents(
            Dictionary<string, string> routeInputMappings,
            List<ExposureEventDto> exposureEvents,
            bool applyBodyweightScaling
        ) {
            var changed = false;
            var bodyweightMultiplication = applyBodyweightScaling
                ? $"*{_idBodyWeightParameter}" : string.Empty;

            static string getFingerprint(ExposureEventDto e) {
                return e.Kind == "single"
                    ? $"S_{e.Route}_{n(e.Time)}"
                    : $"R_{e.Route}_{n(e.Time)}";
            }

            var clearEvents = !ReuseEvents;
            if (ReuseEvents && _eventCache != null) {
                // Check if events match cache
                var eventFingerprints = exposureEvents.Select(getFingerprint).ToHashSet();
                clearEvents = eventFingerprints.Count != _eventCache.Count
                    || !eventFingerprints.SetEquals(_eventCache.Keys);
            }

            if (clearEvents) {
                // Clear cache and remove old events
                var eventIds = _model.model.getEventIds();
                foreach (var eventId in eventIds) {
                    _model.removeEvent(eventId, false);
                }
                _eventCache?.Clear();
                changed = true;
            }

            // Create events table dictionary with (target, time) as key and assignment as value
            var eidCounter = 1;
            foreach (var exposureEvent in exposureEvents) {
                if (!routeInputMappings.TryGetValue(exposureEvent.Route, out var speciesId)) {
                    throw new InvalidDataException($"No SBML input species mapping for route '{exposureEvent.Route}'.");
                }

                var trigger = exposureEvent.Kind == "repeating"
                    ? $"time % {n(exposureEvent.Interval)} == 0"
                    : $"time > {n(exposureEvent.Time)}";

                if (ReuseEvents) {
                    var doseParamId = string.Empty;
                    // Try to reuse events. SUCCESS: No recompilation.
                    // Instant C++ backend update via __setattr__
                    var fingerprint = getFingerprint(exposureEvent);
                    if (_eventCache.TryGetValue(fingerprint, out var eventId)) {
                        // Set dose parameter value of the event
                        // --- INDIVIDUAL 2+ (Or Cached Path) ---
                        doseParamId = $"dose_{eventId}";
                    } else {
                        // Event not found; create event and add to cache
                        eventId = $"ev_{eidCounter++}";
                        doseParamId = $"dose_{eventId}";
                        var assignment = $"{speciesId} + {doseParamId}{bodyweightMultiplication}";
                        // Set up model structure without intermediate recompilation (forceRegenerate = false)
                        _model.addParameter(doseParamId, 0, false);
                        _model.setConstant(doseParamId, false);
                        _model.addEvent(eventId, false, trigger, false);
                        _model.addEventAssignment(eventId, speciesId, assignment, false);
                        _eventCache.Add(fingerprint, eventId);
                        changed = true;
                    }
                    // CRITICAL: Explicitly set the initial value metadata so it survives the reset lifecycle
                    _model.setValue($"init({doseParamId})", exposureEvent.Value);
                    // Set the live tracking variable value for immediate time execution
                    _model.__setattr__(doseParamId, exposureEvent.Value);
                } else {
                    // Legacy fallback: Hardcodes the string value, forcing a recompile every single time
                    var value = n(exposureEvent.Value);
                    var assignment = $"{speciesId} + {value}{bodyweightMultiplication}";
                    var eventId = $"ev_{eidCounter++}";
                    _model.addEvent(eventId, false, trigger, false);
                    _model.addEventAssignment(eventId, speciesId, assignment, false);
                    changed = true;
                }
            }
            // Events changed or newly created
            return changed;
        }

        private static string n(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        public void Dispose() {
            // Python objects (_roadrunner, _model) are released via normal reference counting /
            // garbage collection; there is no persistent GIL handle to release here (see class
            // remarks: the GIL is acquired and released per call, not held for this object's lifetime).
        }
    }
}
