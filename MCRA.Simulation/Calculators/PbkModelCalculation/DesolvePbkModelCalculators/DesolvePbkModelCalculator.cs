using System.Globalization;
using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration;
using MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators {
    public abstract class DesolvePbkModelCalculator : PbkModelCalculatorBase<DeSolvePbkModelSpecification> {

        private readonly string _dllFileName;
        private readonly string _modelFilePath;

        public bool DebugMode { get; set; } = false;
        public Func<RDotNetEngine> CreateREngine = () => new RDotNetEngine();

        public DesolvePbkModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
            _dllFileName = Path.GetFileNameWithoutExtension(PbkModelSpecification.FileName);
            _modelFilePath = getModelFilePath();
            if (simulationSettings.PbkSimulationMethod != PbkSimulationMethod.Standard) {
                throw new NotImplementedException($"PBK simulation method [{simulationSettings.PbkSimulationMethod}] not implemented for DeSolve PBK models.");
            }
            if (simulationSettings.BodyWeightCorrected) {
                throw new NotImplementedException($"Bodyweight corrected exposure events not implemented for DeSolve PBK models.");
            }
        }

        /// <summary>
        /// Computes target organ exposures for the given collections of individual day exposures.
        /// </summary>
        public override List<PbkSimulationOutput> calculate(
            ICollection<(SimulatedIndividual Individual, List<IExternalIndividualDayExposure> IndividualDayExposures)> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            IRandom generator
        ) {
            // Get time resolution
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(PbkModelSpecification.Resolution);
            var numberOfSimulatedDays = SimulationSettings.NumberOfSimulatedDays;
            var duration = timeUnitMultiplier * numberOfSimulatedDays;
            var stepsPerDay = getSimulationStepsPerDay();
            var stepLength = 1d / stepsPerDay * timeUnitMultiplier;

            // Get integrator
            var integrator = string.Empty;
            if (PbkModelSpecification.IdIntegrator != null) {
                if (PbkModelSpecification.IdIntegrator.StartsWith("rk")) {
                    integrator = $", method = rkMethod('{PbkModelSpecification.IdIntegrator}') ";
                } else {
                    integrator = $", method = '{PbkModelSpecification.IdIntegrator}' ";
                }
            }

            // Get nominal parameter values
            var nominalParameterValues = getNominalParameterValues();

            // Get model inputs
            var modelInputs = PbkModelSpecification.Forcings;

            // Get model inputs/exposure routes in order of specification to match deSolve forcings
            var modelInputRoutes = modelInputs
                .OrderBy(r => r.Order)
                .Select(c => c.Route)
                .ToList();

            // Check if routes are supported by the model
            var missingRoutes = routes.Except(modelInputRoutes).ToList();
            if (missingRoutes.Count > 0) {
                var routeNames = string.Join(", ", missingRoutes.Select(r => r.GetDisplayName()));
                throw new Exception($"Exposure routes {routeNames} are not supported by PBK model {PbkModelSpecification.Id}.");
            }

            // Initialise exposure events generator
            var exposureEventsGenerator = new ExposureEventsGenerator(
                SimulationSettings,
                PbkModelSpecification.Resolution,
                exposureUnit,
                modelInputs
                    .ToDictionary(
                        r => r.Route,
                        r => r.DoseUnit.GetSubstanceAmountUnit()
                    )
            );

            // Get model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            // Get model states in order of specification to match deSolve states
            var modelStates = PbkModelSpecification.GetStates();

            // Get model outputs in order of specification to match deSolve outputs
            var modelOutputs = PbkModelSpecification.Outputs
                .OrderBy(c => c.Order)
                .ToList();

            // Create lookup for model outputs by id
            var outputDefinitionsLookup = PbkModelSpecification.Outputs
                .ToDictionary(r => r.Id);

            var results = new List<PbkSimulationOutput>();
            using (var R = CreateREngine()) {
                R.LoadLibrary("deSolve", null, true);
                R.EvaluateNoReturn($"dyn.load(paste('{_modelFilePath}', .Platform$dynlib.ext, sep = ''))");
                try {
                    R.EvaluateNoReturn($"times <- seq(from=0, to={duration}, by={stepLength.ToString(CultureInfo.InvariantCulture)})");
                    foreach (var id in externalIndividualExposures) {
                        var individual = id.Individual;

                        // Create exposure events
                        var exposureEvents = exposureEventsGenerator
                            .CreateExposureEvents(
                                id.IndividualDayExposures,
                                routes,
                                Substance,
                                generator
                            );

                        // Compute event timings and doses per route and set in R
                        (var eventTimings, var dosesPerRoute) = computeDoses(
                            modelInputRoutes,
                            exposureEvents,
                            timeUnitMultiplier
                        );

                        SimulationOutput simulationOutput = null;

                        // If we have positive exposures, then run simulation
                        if (exposureEvents.Count > 0) {
                            var parameters = drawParameters(
                                nominalParameterValues,
                                generator,
                                SimulationSettings.UseParameterVariability
                            );
                            setPhysiologicalParameterValues(parameters, individual);
                            setStartingEvents(parameters);

                            // Create forcings
                            R.SetSymbol("events", eventTimings);
                            foreach (var routeDoses in dosesPerRoute) {
                                R.SetSymbol("doses", routeDoses.Value);
                                R.EvaluateNoReturn(routeDoses.Key.ToString() + " <- cbind(events, doses)");
                            }
                            R.EvaluateNoReturn($"allDoses <- list({string.Join(",", modelInputRoutes.Select(r => r.ToString()))})");

                            // Initialize ode parameters, states, and doses
                            R.SetSymbol("params", parameters.Values);
                            R.EvaluateNoReturn($"params <- .C('getParms', as.double(params), out=double(length(params)), as.integer(length(params)), PACKAGE='{_dllFileName}')$out");
                            R.SetSymbol("paramNames", parameters.Keys);
                            R.EvaluateNoReturn($"names(params) <- paramNames");
                            R.SetSymbol("states", Enumerable.Repeat(0d, modelStates.Count));
                            R.SetSymbol("outputNames", modelOutputs.Select(r => r.Id));

                            // Call to ODE
                            var cmd = "output <- ode(y = states, times = times, func = 'derivs', parms = params, " +
                                "dllname = '" + _dllFileName + "', initfunc = 'initmod', initforc = 'initforc', " +
                                "forcings = allDoses, events = list(func = 'event', time = events), " +
                                "nout = length(outputNames), outnames = outputNames" + integrator + ")";
                            R.EvaluateNoReturn(cmd);

                            // When in debug mode, also set state names
                            if (DebugMode) {
                                R.SetSymbol("stateNames", modelStates.Select(r => r.Id));
                                R.EvaluateNoReturn("colnames(output) <- c(\"time\", stateNames, outputNames)");
                            }

                            simulationOutput = new SimulationOutput() {
                                Time = [.. R.EvaluateNumericVector("times")],
                                OutputTimeSeries = [],
                                OutputStates = [],
                            };

                            // Collect the compartment sizes as output states
                            foreach (var sc in outputMappings) {
                                var outputDefinition = outputDefinitionsLookup[sc.OutputId];
                                var compartmentSize = !string.IsNullOrEmpty(outputDefinition.CompartmentSizeParameter)
                                    ? R.EvaluateDouble($"params['{outputDefinition.CompartmentSizeParameter}']")
                                    : getRelativeCompartmentWeight(outputDefinition, parameters) * individual.BodyWeight;
                                simulationOutput.OutputStates[sc.CompartmentId] = compartmentSize;
                            }

                            // Collect the output timeseries
                            foreach (var modelOutput in modelOutputs) {
                                var series = R.EvaluateNumericArray($"output[,'{modelOutput.Id}']");
                                simulationOutput.OutputTimeSeries[modelOutput.Id] = series;
                            }
                        }

                        results.Add(
                            collectPbkSimulationResults(
                                outputMappings,
                                individual,
                                simulationOutput,
                                1D / stepsPerDay
                            )
                        );
                    }
                } finally {
                    R.EvaluateNoReturn($"dyn.unload(paste('{getModelFilePath()}', .Platform$dynlib.ext, sep = ''))");
                }
            }

            return results;
        }

        private Dictionary<string, KineticModelInstanceParameter> getNominalParameterValues() {
            // Get nominal input parameters
            var result = PbkModelSpecification
                .GetParameters()
                .OrderBy(c => c.Order)
                .ToDictionary(
                    r => r.Id,
                    r => {
                        if (KineticModelInstance.KineticModelInstanceParameters.TryGetValue(r.Id, out var parameter)) {
                            return parameter;
                        } else if (!r.IsInternalParameter) {
                            throw new Exception($"PBK model parameter {r.Id} not found in model instance {KineticModelInstance.IdModelInstance}");
                        } else {
                            return new KineticModelInstanceParameter() { Parameter = r.Id, Value = 0D };
                        }
                    }
                );
            return result;
        }

        protected virtual (List<double> timings, Dictionary<ExposureRoute, List<double>> dosesPerRoute) computeDoses(
            ICollection<ExposureRoute> modelExposureRoutes,
            List<IExposureEvent> exposureEvents,
            double timeUnitMultiplier
        ) {
            // Convert to repating exposure events to single events
            var singleExposureEvents = exposureEvents
                .SelectMany(r => (r is SingleExposureEvent singleExpEvt)
                    ? [singleExpEvt]
                    : (r as RepeatingExposureEvent).Expand(SimulationSettings.NumberOfSimulatedDays * timeUnitMultiplier)
                )
                .ToList();

            var exposureEventsGrouped = singleExposureEvents
                .GroupBy(r => r.Time)
                .OrderBy(r => r.Key);
            var timings = exposureEventsGrouped
                .Select(r => r.Key)
                .ToList();
            var dosesPerRoute = modelExposureRoutes
                .ToDictionary(
                    r => r,
                    r => Enumerable.Repeat(0D, timings.Count).ToList()
                );
            var groupIx = 0;
            foreach (var group in exposureEventsGrouped) {
                foreach (var record in group) {
                    dosesPerRoute[record.Route][groupIx] = record.Value;
                }
                groupIx++;
            }
            return (timings, dosesPerRoute);
        }

        /// <summary>
        /// Gets the dll or xml path of the kinetic model definition.
        /// </summary>
        protected string getModelFilePath() {
            var location = Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = new FileInfo(location).Directory.FullName;
            var dllPath = Path.Combine(assemblyFolder, "Resources", "KineticModels", _dllFileName);
            // Convert backslashes to / explicitly, path is used in R script
            return dllPath.Replace(@"\", "/");
        }

        protected virtual void setStartingEvents(
            IDictionary<string, double> parameters
        ) {
            // Default: nothing
        }

        protected virtual double getRelativeCompartmentWeight(
            DeSolvePbkModelOutputSpecification outputSpecification,
            IDictionary<string, double> parameters
        ) {
            var factor = 1D;
            foreach (var scalingFactor in outputSpecification.ScalingFactors) {
                factor *= parameters[scalingFactor];
            }
            foreach (var multiplicationFactor in outputSpecification.MultiplicationFactors) {
                factor *= multiplicationFactor;
            }
            return factor;
        }
    }
}
