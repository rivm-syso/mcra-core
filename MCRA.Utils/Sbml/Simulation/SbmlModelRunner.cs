using System.Globalization;
using Python.Runtime;

namespace MCRA.Utils.Sbml.Simulation {
    public class SbmlModelRunner {

        public static bool ReuseEvents = true;

        private readonly dynamic _rr = null;
        private readonly Dictionary<string, string> _eventCache = [];

        private dynamic _model = null;

        public SbmlModelRunner() {
            // Initialize python and acquire GIL for the lifetime of the runner
            if (!PythonEngine.IsInitialized) {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
            using (var gil = Py.GIL()) {
                _rr = Py.Import("roadrunner");
            }
        }

        public void LoadModelFromFile(string filename) {
            using (var gil = Py.GIL()) {
                _model = _rr.RoadRunner(filename);
            }
        }

        public void LoadModelFromXmlString(string xml) {
            using (var gil = Py.GIL()) {
                _model = _rr.RoadRunner(xml);
            }
        }

        private void reset() {
            _model.resetToOrigin();
            _model.resetAll();
            _model.integrator.maximum_num_steps = 1000000;
        }

        public SbmlSimulationResult Run(
            List<SbmlSimulationEvent> events,
            Dictionary<string, double> parameters,
            List<string> outputStatesSelection,
            List<string> outputTimeSeriesSelection,
            int evaluationPeriod,
            int steps
        ) {
            using (var gil = Py.GIL()) {
                // (Re)initialize model
                reset();

                // Create model exposure events
                var eventsChanged = setEvents(events);

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
                var result = new SbmlSimulationResult() {
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
        }

        private bool setEvents(List<SbmlSimulationEvent> exposureEvents) {
            var changed = false;

            var clearEvents = !ReuseEvents;
            if (ReuseEvents && _eventCache != null) {
                // Check if events match cache
                var eventFingerprints = exposureEvents.Select(r => r.Id).ToHashSet();
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
            foreach (var events in exposureEvents) {
                string trigger;
                if (events.Kind == SbmlSimulationEventType.Single) {
                    trigger = $"time > {events.Time.ToString(CultureInfo.InvariantCulture)}";
                } else if (events.Kind == SbmlSimulationEventType.Repeated) {
                    trigger = $"time % {events.Interval.ToString(CultureInfo.InvariantCulture)} == 0";
                } else {
                    throw new NotImplementedException();
                }
                if (ReuseEvents) {
                    var eventParamId = string.Empty;
                    // Try to reuse events. SUCCESS: No recompilation.
                    // Instant C++ backend update via __setattr__
                    var fingerprint = events.Id;
                    if (_eventCache.TryGetValue(fingerprint, out var eventId)) {
                        // Set dose parameter value of the event
                        // --- INDIVIDUAL 2+ (Or Cached Path) ---
                        eventParamId = $"dose_{eventId}";
                    } else {
                        // Event not found; create event and add to cache
                        eventId = $"ev_{eidCounter++}";
                        eventParamId = $"dose_{eventId}";
                        var assignment = !string.IsNullOrEmpty(events.Adjustment)
                            ? $"{events.Target} + {eventParamId}{events.Adjustment}"
                            : $"{events.Target} + {eventParamId}";
                        // Set up model structure without intermediate recompilation (forceRegenerate = false)
                        _model.addParameter(eventParamId, 0, false);
                        _model.setConstant(eventParamId, false);
                        _model.addEvent(eventId, false, trigger, false);
                        _model.addEventAssignment(eventId, events.Target, assignment, false);
                        _eventCache.Add(fingerprint, eventId);
                        changed = true;
                    }
                    // CRITICAL: Explicitly set the initial value metadata so it survives the reset lifecycle
                    _model.setValue($"init({eventParamId})", events.Value);
                    // Set the live tracking variable value for immediate time execution
                    _model.__setattr__(eventParamId, events.Value);
                } else {
                    // Legacy fallback: Hardcodes the string value, forcing a recompile every single time
                    var value = events.Value.ToString(CultureInfo.InvariantCulture);
                    var assignment = !string.IsNullOrEmpty(events.Adjustment)
                        ? $"{events.Target} + {value}{events.Adjustment}"
                        : $"{events.Target} + {value}";
                    var eventId = $"ev_{eidCounter++}";
                    _model.addEvent(eventId, false, trigger, false);
                    _model.addEventAssignment(eventId, events.Target, assignment, false);
                    changed = true;
                }
            }
            // Events changed or newly created
            return changed;
        }
    }
}
