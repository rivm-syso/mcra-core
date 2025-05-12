using System.Globalization;
using ExCSS;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent;
using Python.Runtime;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation {
    public class SbmlModelRunner : IDisposable {

        private bool _disposed = false;

        private readonly string _modelFileName;
        private readonly Dictionary<string, double> _defaultParameters;
        private readonly Dictionary<ExposureRoute, string> _modelInputs;
        private readonly List<TargetOutputMapping> _targetOutputMappings;

        private dynamic _rr = null;
        private dynamic _model = null;

        public SbmlModelRunner(
            KineticModelInstance modelInstance,
            List<TargetOutputMapping> targetOutputMappings
        ) {
            var modelDefinition = modelInstance.KineticModelDefinition;
            _modelInputs = modelDefinition.Forcings.ToDictionary(r => r.Route, r => r.Id);
            _modelFileName = modelInstance.KineticModelDefinition.FileName;
            _targetOutputMappings = targetOutputMappings;
            _defaultParameters = [];
            foreach (var parameterDefinition in modelDefinition.Parameters) {
                if (modelInstance.KineticModelInstanceParameters.TryGetValue(parameterDefinition.Id, out var parameter)) {
                    _defaultParameters[parameterDefinition.Id] = parameter.Value;
                } else {
                    //throw new Exception($"Kinetic model parameter {parameterDefinition.Id} not found in model instance [{ModelInstance.IdModelInstance}].");
                }
            }

            // Initialize python
            if (!PythonEngine.IsInitialized) {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        ~SbmlModelRunner() {
            Dispose(false);
        }

        private void initializeModel() {
            // Import roadrunner and read model
            if (_model == null) {
                // Load model
                _rr = Py.Import("roadrunner");
                _model = _rr.RoadRunner(_modelFileName);

                // Set boundary condition for inputs (for discrete/bolus dosing events)
                foreach (var input in _modelInputs.Values) {
                    _model.setInitAmount(input, 0);
                    _model.setConstant(input, false);
                    _model.setBoundary(input, false);
                }
            } else {
                // TODO: find a way to really reset the model (including deletion of events).
                // The resetToOrigin/resetAll methods do not seem to work.
                _model.resetAll();
                _model.resetToOrigin();
                _model.integrator.maximum_num_steps = 1000000;
                // Remove events
                var eventIds = _model.model.getEventIds();
                foreach (var eventId in eventIds) {
                    _model.removeEvent(eventId, false);
                }
            }
        }

        public SimulationOutput Run(
            List<IExposureEvent> exposureEvents,
            Dictionary<string, double> parameters,
            int evaluationPeriod,
            int steps,
            bool applyBodyweightScaling,
            string idBodyweightParameter
        ) {
            using (Py.GIL()) {
                initializeModel();

                // Set default model parameters from instance
                foreach (var parameter in _defaultParameters) {
                    _model.__setattr__(parameter.Key, parameter.Value);
                }

                // Set model parameters
                if (parameters != null) {
                    foreach (var parameter in parameters) {
                        _model.__setattr__(parameter.Key, parameter.Value);
                    }
                }
                setExposuresEvents(exposureEvents, applyBodyweightScaling, idBodyweightParameter);

                // Regenerate model
                _model.regenerateModel(true, true);

                // Determine total evaluation period and output steps and run the model
                var simulationOutput = _model.simulate(0, evaluationPeriod, steps);

                // Get output compartment (trim brackets that are added by roadrunner)
                var outputNames = new List<string>(simulationOutput.colnames.As<string[]>())
                    .Skip(1)
                    .Select(r => r.Trim('[', ']'))
                    .ToList();

                // Get output data and store in result
                var output = (double[][])simulationOutput.T.As<double[][]>();
                var result = new SimulationOutput() {
                    Time = output[0],
                    OutputTimeSeries = [],
                    CompartmentVolumes = []
                };
                for (var i = 0; i < output.Length - 1; i++) {
                    result.OutputTimeSeries[outputNames[i]] = output[i + 1].ToList();
                }

                foreach (var mapping in _targetOutputMappings) {
                    result.CompartmentVolumes.Add(mapping.CompartmentId, (double)_model[mapping.CompartmentId]);
                }

                return result;
            }
        }

        private void setExposuresEvents(
            List<IExposureEvent> exposureEvents,
            bool applyBodyweightScaling,
            string idBodyweightParameter
        ) {
            // Set exposure events
            var eidCounter = 1;
            var bodyweightMultiplication = applyBodyweightScaling
                ? $"*{idBodyweightParameter}" : string.Empty;
            foreach (var exposureEvent in exposureEvents) {
                // Create an event for each exposure event
                if (exposureEvent.GetType() == typeof(SingleExposureEvent)) {
                    var singleEvent = (SingleExposureEvent)exposureEvent;
                    var speciesId = _modelInputs[exposureEvent.Route];
                    var eid = $"ev_{eidCounter++}";
                    _model.addEvent(
                        eid,
                    false,
                        $"time > {singleEvent.Time.ToString(CultureInfo.InvariantCulture)}",
                        false
                    );
                    _model.addEventAssignment(
                        eid,
                        speciesId,
                        $"{speciesId} + {singleEvent.Value.ToString(CultureInfo.InvariantCulture)}{bodyweightMultiplication}",
                        false
                    );
                } else {
                    var repetitiveEvent = (RepeatingExposureEvent)exposureEvent;
                    var speciesId = _modelInputs[exposureEvent.Route];
                    var eid = $"ev_{eidCounter++}";
                    _model.addEvent(
                        eid,
                        false,
                        $"time % {repetitiveEvent.Interval.ToString(CultureInfo.InvariantCulture)} == 0",
                        false
                    );
                    _model.addEventAssignment(
                        eid,
                        speciesId,
                        $"{speciesId} + {repetitiveEvent.Value.ToString(CultureInfo.InvariantCulture)}{bodyweightMultiplication}",
                        false
                    );
                }
            }
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
                if (PythonEngine.IsInitialized) {
                    //PythonEngine.Shutdown();
                }
            }
            _disposed = true;
        }
    }
}