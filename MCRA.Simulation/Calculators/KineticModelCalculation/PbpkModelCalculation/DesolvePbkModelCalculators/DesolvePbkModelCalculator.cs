using System.Globalization;
using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators {
    public abstract class DesolvePbkModelCalculator : PbkModelCalculatorBase {

        private readonly string _dllFileName;
        private readonly string _modelFilePath;
        private readonly List<KineticModelStateSubstanceDefinition> _modelStates;
        private readonly List<(string idSpecies, KineticModelOutputDefinition outputDefinition)> _modelOutputs;

        public bool DebugMode { get; set; } = false;
        public Func<RDotNetEngine> CreateREngine = () => new RDotNetEngine();

        public DesolvePbkModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
            _dllFileName = Path.GetFileNameWithoutExtension(KineticModelDefinition.FileName);
            _modelFilePath = getModelFilePath();
            _modelStates = KineticModelDefinition.States
                .SelectMany(c =>
                    c.StateSubstances?.Count > 0
                        ? c.StateSubstances
                        : [new KineticModelStateSubstanceDefinition() {
                            Id = c.Id,
                            IdSubstance = c.IdSubstance,
                            Order = c.Order ?? -1,
                        }]
                )
                .OrderBy(r => r.Order)
                .ToList();
            _modelOutputs = KineticModelDefinition.Outputs
                .OrderBy(c => c.Order)
                .SelectMany(r => r.Species?.Count > 0
                    ? r.Species.Select(s => (id: s.IdSpecies, r))
                    : [(id: r.Id, r)]
                )
                .ToList();

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
        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            progressState.Update("Starting PBK model simulation");

            // Determine modelled/unmodelled exposure routes
            var modelExposureRoutes = KineticModelInstance.KineticModelDefinition.GetExposureRoutes();

            // Get time resolution
            var timeUnitMultiplier = (int)TimeUnit.Days.GetTimeUnitMultiplier(KineticModelDefinition.Resolution);
            var numberOfSimulatedDays = SimulationSettings.NumberOfSimulatedDays;
            var duration = timeUnitMultiplier * numberOfSimulatedDays;
            var stepsPerDay = getSimulationStepsPerDay();
            var stepLength = (1d / stepsPerDay) * timeUnitMultiplier;

            var nominalInputParameters = getNominalParameter();

            // Get integrator
            var integrator = string.Empty;
            if (KineticModelDefinition.IdIntegrator != null) {
                if (KineticModelDefinition.IdIntegrator.StartsWith("rk")) {
                    integrator = $", method = rkMethod('{KineticModelDefinition.IdIntegrator}') ";
                } else {
                    integrator = $", method = '{KineticModelDefinition.IdIntegrator}' ";
                }
            }

            // Initialise exposure events generator
            var exposureEventsGenerator = new ExposureEventsGenerator(
                SimulationSettings,
                KineticModelDefinition.Resolution,
                exposureUnit,
                KineticModelDefinition.Forcings
                    .ToDictionary(
                        r => r.Route,
                        r => r.DoseUnit
                    )
            );

            // Get kinetic model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            using (var R = CreateREngine()) {
                R.LoadLibrary("deSolve", null, true);
                R.EvaluateNoReturn($"dyn.load(paste('{_modelFilePath}', .Platform$dynlib.ext, sep = ''))");
                try {
                    R.EvaluateNoReturn($"times <- seq(from=0, to={duration}, by={stepLength.ToString(CultureInfo.InvariantCulture)})");
                    foreach (var id in externalIndividualExposures.Keys) {
                        var individual = externalIndividualExposures[id].First().SimulatedIndividual;

                        // Create exposure events
                        var exposureEvents = exposureEventsGenerator
                            .CreateExposureEvents(
                                externalIndividualExposures[id],
                                routes,
                                Substance,
                                generator
                            );

                        // Compute event timings and doses per route and set in R
                        (var timings, var dosesPerRoute) = computeDoses(
                            modelExposureRoutes,
                            exposureEvents,
                            timeUnitMultiplier
                        );

                        var result = new List<SubstanceTargetExposurePattern>();
                        if (exposureEvents.Any()) {
                            // Use variability
                            var parameters = drawParameters(
                                nominalInputParameters,
                                generator,
                                isNominal,
                                SimulationSettings.UseParameterVariability
                            );
                            setPhysiologicalParameterValues(parameters, individual);
                            setStartingEvents(parameters);

                            // Create forcings
                            R.SetSymbol("events", timings);
                            foreach (var routeDoses in dosesPerRoute) {
                                R.SetSymbol("doses", routeDoses.Value);
                                R.EvaluateNoReturn(routeDoses.Key.ToString() + " <- cbind(events, doses)");
                            }
                            R.EvaluateNoReturn($"allDoses <- list({string.Join(",", modelExposureRoutes.Select(r => r.ToString()))})");

                            // Initialize ode parameters, states, and doses
                            R.SetSymbol("params", parameters.Values);
                            R.EvaluateNoReturn($"params <- .C('getParms', as.double(params), out=double(length(params)), as.integer(length(params)), PACKAGE='{_dllFileName}')$out");
                            R.SetSymbol("paramNames", parameters.Keys);
                            R.EvaluateNoReturn($"names(params) <- paramNames");
                            R.SetSymbol("states", Enumerable.Repeat(0d, _modelStates.Count));
                            R.SetSymbol("outputNames", _modelOutputs.Select(r => r.idSpecies));

                            // Call to ODE
                            var cmd = "output <- ode(y = states, times = times, func = 'derivs', parms = params, " +
                                "dllname = '" + _dllFileName + "', initfunc = 'initmod', initforc = 'initforc', " +
                                "forcings = allDoses, events = list(func = 'event', time = events), " +
                                "nout = length(outputNames), outnames = outputNames" + integrator + ")";
                            R.EvaluateNoReturn(cmd);

                            // When in debug mode, also set state names
                            if (DebugMode) {
                                R.SetSymbol("stateNames", _modelStates.Select(r => r.Id));
                                R.EvaluateNoReturn("colnames(output) <- c(\"time\", stateNames, outputNames)");
                            }

                            // Store results of selected compartments/species
                            foreach (var sc in outputMappings) {
                                var output = R.EvaluateNumericVector($"output[,'{sc.SpeciesId}']");
                                List<TargetExposurePerTimeUnit> exposures = null;

                                // Compartment volume/mass
                                var bodyWeight = individual.BodyWeight;
                                var compartmentSize = !string.IsNullOrEmpty(sc.OutputDefinition.CompartmentSizeParameter)
                                    ? R.EvaluateDouble($"params['{sc.OutputDefinition.CompartmentSizeParameter}']")
                                    : getRelativeCompartmentWeight(sc, parameters) * bodyWeight;

                                // If output is cumulative amount, then...
                                if (sc.OutputType == KineticModelOutputType.CumulativeAmount) {
                                    //The cumulative amounts are reverted to differences between timepoints
                                    //(according to the specified resolution, in general hours).
                                    var runningSum = 0D;
                                    exposures = output
                                        .Select((r, i) => {
                                            var alignmentFactor = sc.GetUnitAlignmentFactor(compartmentSize);
                                            var exposure = alignmentFactor * r - runningSum;
                                            runningSum += exposure;
                                            return new TargetExposurePerTimeUnit(i * stepLength, exposure);
                                        })
                                        .ToList();
                                } else {
                                    exposures = output
                                        .Select((r, i) => {
                                            var alignmentFactor = sc.GetUnitAlignmentFactor(compartmentSize);
                                            var result = new TargetExposurePerTimeUnit(i * stepLength, r * alignmentFactor);
                                            return result;
                                        })
                                        .ToList();
                                }

                                result.Add(new SubstanceTargetExposurePattern() {
                                    Substance = sc.Substance,
                                    Target = sc.TargetUnit.Target,
                                    Compartment = sc.CompartmentId,
                                    RelativeCompartmentWeight = compartmentSize / bodyWeight,
                                    ExposureType = exposureType,
                                    TargetExposuresPerTimeUnit = exposures ?? [],
                                    NonStationaryPeriod = SimulationSettings.NonStationaryPeriod,
                                    TimeUnitMultiplier = timeUnitMultiplier
                                });
                            }
                        } else {
                            // No positive exposure: all zero!
                            foreach (var sc in outputMappings) {
                                result.Add(new SubstanceTargetExposurePattern() {
                                    Substance = sc.Substance,
                                    Target = sc.TargetUnit.Target,
                                    Compartment = sc.CompartmentId,
                                    RelativeCompartmentWeight = double.NegativeInfinity,
                                    ExposureType = exposureType,
                                    TargetExposuresPerTimeUnit = [],
                                    NonStationaryPeriod = SimulationSettings.NonStationaryPeriod,
                                    TimeUnitMultiplier = timeUnitMultiplier
                                });
                            }
                        }
                        individualResults[id] = result;
                    }
                } finally {
                    R.EvaluateNoReturn($"dyn.unload(paste('{getModelFilePath()}', .Platform$dynlib.ext, sep = ''))");
                }
            }
            progressState.Update("PBK model simulation finished", 100);
            return individualResults;
        }

        private Dictionary<string, KineticModelInstanceParameter> getNominalParameter() {
            // Get nominal input parameters
            var nominalInputParametersOrder = KineticModelDefinition.Parameters
                .Where(r => !r.SubstanceParameterValues.Any())
                .ToDictionary(
                    r => r.Order,
                    r => {
                        if (KineticModelInstance.KineticModelInstanceParameters.TryGetValue(r.Id, out var parameter)) {
                            return parameter;
                        } else if (!r.IsInternalParameter) {
                            throw new Exception($"Kinetic model parameter {r.Id} not found in model instance {KineticModelInstance.IdModelInstance}");
                        } else {
                            return new KineticModelInstanceParameter() { Parameter = r.Id, Value = 0D };
                        }
                    }
                );

            // Get nominal input parameters for parent and metabolites
            var substanceParameterValuesOrder = KineticModelDefinition.Parameters
                .Where(r => r.SubstanceParameterValues.Any())
                .SelectMany(c => c.SubstanceParameterValues)
                .ToDictionary(c => c.Order,
                    c => {
                        if (KineticModelInstance.KineticModelInstanceParameters.TryGetValue(c.IdParameter, out var parameter)) {
                            return parameter;
                        } else {
                            throw new Exception($"Kinetic model parameter {c.IdParameter} not found in model instance {KineticModelInstance.IdModelInstance}");
                        }
                    }
                );

            // Combine dictionaries
            substanceParameterValuesOrder.ToList().ForEach(x => nominalInputParametersOrder[x.Key] = x.Value);
            var nominalInputParameters = nominalInputParametersOrder
                .OrderBy(c => c.Key)
                .ToDictionary(c => c.Value.Parameter, c => c.Value);
            return nominalInputParameters;
        }

        protected virtual (List<double> timings, Dictionary<ExposureRoute, List<double>> dosesPerRoute) computeDoses(
            ICollection<ExposureRoute> modelExposureRoutes,
            List<IExposureEvent> exposureEvents,
            int timeUnitMultiplier
        ) {
            // Convert to repating exposure events to single events
            var singleExposureEvents = exposureEvents
                .SelectMany(r => (r is SingleExposureEvent)
                    ? [r as SingleExposureEvent]
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
            TargetOutputMapping outputMapping,
            IDictionary<string, double> parameters
        ) {
            var factor = 1D;
            foreach (var scalingFactor in outputMapping.OutputDefinition.ScalingFactors) {
                factor *= parameters[scalingFactor];
            }
            foreach (var multiplicationFactor in outputMapping.OutputDefinition.MultiplicationFactors) {
                factor *= multiplicationFactor;
            }
            return factor;
        }
    }
}
