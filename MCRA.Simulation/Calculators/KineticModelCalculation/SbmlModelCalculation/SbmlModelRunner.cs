using System.Globalization;
using System.Security.Policy;
using System.Text;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Python.Runtime;
using static Python.Runtime.Py;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation {
    public class SbmlModelRunner : IDisposable {

        private bool _disposed = false;

        private readonly string _modelFileName;
        private readonly Dictionary<string, double> _defaultParameters;
        private readonly Dictionary<ExposurePathType, string> _modelInputs;

        private readonly GILState _gil = null;
        private dynamic _model = null;

        public SbmlModelRunner(
            KineticModelInstance modelInstance
        ) {
            var modelDefinition = modelInstance.KineticModelDefinition;
            _modelInputs = modelDefinition.Forcings.ToDictionary(r => r.Route, r => r.Id);
            _modelFileName = SbmlPbkModelCalculator.GetModelFilePath(modelInstance.KineticModelDefinition.FileName);
            _defaultParameters = new();
            foreach (var parameterDefinition in modelDefinition.Parameters) {
                if (modelInstance.KineticModelInstanceParameters.TryGetValue(parameterDefinition.Id, out var parameter)) {
                    _defaultParameters[parameterDefinition.Id] = parameter.Value;
                } else {
                    //throw new Exception($"Kinetic model parameter {parameterDefinition.Id} not found in model instance [{ModelInstance.IdModelInstance}].");
                }
            }

            // Initialize python
            PythonEngine.Initialize();
            _gil = GIL();

            initializeModel();
        }

        private void initializeModel() {
            // Import roadrunner and read model
            if (_model == null || true) {
                // Load model
                dynamic rr = Import("roadrunner");
                _model = rr.RoadRunner(_modelFileName);
            } else {
                // TODO: find a way to reset the model instead of reloading the model
                // (including deletion of events)

                // From documentation: It is also possible to load a model from a string containing a SBML model, for example:
                // rr = roadrunner.RoadRunner(sbmlStr)
                // This is useful when one wishes to create a new roadrunner instance from an existing model, eg:
                // sbmlStr = rr.getCurrentSBML() rrnew = roadrunner.RoadRunner(sbmlStr)
                // Is slightly faster, probably because the sbmlStr is dynamic

                // Regenerate model
                _model.resetToOrigin();

                // Remove events
                var eventIds = _model.getEventIds();
                foreach (var eventId in eventIds) {
                    _model.removeEvent(eventId, false);
                }

                // Regenerate model
                _model.regenerateModel(true, true);
            }

            // Set boundary condition for inputs (for discrete/bolus dosing events)
            foreach (var input in _modelInputs.Values) {
                _model.setInitAmount(input, 0);
                _model.setConstant(input, false);
                _model.setBoundary(input, false);
            }

            // Set model parameters
            foreach (var parameter in _defaultParameters) {
                _model.__setattr__(parameter.Key, parameter.Value);
            }
        }

        ~SbmlModelRunner() {
            Dispose(disposing: false);
        }

        public SimulationOutput Run(
            List<SimulationInput> exposureEvents,
            Dictionary<string, double> parameters,
            int evaluationPeriod,
            int steps
        ) {

            initializeModel();

            // Set model parameters
            if (parameters != null) {
                foreach (var parameter in parameters) {
                    _model.__setattr__(parameter.Key, parameter.Value);
                }
            }

            // Set exposure events
            var eidCounter = 1;
            foreach (var exposure in exposureEvents) {
                // Create an event for each exposure event
                foreach (var (time, value) in exposure.Events) {
                    var speciesId = _modelInputs[exposure.Route];
                    var eid = $"ev_{eidCounter++}";
                    _model.addEvent(
                        eid,
                        false,
                        $"time > {(time).ToString(CultureInfo.InvariantCulture)}",
                        false
                    );
                    _model.addEventAssignment(
                        eid,
                        speciesId,
                        $"{speciesId} + {value.ToString(CultureInfo.InvariantCulture)}",
                        false
                    );
                }
            }

            // Regenerate model
            _model.regenerateModel(true, true);
            //TODO werkt niet Waldo, rk4 does not support events
            //_model.setIntegrator("rk4");

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
                OutputTimeSeries = new Dictionary<string, List<double>>()
            };
            for (int i = 0; i < output.Length - 1; i++) {
                result.OutputTimeSeries[outputNames[i]] = output[i + 1].ToList();
            }

            return result;
        }
        public double[] GetCompartmentVolumes(params string[] compartmentIds) {
            var result = new double[compartmentIds.Length];
            for (int i = 0; i < compartmentIds.Length; i++) {
                var vol_compartment = $"V{compartmentIds[i].Remove(0, 1)}";
                result[i] = _model[vol_compartment];
            }
            return result;
        }
        public double GetCompartmentVolume(string compartmentIds) {
            return _model[compartmentIds];
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _gil.Dispose();
                PythonEngine.Shutdown();
                _disposed = true;
            }
        }
    }
}