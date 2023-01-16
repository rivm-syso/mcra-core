using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using System.Reflection;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public abstract class PbpkModelCalculator : LinearDoseAggregationCalculator {

        protected List<string> _outputs;
        protected List<(string, Compound)> _substanceCompartments;
        protected List<double> _stateVariables;
        protected IDictionary<ExposureRouteType, KineticModelInputDefinition> _forcings;
        protected KineticModelInstance _kineticModelInstance;

        public KineticModelDefinition KineticModelDefinition {
            get {
                return _kineticModelInstance.KineticModelDefinition;
            }
        }

        public KineticModelInstance KineticModelInstance {
            get {
                return _kineticModelInstance;
            }
        }

        public override Compound InputSubstance {
            get {
                return _kineticModelInstance.InputSubstance;
            }
        }

        public override List<Compound> OutputSubstances {
            get {
                // TODO kinetic models: the output substances may depend
                // on the selected compartment.
                return _kineticModelInstance.Substances.ToList();
            }
        }

        public PbpkModelCalculator(
            KineticModelInstance kineticModelInstance,
            Dictionary<ExposureRouteType, double> defaultAbsorptionFactors
        ) : base(kineticModelInstance.InputSubstance, defaultAbsorptionFactors) {
            _kineticModelInstance = kineticModelInstance;
            _forcings = KineticModelDefinition.Forcings
                .ToDictionary(r => r.Id);
            _stateVariables = Enumerable
                .Repeat(0d, KineticModelDefinition.States.Count)
                .ToList();
            var outputs = KineticModelDefinition.Outputs
                .OrderBy(c => c.Order)
                .Select(c => c.Id)
                .ToList();
            _outputs = getOutPutCompartments(outputs);
            _substanceCompartments = getSubstanceCompartments(outputs);
        }

        /// <summary>
        /// Override: computes peak target (internal) substance amounts.
        /// </summary>
        /// <param name="individualDayExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="progressState"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override List<IndividualDaySubstanceTargetExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            ProgressState progressState,
            IRandom generator
        ) {
            var individualDayExposureRoutes = individualDayExposures.ToDictionary(r => r.SimulatedIndividualDayId, r => new List<IExternalIndividualDayExposure>() { r });
            var targetExposures = calculate(individualDayExposureRoutes, substance, exposureRoutes, ExposureType.Acute, exposureUnit, relativeCompartmentWeight, false, generator, progressState);
            var result = individualDayExposures
                .Select((r, i) => {
                    if (targetExposures.TryGetValue(i, out var exposures)) {
                        return new IndividualDaySubstanceTargetExposure() {
                            SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                            SubstanceTargetExposures = exposures.Select(c => c as ISubstanceTargetExposure).ToList()
                        };
                    } else {
                        return new IndividualDaySubstanceTargetExposure();
                    }
                }).ToList();
            var peak = result.SelectMany(c => c.SubstanceTargetExposures.Select(q => (q as SubstanceTargetExposurePattern).PeakTargetExposure)).ToList();
            var steady = result.SelectMany(c => c.SubstanceTargetExposures.Select(q => (q as SubstanceTargetExposurePattern).SteadyStateTargetExposure)).ToList();
            return result;
        }

        /// <summary>
        /// Override: computes long term target (internal) substance amounts.
        /// </summary>
        /// <param name="individualExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="progressState"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override List<IndividualSubstanceTargetExposure> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            ProgressState progressState,
            IRandom generator
        ) {
            var individualExposureRoutes = individualExposures
                .ToDictionary(r => r.SimulatedIndividualId, r => r.ExternalIndividualDayExposures);
            //Contains for all individuals the exposure pattern.
            var targetExposures = calculate(individualExposureRoutes, substance, exposureRoutes, ExposureType.Chronic, exposureUnit, relativeCompartmentWeight, false, generator, progressState);
            var result = individualExposures
                .Select((r, i) => {
                    if (targetExposures.TryGetValue(i, out var exposures)) {
                        return new IndividualSubstanceTargetExposure() {
                            Individual = r.Individual,
                            SimulatedIndividualId = r.SimulatedIndividualId,
                            IndividualSamplingWeight = r.IndividualSamplingWeight,
                            SubstanceTargetExposures = exposures.Select(c => c as ISubstanceTargetExposure).ToList()
                        };
                    } else {
                        return new IndividualSubstanceTargetExposure();
                    }
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Override: computes substance internal exposure for the specified individual
        /// day exposure.
        /// </summary>
        /// <param name="externalIndividualDayExposure"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="exposureType"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            Compound substance,
            ExposureRouteType exposureRoute,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            IRandom generator = null
        ) {
            var individualExposureRoutes = new Dictionary<int, List<IExternalIndividualDayExposure>> {
                { 0, new List<IExternalIndividualDayExposure> { externalIndividualDayExposure } }
            };
            var exposureRoutes = new List<ExposureRouteType>() { exposureRoute };
            var internalExposures = calculate(
                individualExposureRoutes,
                substance,
                exposureRoutes,
                exposureType,
                exposureUnit,
                relativeCompartmentWeight,
                true,
                generator,
                new ProgressState()
            );
            return internalExposures[0].First();
        }

        /// <summary>
        /// Calculates an average absorption factor for all routes (forcings) in the kinetic model weighted 
        /// by individual sampling weights. For each compound, the external and internal mean exposure is 
        /// calculated. The ratio: internal/external is the absorption factor. For the external exposure the 
        /// contribution of each route is known, however for the internal exposure the contribution of each
        /// route can not be calculated. The internal exposure is the result of what happens in the kinetic 
        /// model and it is no longer possible to backtrack  the contributions of the different routes to the 
        /// internal exposure. For a kinetic model containing multiple routes, the absorption factor is the 
        /// combined result of all routes. So all routes available in the kinetic model (forcings) are assigned
        /// the same absorption factor (which reflects the the combined result of all routes in the kinetic 
        /// model).
        /// </summary>
        /// <param name="aggregateIndividualExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override Dictionary<ExposureRouteType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var individualDayExposures = aggregateIndividualExposures
                .Cast<IExternalIndividualExposure>()
                .ToList();
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                individualDayExposures,
                _kineticModelInstance.Substances.First(),
                _forcings.Keys,
                exposureUnit
            );
            return computeAbsorptionFactors(
                substance,
                exposureRoutes,
                exposurePerRoutes,
                ExposureType.Chronic,
                exposureUnit,
                nominalBodyWeight,
                generator
            );
        }

        /// <summary>
        /// Calculate absorptionfactors
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override Dictionary<ExposureRouteType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var individualDayExposures = aggregateIndividualDayExposures.Cast<IExternalIndividualDayExposure>().ToList();
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(individualDayExposures, _kineticModelInstance.Substances.First(), exposureUnit, _forcings.Keys);
            return computeAbsorptionFactors(substance, exposureRoutes, exposurePerRoutes, ExposureType.Acute, exposureUnit, nominalBodyWeight, generator);
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="dose"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="exposureType"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="bodyWeight"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override double CalculateTargetDose(
            double dose,
            Compound substance,
            ExposureRouteType exposureRoute,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double bodyWeight,
            double relativeCompartmentWeight,
            IRandom generator = null
        ) {
            var individual = new Individual(0) {
                BodyWeight = bodyWeight
            };
            var externalExposure = ExternalIndividualDayExposure.FromSingleDose(exposureRoute, substance, dose, exposureUnit, individual);
            var internalExposure = CalculateInternalDoseTimeCourse(externalExposure, substance, exposureRoute, exposureType, exposureUnit, relativeCompartmentWeight, generator);
            var targetDose = exposureUnit.IsPerBodyWeight()
                ? internalExposure.SubstanceAmount / (bodyWeight * relativeCompartmentWeight)
                : internalExposure.SubstanceAmount;
            return targetDose;
        }

        /// <summary>
        /// Override: uses bisection search to find the external dose corresponding to the specified internal dose.
        /// The kinetic model is applied using nominal values (i.e., without variability).
        ///
        /// </summary>
        /// <param name="dose"></param>
        /// <param name="substance"></param>
        /// <param name="externalExposureRoute"></param>
        /// <param name="exposureType"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="bodyWeight"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public override double Reverse(
            double dose,
            Compound substance,
            ExposureRouteType externalExposureRoute,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double bodyWeight,
            double relativeCompartmentWeight,
            IRandom generator
        ) {
            var precision = 0.001;
            var xLower = 10E-6 * dose;
            var xUpper = 10E6 * dose;
            var xMiddle = double.NaN;
            var fMiddle = double.NaN;
            for (int i = 0; i < 1000; i++) {
                xMiddle = (xLower + xUpper) / 2;
                fMiddle = CalculateTargetDose(xMiddle, substance, externalExposureRoute, exposureType, exposureUnit, bodyWeight, relativeCompartmentWeight, generator);
                if (Math.Abs(fMiddle - dose) < precision) {
                    break;
                }
                if (fMiddle > dose) {
                    xUpper = xMiddle;
                } else {
                    xLower = xMiddle;
                }
            }
            return xMiddle;
        }

        /// <summary>
        /// Computes target organ exposures for the given collections of individual day exposures.
        /// </summary>
        /// <param name="externalIndividualExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="exposureType"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="isNominal"></param>
        /// <param name="generator"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        private Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            // Determine modelled/unmodelled exposure routes
            var modelExposureRoutes = _kineticModelInstance.KineticModelDefinition.GetExposureRoutes();
            var unforcedExposureRoutes = exposureRoutes.Except(modelExposureRoutes).ToList();

            // Get time resolution
            int timeUnitMultiplier = getTimeUnitMultiplier(_kineticModelInstance.ResolutionType);
            var stepLength = getStepLength(_kineticModelInstance.ResolutionType, KineticModelDefinition.EvaluationFrequency);
            var resolution = stepLength * KineticModelDefinition.EvaluationFrequency;
            var evaluationPeriod = timeUnitMultiplier * _kineticModelInstance.NumberOfDays;

            // Get nominal input parameters
            var nominalInputParameters = KineticModelDefinition.Parameters
                .ToDictionary(
                    r => r.Id,
                    r => {
                        if (_kineticModelInstance.KineticModelInstanceParameters.TryGetValue(r.Id, out var parameter)) {
                            return parameter;
                        } else if (!r.IsInternalParameter) {
                            throw new Exception($"Kinetic model parameter {r.Id} not found in model instance {_kineticModelInstance.IdModelInstance}");
                        } else {
                            return new KineticModelInstanceParameter() { Value = 0D };
                        }
                    }
                );

            // Adapt physiological parameters to current individual
            var standardBW = nominalInputParameters[KineticModelDefinition.IdBodyWeightParameter].Value;
            var standardBSA = double.NaN;
            if (KineticModelDefinition.IdBodySurfaceAreaParameter != null) {
                standardBSA = nominalInputParameters[KineticModelDefinition.IdBodySurfaceAreaParameter].Value;
            }
            //Get integrator
            var integrator = string.Empty;
            if (KineticModelDefinition.IdIntegrator != null) {
                if (KineticModelDefinition.IdIntegrator.StartsWith("rk")) {
                    integrator = $", method = rkMethod('{KineticModelDefinition.IdIntegrator}') ";
                } else {
                    integrator = $", method = '{KineticModelDefinition.IdIntegrator}' ";
                }
            }
            // Get output parameter and output unit
            var selectedOutputParameter = KineticModelDefinition.Outputs.First(c => c.Id == _kineticModelInstance.CodeCompartment);
            var outputDoseUnit = TargetUnit.FromDoseUnit(selectedOutputParameter.DoseUnit, _kineticModelInstance.CodeCompartment);
            var isOutputConcentration = outputDoseUnit.IsPerBodyWeight();

            var reverseIntakeUnitConversionFactor = outputDoseUnit.GetAlignmentFactor(exposureUnit, substance.MolecularMass, relativeCompartmentWeight);
            var substanceAmountUnit = exposureUnit.SubstanceAmount;

            // Get events
            var eventsDictionary = modelExposureRoutes.ToDictionary(r => r, r => getEvents(
                r, timeUnitMultiplier, _kineticModelInstance.NumberOfDays, _kineticModelInstance.SpecifyEvents, _kineticModelInstance.SelectedEvents
            ));
            var events = calculateEvents(eventsDictionary);
            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            progressState.Update("PBPK modelling started");

            //var logger = new FileLogger($@"C:/LocalD/Data/Tmp/logTest.R");
            //using (var R = new LoggingRDotNetEngine(logger)) {
            using (var R = new RDotNetEngine()) {
                R.LoadLibrary("deSolve", null, true);
                R.EvaluateNoReturn($"dyn.load(paste('{getDllPath()}', .Platform$dynlib.ext, sep = ''))");
                try {
                    R.SetSymbol("events", events);
                    R.EvaluateNoReturn(command: $"times  <- seq(from=0, to={evaluationPeriod * resolution}, by={stepLength}) / {resolution} ");
                    foreach (var id in externalIndividualExposures.Keys) {
                        var boundedForcings = new List<string>();
                        var hasPositiveExposures = false;
                        foreach (var route in modelExposureRoutes) {
                            if (_forcings.ContainsKey(route)) {
                                var routeExposures = getRouteSubstanceIndividualDayExposures(externalIndividualExposures[id], substance, route);
                                var substanceAmountUnitMultiplier = _forcings[route].DoseUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(substanceAmountUnit, substance.MolecularMass);
                                var dailyDoses = routeExposures.Select(r => r / substanceAmountUnitMultiplier).ToList();
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
                        var exposureTuples = new List<(string, Compound, List<TargetExposurePerTimeUnit>)>();
                        var result = new List<SubstanceTargetExposurePattern>();
                        if (hasPositiveExposures) {
                            // Use variability
                            var inputParameters = drawParameters(nominalInputParameters, generator, isNominal, _kineticModelInstance.UseParameterVariability);
                            inputParameters = setStartingEvents(inputParameters);
                            var bodyWeight = externalIndividualExposures[id].First().Individual?.BodyWeight ?? standardBW;
                            inputParameters[KineticModelDefinition.IdBodyWeightParameter] = bodyWeight;
                            if (KineticModelDefinition.IdBodySurfaceAreaParameter != null) {
                                var allometricScaling = Math.Pow((standardBW / bodyWeight), (1 - 0.7));
                                inputParameters[KineticModelDefinition.IdBodySurfaceAreaParameter] = standardBSA / allometricScaling;
                            }
                            if (KineticModelDefinition.IdAgeParameter != null) {
                                inputParameters[KineticModelDefinition.IdAgeParameter] = getAge(inputParameters, externalIndividualExposures[id].First().Individual, KineticModelDefinition.IdAgeParameter);
                            }
                            if (KineticModelDefinition.IdGenderParameter != null) {
                                inputParameters[KineticModelDefinition.IdGenderParameter] = getGender(inputParameters, externalIndividualExposures[id].First().Individual, KineticModelDefinition.IdGenderParameter);
                            }

                            R.SetSymbol("inputs", inputParameters.Select(c => c.Value).ToList());
                            R.EvaluateNoReturn($"inputs <- .C('getParms', as.double(inputs), out=double(length(inputs)), as.integer(length(inputs)), PACKAGE='{KineticModelDefinition.DllName}')$out");

                            R.SetSymbol("outputs", _outputs);
                            R.SetSymbol("states", _stateVariables);
                            R.EvaluateNoReturn($"allDoses <- list({string.Join(",", boundedForcings)})");

                            // Call to ODE
                            var cmd = "output <- ode(y = states, times = times, func = 'derivs', parms = inputs, " +
                                "dllname = '" + KineticModelDefinition.DllName + "', initfunc = 'initmod', initforc = 'initforc', " +
                                "forcings = allDoses, events = list(func = 'event', time = events), " +
                                "nout = length(outputs), outnames = outputs" + integrator + ")";
                            R.EvaluateNoReturn(cmd);
                            foreach (var compartment in _substanceCompartments) {
                                var output = R.EvaluateNumericVector($"output[,'{compartment.Item1}']");
                                //fix for CPF_Metabolites model when TCPy is negative
                                if (output.Average() < 0) {
                                    var exposure = new List<TargetExposurePerTimeUnit>() { new TargetExposurePerTimeUnit() { Time = 0, Exposure = 0, } };
                                    exposureTuples.Add((compartment.Item1, compartment.Item2, exposure));
                                } else {

                                    var exposure = output.Select((r, i) =>
                                       new TargetExposurePerTimeUnit() {
                                           Time = i,
                                           Exposure = r * reverseIntakeUnitConversionFactor * (isOutputConcentration ? relativeCompartmentWeight * bodyWeight : 1D),
                                       })
                                       .ToList();
                                    exposureTuples.Add((compartment.Item1, compartment.Item2, exposure));
                                }
                            }
                        } else {
                            foreach (var compartment in _substanceCompartments) {
                                var exposure = new List<TargetExposurePerTimeUnit>() { new TargetExposurePerTimeUnit() { Time = 0, Exposure = 0, } };
                                exposureTuples.Add((compartment.Item1, compartment.Item2, exposure));
                            }
                        }

                        var targetExposureUnForced = calculateUnforcedRoutesSubstanceAmount(externalIndividualExposures[id], substance, unforcedExposureRoutes, relativeCompartmentWeight);
                        foreach (var item in exposureTuples) {
                            result.Add(new SubstanceTargetExposurePattern() {
                                Substance = item.Item2,
                                Compartment = item.Item1,
                                ExposureType = exposureType,
                                TargetExposuresPerTimeUnit = item.Item3,
                                NonStationaryPeriod = _kineticModelInstance.NonStationaryPeriod,
                                TimeUnitMultiplier = timeUnitMultiplier * KineticModelDefinition.EvaluationFrequency,
                                OtherRouteExposures = targetExposureUnForced
                            });
                        }
                        individualResults[id] = result;
                    }
                } finally {
                    R.EvaluateNoReturn($"dyn.unload(paste('{getDllPath()}', .Platform$dynlib.ext, sep = ''))");
                    //logger.Write();
                }
            }
            progressState.Update("PBPK modelling finished");
            return individualResults;
        }

        public override double GetNominalRelativeCompartmentWeight() {
            var outputParam = KineticModelDefinition.Outputs.FirstOrDefault(r => r.Id == _kineticModelInstance.CodeCompartment);
            var nominalInputParameterDictionary = KineticModelDefinition.Parameters
                .ToDictionary(
                    r => r.Id,
                    r => _kineticModelInstance.KineticModelInstanceParameters.TryGetValue(r.Id, out var parameter) ? parameter.Value : 0
                );
            return getRelativeCompartmentWeight(outputParam, nominalInputParameterDictionary);
        }

        protected abstract double getRelativeCompartmentWeight(KineticModelOutputDefinition outputParameter, Dictionary<string, double> parameters);

        private List<int> getEvents(
            ExposureRouteType route,
            int timeMultiplier,
            int numberOfDays,
            bool specifyEvents,
            int[] selectedEvents
        ) {
            if (specifyEvents) {
                switch (route) {
                    case ExposureRouteType.Dietary:
                        return getAllEvents(_kineticModelInstance.SelectedEvents, timeMultiplier, numberOfDays);
                    case ExposureRouteType.Oral:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDayNonDietaryOral, timeMultiplier, numberOfDays);
                    case ExposureRouteType.Dermal:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDayNonDietaryDermal, timeMultiplier, numberOfDays);
                    case ExposureRouteType.Inhalation:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation, timeMultiplier, numberOfDays);
                    default:
                        throw new Exception("Route not recognized");
                }
            } else {
                switch (route) {
                    case ExposureRouteType.Dietary:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDay, timeMultiplier, numberOfDays);
                    case ExposureRouteType.Oral:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDayNonDietaryOral, timeMultiplier, numberOfDays);
                    case ExposureRouteType.Dermal:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDayNonDietaryDermal, timeMultiplier, numberOfDays);
                    case ExposureRouteType.Inhalation:
                        return getAllEvents(_kineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation, timeMultiplier, numberOfDays);
                    default:
                        throw new Exception("Route not recognized");
                }
            }
        }

        /// <summary>
        /// Get all events based on specification of numberOfDoses
        /// </summary>
        /// <param name="numberOfDoses"></param>
        /// <param name="timeMultiplier"></param>
        /// <param name="numberOfDays"></param>
        /// <returns></returns>
        private List<int> getAllEvents(int numberOfDoses, int timeMultiplier, int numberOfDays) {
            if (numberOfDoses <= 0) {
                numberOfDoses = 1;
            }
            var interval = timeMultiplier / numberOfDoses;
            var events = new List<int>();
            for (int d = 0; d < numberOfDays; d++) {
                for (int n = 0; n < numberOfDoses; n++) {
                    events.Add(n * interval + d * timeMultiplier);
                }
            }
            return events;
        }

        /// <summary>
        /// Get all events based on specification of selected events/hours
        /// </summary>
        /// <param name="selectedEvents"></param>
        /// <param name="timeMultiplier"></param>
        /// <param name="numberOfDays"></param>
        /// <returns></returns>
        private List<int> getAllEvents(int[] selectedEvents, int timeMultiplier, int numberOfDays) {
            if (timeMultiplier != 24) {
                throw new Exception("Specification of events/hours is only implemented for resolution = 24 hours.");
            }
            var events = new List<int>();
            for (int d = 0; d < numberOfDays; d++) {
                for (int n = 0; n < selectedEvents.Length; n++) {
                    events.Add(selectedEvents[n] + d * timeMultiplier - 1);
                }
            }
            return events;
        }

        /// <summary>
        /// Combines doses with relevant events for each exposure
        /// </summary>
        /// <param name="allEvents"></param>
        /// <param name="events"></param>
        /// <param name="doses"></param>
        /// <returns></returns>
        protected virtual List<double> combineDosesWithEvents(List<int> allEvents, List<int> events, List<double> doses) {
            var dosesDict = allEvents.ToDictionary(r => r, r => 0D);
            for (int i = 0; i < events.Count; i++) {
                dosesDict[events[i]] = doses[i];
            }
            return dosesDict.Values.ToList();
        }

        /// <summary>
        /// Gets the dll path of the kinetic model definition.
        /// </summary>
        private string getDllPath() {
            var location = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var assemblyFolder = (new FileInfo(location.LocalPath).Directory).FullName;
            return Path.Combine(assemblyFolder, $"Resources/KineticModels/{KineticModelDefinition.DllName}").Replace(@"\", "/");
        }

        /// <summary>
        /// Returns a draw of the parameter values or nominal values
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, double> drawParameters(Dictionary<string, KineticModelInstanceParameter> parameters, IRandom random, bool isNominal, bool useParameterVariability) {
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

        /// <summary>
        /// Correct doses for number of doses per day based on exposure route
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="doses"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        protected virtual List<double> getUnitDoses(Dictionary<string, KineticModelInstanceParameter> parameters, List<double> doses, ExposureRouteType route) {
            var result = new List<double>();
            switch (route) {
                case ExposureRouteType.Dietary:
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposureRouteType.Oral:
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDayNonDietaryOral));
                    break;
                case ExposureRouteType.Dermal:
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposureRouteType.Inhalation:
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation));
                    break;
                default:
                    throw new Exception("Route not recognized");
            }
            return result;
        }

        protected virtual Dictionary<string, double> setStartingEvents(Dictionary<string, double> parameters) {
            return parameters;
        }

        protected virtual double getAge(Dictionary<string, double> parameters, Individual Individual, string ageProperty) {
            return 0;
        }

        protected virtual double getGender(Dictionary<string, double> parameters, Individual Individual, string genderProperty) {
            return 0;
        }

        protected virtual List<int> calculateEvents(Dictionary<ExposureRouteType, List<int>> eventsDictionary) {
            return eventsDictionary.SelectMany(c => c.Value).Distinct(c => c).OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Draw random doses
        /// </summary>
        /// <param name="doseRecords"></param>
        /// <param name="numberOfDoses"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        protected List<double> drawSimulatedDoses(List<double> doseRecords, int numberOfDoses, IRandom random) {
            var doses = new List<double>();
            for (int i = 0; i < numberOfDoses; i++) {
                var ix = random.Next(0, doseRecords.Count);
                doses.Add(doseRecords[ix]);
            }
            return doses;
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

        private Dictionary<ExposureRouteType, double> computeAbsorptionFactors(
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            Dictionary<ExposureRouteType, double> exposurePerRoutes,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var absorptionFactors = new Dictionary<ExposureRouteType, double>();
            var relativeCompartmentWeight = GetNominalRelativeCompartmentWeight();
            foreach (var route in exposureRoutes) {
                if (exposurePerRoutes.Keys.Contains(route)) {
                    var internalDose = CalculateTargetDose(exposurePerRoutes[route], substance, route, exposureType, exposureUnit, nominalBodyWeight, relativeCompartmentWeight, generator);
                    absorptionFactors[route] = exposureUnit.IsPerBodyWeight()
                        ? internalDose / exposurePerRoutes[route]
                        : (1 / relativeCompartmentWeight) * (internalDose / exposurePerRoutes[route]);
                } else {
                    absorptionFactors.Add(route, _absorptionFactors[route]);
                }
            }
            return absorptionFactors;
        }

        /// <summary>
        /// Computes the average of the positive, chronic, substance exposures per route.
        /// NOTE: uses samplingweights to account for weighing of individuals.
        /// </summary>
        /// <param name="externalIndividualExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        private static Dictionary<ExposureRouteType, double> computeAverageSubstanceExposurePerRoute(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit
        ) {
            var exposurePerRoute = new Dictionary<ExposureRouteType, double>();
            foreach (var route in exposureRoutes) {
                var positives = externalIndividualExposures.Where(r => r.ExposuresPerRouteSubstance.ContainsKey(route)
                        && r.ExposuresPerRouteSubstance[route].Any(epc => epc.Compound == substance)
                        && r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == substance).Exposure > 0)
                    .ToList();
                if (positives.Any()) {
                    var averageBodyWeight = positives.Average(c => c.Individual.BodyWeight);
                    var sumOfWeights = externalIndividualExposures.Sum(c => c.IndividualSamplingWeight);
                    var exposures = positives
                        .Select(r => r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == substance).Exposure * r.IndividualSamplingWeight)
                        .ToList();
                    var exposure = exposures.Sum() / sumOfWeights;
                    if (exposureUnit.IsPerBodyWeight()) {
                        exposure = exposure / averageBodyWeight;
                    }
                    exposurePerRoute.Add(route, exposure);
                }
            }
            return exposurePerRoute;
        }

        /// <summary>
        /// Computes the average of the positive, acute, substance exposures per route.
        /// NOTE: uses samplingweights to account for weighing of individuals.
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="compound"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="exposureRoutes"></param>
        /// <returns></returns>
        private static Dictionary<ExposureRouteType, double> computeAverageSubstanceExposurePerRoute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            Compound compound,
            TargetUnit exposureUnit,
            ICollection<ExposureRouteType> exposureRoutes
        ) {
            var exposurePerRoute = new Dictionary<ExposureRouteType, double>();
            foreach (var route in exposureRoutes) {
                var positives = externalIndividualDayExposures.Where(r => r.ExposuresPerRouteSubstance.ContainsKey(route)
                        && r.ExposuresPerRouteSubstance[route].Any(epc => epc.Compound == compound)
                        && r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == compound).Exposure > 0)
                    .ToList();
                if (positives.Any()) {
                    var averageBodyWeight = positives.Average(c => c.Individual.BodyWeight);
                    var sumOfWeights = positives.Sum(c => c.IndividualSamplingWeight);
                    var exposures = positives
                        .Select(r => r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == compound).Exposure * r.IndividualSamplingWeight)
                        .ToList();
                    var exposure = exposures.Sum() / sumOfWeights;
                    if (exposureUnit.IsPerBodyWeight()) {
                        exposure = exposure / averageBodyWeight;
                    }
                    exposurePerRoute.Add(route, exposure);
                }
            }
            return exposurePerRoute;
        }

        /// <summary>
        /// Calculates exposure for for unforced routes
        /// </summary>
        /// <param name="exposuresByRoute"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <returns></returns>
        private double calculateUnforcedRoutesSubstanceAmount(
            List<IExternalIndividualDayExposure> exposuresByRoute,
            Compound substance,
            ICollection<ExposureRouteType> unforcedRoutes,
            double relativeCompartmentWeight
        ) {
            var unforcedRoutesSubstanceAmount = 0d;
            foreach (var route in unforcedRoutes) {
                unforcedRoutesSubstanceAmount += _absorptionFactors[route] * relativeCompartmentWeight * getRouteSubstanceIndividualDayExposures(exposuresByRoute, substance, route).Average();
            }
            return unforcedRoutesSubstanceAmount;
        }

        /// <summary>
        /// Restore output compartments to output variables in kinetic model
        /// </summary>
        /// <param name="outputs"></param>
        /// <returns></returns>
        private List<string> getOutPutCompartments(List<string> outputs) {
            _outputs = new List<string>();
            foreach (var id in outputs) {
                var substances = KineticModelDefinition.Outputs.Where(c => c.Id == id).SelectMany(c => c.Substances).ToList();
                if (substances.Any()) {
                    foreach (var substance in substances) {
                        _outputs.Add($"{id}_{substance}");
                    }
                } else {
                    _outputs.Add(id);
                }
            }
            return _outputs;
        }

        /// <summary>
        /// Restore output compartments of the selected biological matrix to the output variables in kinetic model
        /// Currently the order of the substance compartments is dependent on the order in the XML.
        /// It is assumed that the Parent (P) is the first one followed by the metabolites (M1, M2 etc)
        /// </summary>
        /// <param name="outputs"></param>
        /// <returns></returns>
        private List<(string, Compound)> getSubstanceCompartments(List<string> outputs) {
            var result = new List<(string, Compound)>();
            var compartmentCode = outputs.Single(c => c == _kineticModelInstance.CodeCompartment);
            var output = KineticModelDefinition.Outputs
                .Single(c => c.Id == _kineticModelInstance.CodeCompartment);
            if (output.Substances?.Any() ?? false) {
                var kineticModelSubstanceIds = _kineticModelInstance.KineticModelSubstances
                    .Select(c => c.SubstanceDefinition.Id)
                    .ToList();
                var compartmentSubstanceCodes = output.Substances
                    .ToList();
                if (compartmentSubstanceCodes.Any()) {
                    foreach (var compartmentSubstanceId in compartmentSubstanceCodes) {
                        var instance = _kineticModelInstance.KineticModelSubstances
                            .Single(c => c.SubstanceDefinition.Id == compartmentSubstanceId);
                        result.Add(($"{compartmentCode}_{compartmentSubstanceId}", instance.Substance));
                    }
                } else {
                    result.Add((compartmentCode, _kineticModelInstance.Substances.First()));
                }
            } else {
                result.Add((compartmentCode, _kineticModelInstance.Substances.First()));
            }
            return result;
        }
    }
}
