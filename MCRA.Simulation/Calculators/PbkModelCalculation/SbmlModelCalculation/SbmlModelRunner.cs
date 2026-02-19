using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration;
using Python.Runtime;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation {
    public class SbmlModelRunner : IDisposable {

        private bool _disposed = false;

        private readonly string _modelFileName;
        private readonly string _idBodyWeightParameter;
        private readonly IPbkModelSpecification _pbkModelSpecification;
        private readonly Py.GILState _gil;

        private dynamic _rr = null;
        private dynamic _model = null;

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
            Dictionary<ExposureRoute, string> routeInputMappings,
            List<string> outputStatesSelection,
            List<string> outputTimeSeriesSelection,
            int evaluationPeriod,
            int steps,
            bool applyBodyWeightScaling
        ) {
            // (Re)initialize model
            initializeModel(routeInputMappings);

            // Set model parameters
            if (parameters != null) {
                foreach (var parameter in parameters) {
                    _model.__setattr__(parameter.Key, parameter.Value);
                }
            }


            // Create model exposure events
            setExposuresEvents(
                routeInputMappings,
                exposureEvents,
                applyBodyWeightScaling
            );

            // Regenerate model
            _model.regenerateModel(true, true);

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

        private void setExposuresEvents(
            Dictionary<ExposureRoute, string> routeInputMappings,
            List<IExposureEvent> exposureEvents,
            bool applyBodyweightScaling
        ) {
            // Set exposure events
            var eidCounter = 1;
            var bodyweightMultiplication = applyBodyweightScaling
                ? $"*{_idBodyWeightParameter}" : string.Empty;
            foreach (var exposureEvent in exposureEvents) {
                // Create an event for each exposure event
                if (exposureEvent.GetType() == typeof(SingleExposureEvent)) {
                    var singleEvent = (SingleExposureEvent)exposureEvent;
                    var speciesId = routeInputMappings[exposureEvent.Route];
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
                    var speciesId = routeInputMappings[exposureEvent.Route];
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
                _gil?.Dispose();
                if (PythonEngine.IsInitialized) {
                    //PythonEngine.Shutdown();
                }
            }
            _disposed = true;
        }
    }
}
