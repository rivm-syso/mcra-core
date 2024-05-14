using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators {
    public abstract class DesolvePbkModelCalculator : PbkModelCalculatorBase {

        protected List<double> _stateVariables;

        public Func<RDotNetEngine> CreateREngine = () => new RDotNetEngine();

        public DesolvePbkModelCalculator(
            KineticModelInstance kineticModelInstance
        ) : base(kineticModelInstance) {

            // Get simple states 
            var states = KineticModelDefinition.States
                .Where(r => !r.StateSubstances.Any())
                .ToDictionary(r => r.Order);

            //Get combined states
            var stateSubstances = KineticModelDefinition.States
                .Where(r => r.StateSubstances.Any())
                .SelectMany(c => c.StateSubstances, (state, subst) => new KineticModelStateVariableDefinition() {
                    Id = subst.Id,
                    Unit = state.Unit,
                    Description = state.Description,
                    Order = subst.Order,
                    IdSubstance = subst.IdSubstance
                })
                .ToDictionary(c => c.Order);
            stateSubstances.ToList().ForEach(x => states[x.Key] = x.Value);
            var nominalStates = states
                .OrderBy(c => c.Key)
                .ToDictionary(c => c.Value.Id, c => c.Value);
            _stateVariables = Enumerable
                .Repeat(0d, nominalStates.Count)
                .ToList();
        }

        /// <summary>
        /// Computes target organ exposures for the given collections of individual day exposures.
        /// </summary>
        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            ExposureUnitTriple externalExposureUnit,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            // Determine modelled/unmodelled exposure routes
            var modelExposureRoutes = KineticModelInstance.KineticModelDefinition.GetExposureRoutes();
            var unforcedExposureRoutes = exposureRoutes.Except(modelExposureRoutes).ToList();

            // Get time resolution
            var timeUnitMultiplier = getTimeUnitMultiplier(KineticModelInstance.ResolutionType);
            var stepLength = getStepLength(KineticModelInstance.ResolutionType, KineticModelDefinition.EvaluationFrequency);
            var resolution = stepLength * KineticModelDefinition.EvaluationFrequency;
            var evaluationPeriod = timeUnitMultiplier * KineticModelInstance.NumberOfDays;

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

            // Get kinetic model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            var relativeCompartmentWeights = getNominalRelativeCompartmentWeights(outputMappings)
                .ToDictionary(c => c.Item1, c => c.Item2);

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
            var eventsDictionary = modelExposureRoutes
                .ToDictionary(r => r, r => getEventTimings(
                    r, timeUnitMultiplier, KineticModelInstance.NumberOfDays, KineticModelInstance.SpecifyEvents
                ));
            var events = calculateCombinedEventTimings(eventsDictionary);
            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            progressState.Update("PBPK modelling started");
            using (var R = CreateREngine()) {
                R.LoadLibrary("deSolve", null, true);
                R.EvaluateNoReturn($"dyn.load(paste('{getModelFilePath()}', .Platform$dynlib.ext, sep = ''))");
                try {
                    R.SetSymbol("events", events);
                    R.EvaluateNoReturn(command: $"times <- seq(from=0, to={evaluationPeriod * resolution}, by={stepLength}) / {resolution} ");
                    foreach (var id in externalIndividualExposures.Keys) {
                        var boundedForcings = new List<string>();
                        var hasPositiveExposures = false;
                        foreach (var route in modelExposureRoutes) {
                            if (_modelInputDefinitions.ContainsKey(route)) {
                                var routeExposures = getRouteSubstanceIndividualDayExposures(
                                    externalIndividualExposures[id],
                                    substance,
                                    route
                                );
                                var substanceAmountUnitMultiplier = _modelInputDefinitions[route].DoseUnit
                                    .GetSubstanceAmountUnit()
                                    .GetMultiplicationFactor(
                                        externalExposureUnit.SubstanceAmountUnit,
                                        substance.MolecularMass
                                    );
                                var dailyDoses = routeExposures
                                    .Select(r => r / substanceAmountUnitMultiplier)
                                    .ToList();
                                var unitDoses = getUnitDoses(nominalInputParameters, dailyDoses, route);
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
                            if (KineticModelDefinition.IdGenderParameter != null) {
                                inputParameters[KineticModelDefinition.IdGenderParameter] = getGender(
                                    externalIndividualExposures[id].First().Individual,
                                    _modelParameterDefinitions[KineticModelDefinition.IdGenderParameter].DefaultValue ?? double.NaN
                                );
                            }

                            var outputKeys = KineticModelDefinition.GetModelOutputs().Keys;

                            var dllName = Path.GetFileNameWithoutExtension(KineticModelDefinition.FileName);
                            R.SetSymbol("inputs", inputParameters.Select(c => c.Value).ToList());
                            R.EvaluateNoReturn($"inputs <- .C('getParms', as.double(inputs), out=double(length(inputs)), as.integer(length(inputs)), PACKAGE='{dllName}')$out");
                            R.SetSymbol("outputs", outputKeys);
                            R.SetSymbol("states", _stateVariables);
                            R.EvaluateNoReturn($"allDoses <- list({string.Join(",", boundedForcings)})");

                            // Call to ODE
                            var cmd = "output <- ode(y = states, times = times, func = 'derivs', parms = inputs, " +
                                "dllname = '" + dllName + "', initfunc = 'initmod', initforc = 'initforc', " +
                                "forcings = allDoses, events = list(func = 'event', time = events), " +
                                "nout = length(outputs), outnames = outputs" + integrator + ")";
                            R.EvaluateNoReturn(cmd);

                            // Store results of selected compartments/species
                            foreach (var sc in outputMappings) {
                                var output = R.EvaluateNumericVector($"output[,'{sc.SpeciesId}']");
                                List<TargetExposurePerTimeUnit> exposures = null;

                                if (output.Any(r => r > 0)) {
                                    // Compartment volume/mass
                                    var compartmentSize = relativeCompartmentWeights[sc.CompartmentId] * bodyWeight;

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
                                                return new TargetExposurePerTimeUnit(i, exposure);
                                            })
                                            .ToList();
                                    } else {
                                        exposures = output
                                            .Select((r, i) => {
                                                var alignmentFactor = sc.GetUnitAlignmentFactor(compartmentSize);
                                                var result = new TargetExposurePerTimeUnit(i, r * alignmentFactor);
                                                return result;
                                            })
                                            .ToList();
                                    }
                                }

                                result.Add(new SubstanceTargetExposurePattern() {
                                    Substance = sc.Substance,
                                    Target = sc.TargetUnit.Target,
                                    Compartment = sc.CompartmentId,
                                    RelativeCompartmentWeight = relativeCompartmentWeights[sc.CompartmentId],
                                    ExposureType = exposureType,
                                    TargetExposuresPerTimeUnit = exposures ?? new(),
                                    NonStationaryPeriod = KineticModelInstance.NonStationaryPeriod,
                                    TimeUnitMultiplier = timeUnitMultiplier * KineticModelDefinition.EvaluationFrequency
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
                                    TargetExposuresPerTimeUnit = new(),
                                    NonStationaryPeriod = KineticModelInstance.NonStationaryPeriod,
                                    TimeUnitMultiplier = timeUnitMultiplier * KineticModelDefinition.EvaluationFrequency
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
            var dllName = Path.GetFileNameWithoutExtension(KineticModelDefinition.FileName);
            var dllPath = Path.Combine(assemblyFolder, "Resources", "KineticModels", $"{dllName}");
            //convert backslashes to / explicitly, path is used in R script
            return dllPath.Replace(@"\", "/");
        }

        protected virtual List<int> calculateCombinedEventTimings(IDictionary<ExposurePathType, List<int>> eventsDictionary) {
            return eventsDictionary.SelectMany(c => c.Value).Distinct().OrderBy(c => c).ToList();
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
                    model.Initialize(parameter.Value.Value, parameter.Value.CvVariability);
                    drawn.Add(parameter.Key, model.Sample(random));
                }
            }
            return drawn;
        }

        protected int getTimeUnitMultiplier(TimeUnit resolutionType) {
            switch (resolutionType) {
                case TimeUnit.Hours:
                    return 24;
                case TimeUnit.Minutes:
                    return 3600;
                default:
                    throw new Exception("Unknown time unit multiplier");
            }
        }

        protected int getStepLength(TimeUnit resolutionType, int evaluationFrequency) {
            switch (resolutionType) {
                case TimeUnit.Hours:
                    return 60 / evaluationFrequency;
                case TimeUnit.Minutes:
                    return 1;
                default:
                    throw new Exception("Unknown time unit multiplier");
            }
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

        /// <summary>
        /// Returns the relative compartment weights of the output compartments that are supported
        /// by the kinetic model calculator.
        /// </summary>
        /// <returns>A collection (compartment, relative weight)</returns>
        protected ICollection<(string compartment, double weight)> getNominalRelativeCompartmentWeights(
            List<TargetOutputMapping> outputMappings
        ) {
            var result = new List<(string, double)>();
            var compartmentCodes = outputMappings
                .Select(r => r.CompartmentId)
                .Distinct()
                .ToList();

            foreach (var compartment in compartmentCodes) {
                var outputParam = KineticModelDefinition.Outputs
                    .FirstOrDefault(r => r.Id == compartment);

                // Get nominal input parameters
                var nominalInputParameters = KineticModelDefinition.Parameters
                    .Where(r => !r.SubstanceParameterValues.Any())
                    .ToDictionary(
                        r => r.Id,
                        r => KineticModelInstance.KineticModelInstanceParameters
                            .TryGetValue(r.Id, out var parameter) ? parameter.Value : 0
                    );

                // Get nominal input parameters for parent and metabolites
                var substanceParameterValues = KineticModelDefinition.Parameters
                    .Where(r => r.SubstanceParameterValues.Any())
                    .SelectMany(c => c.SubstanceParameterValues.Select(r => r.IdParameter))
                    .ToDictionary(
                        r => r,
                        r => KineticModelInstance.KineticModelInstanceParameters
                            .TryGetValue(r, out var parameter) ? parameter.Value : 0
                    );

                // Combine dictionaries
                substanceParameterValues.ToList().ForEach(x => nominalInputParameters[x.Key] = x.Value);
                var relativeCompartmentWeight = getRelativeCompartmentWeight(outputParam, nominalInputParameters);
                result.Add((compartment, relativeCompartmentWeight));
            }
            return result;
        }

        protected virtual double getRelativeCompartmentWeight(
            KineticModelOutputDefinition parameter,
            IDictionary<string, double> parameters
        ) {
            var factor = 1D;
            foreach (var scalingFactor in parameter.ScalingFactors) {
                factor *= parameters[scalingFactor];
            }
            foreach (var multiplicationFactor in parameter.MultiplicationFactors) {
                factor *= multiplicationFactor;
            }
            return factor;
        }
    }
}
