using System.Globalization;
using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ParameterDistributionModels;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators {
    public abstract class DesolvePbkModelCalculator : PbkModelCalculatorBase {

        protected string _dllFileName;
        protected List<KineticModelStateSubstanceDefinition> _modelStates;
        protected List<(string idSpecies, KineticModelOutputDefinition outputDefinition)> _modelOutputs;

        public bool DebugMode { get; set; } = false;
        public Func<RDotNetEngine> CreateREngine = () => new RDotNetEngine();

        public DesolvePbkModelCalculator(
            KineticModelInstance kineticModelInstance,
            bool useRepeatedDailyEvents
        ) : base(kineticModelInstance, useRepeatedDailyEvents) {
            _dllFileName = Path.GetFileNameWithoutExtension(KineticModelDefinition.FileName);
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
        }

        /// <summary>
        /// Computes target organ exposures for the given collections of individual day exposures.
        /// </summary>
        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            ExposureUnitTriple externalExposureUnit,
            Compound substance,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            progressState.Update("PBK modelling started");

            // Determine modelled/unmodelled exposure routes
            var modelExposureRoutes = KineticModelInstance.KineticModelDefinition.GetExposureRoutes();

            // Get kinetic model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            // Get time resolution
            var stepLength = 1d / KineticModelDefinition.EvaluationFrequency;

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
                            throw new Exception($"Kinetic model parameter {c} not found in model instance {KineticModelInstance.IdModelInstance}");
                        }
                    }
                );

            // Combine dictionaries
            substanceParameterValuesOrder.ToList().ForEach(x => nominalInputParametersOrder[x.Key] = x.Value);
            var nominalInputParameters = nominalInputParametersOrder
                .OrderBy(c => c.Key)
                .ToDictionary(c => c.Value.Parameter, c => c.Value);

            // Adapt physiological parameters to current individual
            var standardBW = nominalInputParameters[KineticModelDefinition.IdBodyWeightParameter].Value;
            var standardBSA = double.NaN;
            if (KineticModelDefinition.IdBodySurfaceAreaParameter != null) {
                standardBSA = nominalInputParameters[KineticModelDefinition.IdBodySurfaceAreaParameter].Value;
            }

            // Get integrator
            var integrator = string.Empty;
            if (KineticModelDefinition.IdIntegrator != null) {
                if (KineticModelDefinition.IdIntegrator.StartsWith("rk")) {
                    integrator = $", method = rkMethod('{KineticModelDefinition.IdIntegrator}') ";
                } else {
                    integrator = $", method = '{KineticModelDefinition.IdIntegrator}' ";
                }
            }

            // Get events
            var eventsDictionary = UseRepeatedDailyEvents
                ? modelExposureRoutes
                    .ToDictionary(r => r, r => getRepeatedDailyEventTimings(
                       r, _timeUnitMultiplier, KineticModelInstance.NumberOfDays
                    ))
                : modelExposureRoutes
                    .ToDictionary(r => r, r => getEventTimings(
                        r, _timeUnitMultiplier, KineticModelInstance.NumberOfDays, KineticModelInstance.SpecifyEvents
                    ));

            var events = calculateCombinedEventTimings(eventsDictionary);
            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            using (var R = CreateREngine()) {
                R.LoadLibrary("deSolve", null, true);
                R.EvaluateNoReturn($"dyn.load(paste('{getModelFilePath()}', .Platform$dynlib.ext, sep = ''))");
                try {
                    R.SetSymbol("events", events);
                    R.EvaluateNoReturn($"times <- seq(from=0, to={_evaluationPeriod}, by={stepLength.ToString(CultureInfo.InvariantCulture)})");
                    foreach (var id in externalIndividualExposures.Keys) {
                        var boundedForcings = new List<string>();
                        var hasPositiveExposures = false;
                        // Create exposure events
                        foreach (var route in modelExposureRoutes) {
                            if (_modelInputDefinitions.TryGetValue(route, out var value)) {
                                var routeExposures = getRouteSubstanceIndividualDayExposures(
                                    externalIndividualExposures[id],
                                    substance,
                                    route
                                );
                                var substanceAmountUnitMultiplier = value.DoseUnit
                                    .GetSubstanceAmountUnit()
                                    .GetMultiplicationFactor(
                                        externalExposureUnit.SubstanceAmountUnit,
                                        substance.MolecularMass
                                    );
                                var dailyDoses = routeExposures
                                    .Select(r => r / substanceAmountUnitMultiplier)
                                    .ToList();
                                var unitDoses = UseRepeatedDailyEvents
                                    ? [dailyDoses.Average()]
                                    : getUnitDoses(nominalInputParameters, dailyDoses, route);
                                var simulatedDoses = drawSimulatedDoses(unitDoses, eventsDictionary[route].Count, generator);
                                var simulatedDosesExpanded = combineDosesWithEvents(events, eventsDictionary[route], simulatedDoses);
                                R.SetSymbol("doses", simulatedDosesExpanded);
                                R.EvaluateNoReturn(route.ToString() + " <- cbind(events, doses)");
                                boundedForcings.Add(route.ToString());
                                if (unitDoses.Any(c => c > 0)) {
                                    hasPositiveExposures = true;
                                }
                            }
                        }

                        var result = new List<SubstanceTargetExposurePattern>();

                        if (hasPositiveExposures) {
                            // Use variability
                            var inputParameters = drawParameters(
                                nominalInputParameters,
                                generator,
                                isNominal,
                                KineticModelInstance.UseParameterVariability
                            );
                            inputParameters = setStartingEvents(inputParameters);

                            // Set BW
                            var bodyWeight = externalIndividualExposures[id].First().Individual?.BodyWeight ?? standardBW;
                            inputParameters[KineticModelDefinition.IdBodyWeightParameter] = bodyWeight;

                            // Set BSA
                            if (KineticModelDefinition.IdBodySurfaceAreaParameter != null) {
                                var allometricScaling = Math.Pow(standardBW / bodyWeight, 1 - 0.7);
                                inputParameters[KineticModelDefinition.IdBodySurfaceAreaParameter] = standardBSA / allometricScaling;
                            }

                            // Set age
                            if (KineticModelDefinition.IdAgeParameter != null) {
                                inputParameters[KineticModelDefinition.IdAgeParameter] = getAge(
                                    externalIndividualExposures[id].First().Individual,
                                    _modelParameterDefinitions[KineticModelDefinition.IdAgeParameter].DefaultValue ?? double.NaN
                                );
                            }

                            // Set gender
                            if (KineticModelDefinition.IdSexParameter != null) {
                                inputParameters[KineticModelDefinition.IdSexParameter] = getGender(
                                    externalIndividualExposures[id].First().Individual,
                                    _modelParameterDefinitions[KineticModelDefinition.IdSexParameter].DefaultValue ?? double.NaN
                                );
                            }

                            // Initialize ode parameters, states, and doses
                            R.SetSymbol("params", inputParameters.Values);
                            R.EvaluateNoReturn($"params <- .C('getParms', as.double(params), out=double(length(params)), as.integer(length(params)), PACKAGE='{_dllFileName}')$out");
                            R.SetSymbol("paramNames", inputParameters.Keys);
                            R.EvaluateNoReturn($"names(params) <- paramNames");
                            R.SetSymbol("states", Enumerable.Repeat(0d, _modelStates.Count));
                            R.EvaluateNoReturn($"allDoses <- list({string.Join(",", boundedForcings)})");
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
                                var compartmentSize = !string.IsNullOrEmpty(sc.OutputDefinition.CompartmentSizeParameter)
                                    ? R.EvaluateDouble($"params['{sc.OutputDefinition.CompartmentSizeParameter}']")
                                    : getRelativeCompartmentWeight(sc, inputParameters) * bodyWeight;

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
                                    NonStationaryPeriod = KineticModelInstance.NonStationaryPeriod,
                                    TimeUnitMultiplier = _timeUnitMultiplier
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
                                    NonStationaryPeriod = KineticModelInstance.NonStationaryPeriod,
                                    TimeUnitMultiplier = _timeUnitMultiplier
                                });
                            }
                        }
                        individualResults[id] = result;
                    }
                } finally {
                    R.EvaluateNoReturn($"dyn.unload(paste('{getModelFilePath()}', .Platform$dynlib.ext, sep = ''))");
                }
            }
            progressState.Update("PBPK modelling finished");
            return individualResults;
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

        protected virtual List<int> calculateCombinedEventTimings(IDictionary<ExposureRoute, List<int>> eventsDictionary) {
            return eventsDictionary.SelectMany(c => c.Value).Distinct().Order().ToList();
        }

        protected virtual IDictionary<string, double> setStartingEvents(IDictionary<string, double> parameters) {
            return parameters;
        }

        /// <summary>
        /// Combines doses with relevant events for each exposure route, sets irrelevant events to zero
        /// </summary>
        /// <param name="allEvents"></param>
        /// <param name="events"></param>
        /// <param name="doses"></param>
        /// <returns></returns>
        protected virtual List<double> combineDosesWithEvents(List<int> allEvents, List<int> events, List<double> doses) {
            var dosesDict = allEvents.ToDictionary(r => r, r => 0D);
            for (var i = 0; i < events.Count; i++) {
                dosesDict[events[i]] = doses[i];
            }
            return dosesDict.Values.ToList();
        }

        /// <summary>
        /// Returns a draw of the parameter values or nominal values
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, double> drawParameters(IDictionary<string, KineticModelInstanceParameter> parameters, IRandom random, bool isNominal, bool useParameterVariability) {
            var drawn = new Dictionary<string, double>();
            if (isNominal || !useParameterVariability) {
                drawn = parameters.ToDictionary(c => c.Key, c => c.Value.Value);
            } else {
                foreach (var parameter in parameters) {
                    var model = ProbabilityDistributionFactory.createProbabilityDistributionModel(parameter.Value.DistributionType);
                    model.Initialize(parameter.Value.Value, parameter.Value.CvVariability ?? 0);
                    drawn.Add(parameter.Key, model.Sample(random));
                }
            }
            return drawn;
        }

        protected virtual double getAge(Individual individual, double defaultAge) {
            var age = individual.GetAge();
            if (age.HasValue && !double.IsNaN(age.Value)) {
                return age.Value;
            } else {
                return defaultAge;
            }
        }

        protected virtual double getGender(Individual individual, double defaultGender) {
            var gender = individual.GetGender();
            if (gender == GenderType.Undefined) {
                return defaultGender;
            } else {
                return gender == GenderType.Male ? 1d : 0d;
            }
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
