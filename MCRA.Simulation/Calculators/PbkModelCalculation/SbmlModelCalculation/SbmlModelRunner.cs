using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration;
using Python.Runtime;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation {
    public class SbmlModelRunner : IDisposable {

        public static bool ReuseEvents = true;

        private bool _disposed = false;

        private readonly string _modelFileName;
        private readonly string _idBodyWeightParameter;
        private readonly IPbkModelSpecification _pbkModelSpecification;
        private readonly Py.GILState _gil;

        private dynamic _rr = null;
        private dynamic _model = null;
        private readonly Dictionary<string, string> _eventCache = [];

        public SbmlModelRunner(
            ISbmlPbkModelSpecification pbkModelSpecification,
            KineticModelInstance modelInstance
        ) {
            _pbkModelSpecification = pbkModelSpecification;
            _modelFileName = modelInstance.KineticModelDefinition.FileName;
            // Body weight parameter
            _idBodyWeightParameter = _pbkModelSpecification
                .GetParameterDefinitionByType(PbkModelParameterType.BodyWeight)?.Id;

            // Initialize python and acquire GIL for the lifetime of the runner
            if (!PythonEngine.IsInitialized) {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
            _gil = Py.GIL();
        }

        ~SbmlModelRunner() {
            Dispose(false);
        }

        private void initializeModel(Dictionary<ExposureRoute, string> routeInputMappings) {
            // Import roadrunner and read model
            if (_model == null) {
                // Load model
                _rr = Py.Import("roadrunner");
                _model = _rr.RoadRunner(_modelFileName);

                // Set boundary condition for inputs (for discrete/bolus dosing events)
                foreach (var input in routeInputMappings.Values) {
                    _model.setInitAmount(input, 0);
                    _model.setConstant(input, false);
                    _model.setBoundary(input, false);
                }
            } else {
                _model.resetToOrigin();
                _model.resetAll();
                _model.integrator.maximum_num_steps = 1000000;
            }
        }

        public SimulationOutput Run(
            List<IExposureEvent> exposureEvents,
            Dictionary<string, double> parameters,
            Dictionary<ExposureRoute, string> routeInputMappings,
            List<string> outputStatesSelection,
            List<string> outputTimeSeriesSelection,
            int evaluationPeriod,
            int steps,
            bool applyBodyWeightScaling
        ) {
            // (Re)initialize model
            initializeModel(routeInputMappings);

            // Create model exposure events
            var eventsChanged = setExposureEvents(
                routeInputMappings,
                exposureEvents,
                applyBodyWeightScaling
            );

            // Set model parameters
            if (parameters != null) {
                foreach (var parameter in parameters) {
                    _model.__setattr__(parameter.Key, parameter.Value);
                }
            }

            // Regenerate model only if events changed or not reusing
            if (eventsChanged) {
                _model.regenerateModel(true, true);
            }

            // Specify selected output for roadrunner, including time
            var outputSelection = new List<string>() { "time" }.Union(outputTimeSeriesSelection).ToArray();

            // Determine total evaluation period and output steps and run the model
            var simulationOutput = _model.simulate(0, evaluationPeriod, steps, outputSelection);

            // Get output data and store in result
            var output = (double[][])simulationOutput.T.As<double[][]>();
            var result = new SimulationOutput() {
                Time = output[0],
                OutputTimeSeries = [],
                OutputStates = []
            };
            for (var i = 0; i < outputTimeSeriesSelection.Count; i++) {
                // Trim concentration brackets. The nature of the output (i.e. concentration
                // or amount) should be known by the calling function.
                var idOutput = outputTimeSeriesSelection[i].Trim('[', ']');
                result.OutputTimeSeries[idOutput] = [.. output[i + 1]];
            }

            // Get selected output states at end of simulation
            foreach (var item in outputStatesSelection) {
                result.OutputStates.Add(item, (double)_model[item]);
            }
            return result;
        }

        private bool setExposureEvents(
            Dictionary<ExposureRoute, string> routeInputMappings,
            List<IExposureEvent> exposureEvents,
            bool applyBodyweightScaling
        ) {
            var changed = false;
            var bodyweightMultiplication = applyBodyweightScaling
                ? $"*{_idBodyWeightParameter}" : string.Empty;

            static string getFingerprint(IExposureEvent e) {
                return e is SingleExposureEvent
                    ? $"S_{e.Route}_{e.Time.ToString(CultureInfo.InvariantCulture)}"
                    : $"R_{e.Route}_{e.Time.ToString(CultureInfo.InvariantCulture)}";
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
                var speciesId = routeInputMappings[exposureEvent.Route];

                string trigger;
                if (exposureEvent.GetType() == typeof(SingleExposureEvent)) {
                    var singleEvent = (SingleExposureEvent)exposureEvent;
                    trigger = $"time > {singleEvent.Time.ToString(CultureInfo.InvariantCulture)}";
                } else {
                    var repetitiveEvent = (RepeatingExposureEvent)exposureEvent;
                    trigger = $"time % {repetitiveEvent.Interval.ToString(CultureInfo.InvariantCulture)} == 0";
                }
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
                    var value = exposureEvent.Value.ToString(CultureInfo.InvariantCulture);
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

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _gil?.Dispose();
                if (PythonEngine.IsInitialized) {
                    //PythonEngine.Shutdown();
                }
            }
            _disposed = true;
        }
    }
}
