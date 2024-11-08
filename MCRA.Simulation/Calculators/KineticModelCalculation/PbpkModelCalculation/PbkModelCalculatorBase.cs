using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public abstract class PbkModelCalculatorBase : IKineticModelCalculator {

        public double PrecisionReverseDoseCalculation { get; set; } = 0.001;

        // Model instance
        public KineticModelInstance KineticModelInstance { get; }
        public Compound Substance => KineticModelInstance.InputSubstance;
        public List<Compound> OutputSubstances => KineticModelInstance.Substances.ToList();

        // Model definition
        public KineticModelDefinition KineticModelDefinition => KineticModelInstance.KineticModelDefinition;
        protected IDictionary<ExposurePathType, KineticModelInputDefinition> _modelInputDefinitions;
        protected IDictionary<string, KineticModelParameterDefinition> _modelParameterDefinitions;

        // Run/simulation settings
        protected readonly int _timeUnitMultiplier;
        protected readonly int _numberOfDays;
        protected readonly int _evaluationPeriod;
        protected readonly int _steps;

        public PbkModelCalculatorBase(
            KineticModelInstance kineticModelInstance
        ) {
            KineticModelInstance = kineticModelInstance;

            // Lookups/dictionaries for model definition elements
            _modelInputDefinitions = KineticModelDefinition.Forcings
                .ToDictionary(r => r.Route);
            _modelParameterDefinitions = KineticModelDefinition.Parameters
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);

            // Multiplier for converting days to the time-scale of the model
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(KineticModelDefinition.TimeScale);
            if (timeUnitMultiplier < 1) {
                throw new NotImplementedException();
            }
            _timeUnitMultiplier = (int)timeUnitMultiplier;

            // Simulation run-settings: total number of days, total evaluation period of the model and the number of steps
            _numberOfDays = KineticModelInstance.NumberOfDays;
            _evaluationPeriod = _numberOfDays * _timeUnitMultiplier;
            _steps = _evaluationPeriod * KineticModelDefinition.EvaluationFrequency;
        }

        /// <summary>
        /// Computes peak target (internal) substance amounts.
        /// </summary>
        public List<AggregateIndividualDayExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var externalIndividualExposures = individualDayExposures
                .ToDictionary(
                    r => r.SimulatedIndividualDayId, 
                    r => new List<IExternalIndividualDayExposure>() { r }
                );
            var targetExposures = calculate(
                externalIndividualExposures,
                exposureUnit,
                Substance,
                exposureRoutes,
                targetUnits,
                ExposureType.Acute,
                false,
                generator,
                progressState
            );

            var result = new List<AggregateIndividualDayExposure>();
            foreach (var individualExposure in individualDayExposures) {
                var individualTargetExposure = targetExposures[individualExposure.SimulatedIndividualDayId];
                var internalIndividualExposure = new AggregateIndividualDayExposure() {
                    Individual = individualExposure.Individual,
                    SimulatedIndividualDayId = individualExposure.SimulatedIndividualDayId,
                    Day = individualExposure.Day,
                    IndividualSamplingWeight = individualExposure.IndividualSamplingWeight,
                    SimulatedIndividualId = individualExposure.SimulatedIndividualId,
                    ExternalIndividualDayExposures = [ individualExposure ],
                    InternalTargetExposures = individualTargetExposure
                        .Where(r => r.Target != null)
                        .GroupBy(r => r.Target)
                        .ToDictionary(
                            g => g.Key,
                            g => g
                                .ToDictionary(
                                    r => r.Substance,
                                    r => r as ISubstanceTargetExposure
                                )
                        )
                };
                result.Add(internalIndividualExposure);
            }
            return result;
        }

        /// <summary>
        /// Override: computes long term target (internal) substance amounts.
        /// </summary>
        public List<AggregateIndividualExposure> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var externalIndividualExposures = individualExposures
                .ToDictionary(r => r.SimulatedIndividualId, r => r.ExternalIndividualDayExposures);

            // Contains for all individuals the exposure pattern.
            var targetExposures = calculate(
                externalIndividualExposures,
                exposureUnit,
                Substance,
                exposureRoutes,
                targetUnits,
                ExposureType.Chronic,
                false,
                generator,
                progressState
            );

            var result = new List<AggregateIndividualExposure>();
            foreach (var individualExposure in individualExposures) {
                var individualTargetExposure = targetExposures[individualExposure.SimulatedIndividualId];
                var internalIndividualExposure = new AggregateIndividualExposure() {
                    Individual = individualExposure.Individual,
                    IndividualSamplingWeight = individualExposure.IndividualSamplingWeight,
                    SimulatedIndividualId = individualExposure.SimulatedIndividualId,
                    ExternalIndividualDayExposures = individualExposure.ExternalIndividualDayExposures,
                    InternalTargetExposures = individualTargetExposure
                        .Where(r => r.Target != null)
                        .GroupBy(r => r.Target)
                        .ToDictionary(
                            g => g.Key,
                            g => g
                                .ToDictionary(
                                    r => r.Substance,
                                    r => r as ISubstanceTargetExposure
                                )
                        )
                };
                result.Add(internalIndividualExposure);
            }
            return result;
        }

        /// <summary>
        /// Override: computes substance internal exposure for the specified individual
        /// day exposure.
        /// </summary>
        public ISubstanceTargetExposure Forward(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposurePathType exposureRoute,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            var individualExposureRoutes = new Dictionary<int, List<IExternalIndividualDayExposure>> {
                { 0, new List<IExternalIndividualDayExposure> { externalIndividualDayExposure } }
            };
            var internalExposures = calculate(
                individualExposureRoutes,
                exposureUnit,
                Substance,
                [exposureRoute],
                [targetUnit],
                exposureType,
                true,
                generator,
                new ProgressState()
            );
            return internalExposures[0].First();
        }

        /// <summary>
        /// Chronic
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
        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                externalIndividualExposures,
                KineticModelInstance.Substances.First(),
                _modelInputDefinitions.Keys,
                exposureUnit
            );
            // TODO: not the right place to compute average
            // exposures per route and define nominal individual.
            var individual = new Individual(0) {
                BodyWeight = externalIndividualExposures.Average(c => c.Individual.BodyWeight),
            };
            return computeAbsorptionFactors(
                Substance,
                individual,
                exposureRoutes,
                exposurePerRoutes,
                ExposureType.Chronic,
                exposureUnit,
                targetUnit,
                generator
            );
        }

        /// <summary>
        /// Acute
        /// Calculate absorptionfactors
        /// </summary>
        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                externalIndividualDayExposures,
                KineticModelInstance.Substances.First(),
                exposureUnit,
                _modelInputDefinitions.Keys
            );
            // TODO: not the right place to compute average
            // exposures per route and define nominal individual.
            var individual = new Individual(0) {
                BodyWeight = externalIndividualDayExposures.Average(c => c.Individual.BodyWeight),
            };
            return computeAbsorptionFactors(
                Substance,
                individual,
                exposureRoutes,
                exposurePerRoutes,
                ExposureType.Acute,
                exposureUnit,
                targetUnit,
                generator
            );
        }

        public double Forward(
            Individual individual,
            double dose,
            ExposurePathType exposureRoute,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator = null
        ) {
            var externalExposure = ExternalIndividualDayExposure
                .FromSingleDose(
                    exposureRoute,
                    Substance,
                    dose,
                    exposureUnit,
                    individual
                );
            var internalExposure = Forward(
                externalExposure,
                exposureRoute,
                exposureUnit,
                internalTargetUnit,
                exposureType,
                generator
            );
            var targetDose = internalExposure.Exposure;
            return targetDose;
        }

        /// <summary>
        /// Override: uses bisection search to find the external dose corresponding to 
        /// the specified internal dose. The kinetic model is applied using nominal 
        /// values (i.e., without variability).
        /// </summary>
        public double Reverse(
            Individual individual,
            double dose,
            TargetUnit internalDoseUnit,
            ExposurePathType externalExposureRoute,
            ExposureUnitTriple externalExposureUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            var precision = PrecisionReverseDoseCalculation;
            var xLower = 10E-6 * dose;
            var xUpper = 10E6 * dose;
            var xMiddle = double.NaN;
            for (var i = 0; i < 1000; i++) {
                xMiddle = (xLower + xUpper) / 2;
                var fMiddle = Forward(
                    individual,
                    xMiddle,
                    externalExposureRoute,
                    externalExposureUnit,
                    internalDoseUnit,
                    exposureType,
                    generator
                );
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
        /// Runs the PBK model for the provided external individual exposures.
        /// </summary>
        protected abstract Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            ExposureUnitTriple externalExposureUnit,
            Compound inputSubstance,
            ICollection<ExposurePathType> selectedExposureRoutes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        );

        private Dictionary<ExposurePathType, double> computeAbsorptionFactors(
            Compound substance,
            Individual individual,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<ExposurePathType, double> exposurePerRoutes,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new Dictionary<ExposurePathType, double>();
            foreach (var route in exposureRoutes) {
                if (exposurePerRoutes.TryGetValue(route, out var externalDose)) {
                    var internalDose = Forward(
                        individual,
                        externalDose,
                        route,
                        exposureUnit,
                        targetUnit,
                        exposureType,
                        generator
                    );
                    result[route] = internalDose / externalDose;
                }
            }
            return result;
        }

        /// <summary>
        /// Get external individual day exposures of the specified route and substance.
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoute"></param>
        /// <returns></returns>
        protected List<double> getRouteSubstanceIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            Compound substance,
            ExposurePathType exposureRoute
        ) {
            var routeExposures = externalIndividualDayExposures
                .Select(individualDay => {
                    if (individualDay.ExposuresPerRouteSubstance.ContainsKey(exposureRoute)) {
                        return individualDay.ExposuresPerRouteSubstance[exposureRoute]
                            .Where(r => r.Compound == substance)
                            .Sum(r => r.Amount);
                    } else {
                        return 0d;
                    }
                })
                .ToList();
            return routeExposures;
        }

        /// <summary>
        /// Computes the average of the positive, chronic, substance exposures per route.
        /// NOTE: uses samplingweights to account for weighing of individuals.
        /// </summary>
        private static IDictionary<ExposurePathType, double> computeAverageSubstanceExposurePerRoute(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit
        ) {
            var exposurePerRoute = new Dictionary<ExposurePathType, double>();
            foreach (var route in exposureRoutes) {
                var positives = externalIndividualExposures.Where(r => r.ExposuresPerRouteSubstance.ContainsKey(route)
                        && r.ExposuresPerRouteSubstance[route].Any(epc => epc.Compound == substance)
                        && r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == substance).Amount > 0)
                    .ToList();
                if (positives.Any()) {
                    var averageBodyWeight = positives.Average(c => c.Individual.BodyWeight);
                    var sumOfWeights = externalIndividualExposures.Sum(c => c.IndividualSamplingWeight);
                    var exposures = positives
                        .Select(r => r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == substance).Amount * r.IndividualSamplingWeight)
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
        private static Dictionary<ExposurePathType, double> computeAverageSubstanceExposurePerRoute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            Compound compound,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposurePathType> exposureRoutes
        ) {
            var exposurePerRoute = new Dictionary<ExposurePathType, double>();
            foreach (var route in exposureRoutes) {
                var positives = externalIndividualDayExposures
                    .Where(r => r.ExposuresPerRouteSubstance.ContainsKey(route)
                        && r.ExposuresPerRouteSubstance[route].Any(epc => epc.Compound == compound)
                        && r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == compound).Amount > 0)
                    .ToList();
                if (positives.Any()) {
                    var averageBodyWeight = positives.Average(c => c.Individual.BodyWeight);
                    var sumOfWeights = positives.Sum(c => c.IndividualSamplingWeight);
                    var exposures = positives
                        .Select(r => r.ExposuresPerRouteSubstance[route].First(epc => epc.Compound == compound).Amount * r.IndividualSamplingWeight)
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

        protected void setPhysiologicalParameterValues(Dictionary<string, double> physiologicalParameters, Individual individual) {
            var instanceParameters = KineticModelInstance.KineticModelInstanceParameters;

            // Set BW
            if (!string.IsNullOrEmpty(KineticModelDefinition.IdBodyWeightParameter)) {
                // TODO: current code assumes bodyweights in same unit as kinetic model parameter
                var bodyWeight = individual.BodyWeight;
                physiologicalParameters.Add(KineticModelDefinition.IdBodyWeightParameter, bodyWeight);

                // Set BSA
                if (!string.IsNullOrEmpty(KineticModelDefinition.IdBodySurfaceAreaParameter)) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdBodySurfaceAreaParameter, out var bsaParameterValue)
                        && instanceParameters.TryGetValue(KineticModelDefinition.IdBodyWeightParameter, out var bwParameterValue)
                    ) {
                        var standardBSA = bsaParameterValue.Value;
                        var standardBW = bwParameterValue.Value;
                        var allometricScaling = Math.Pow(standardBW / bodyWeight, 1 - 0.7);
                        physiologicalParameters[KineticModelDefinition.IdBodySurfaceAreaParameter] = standardBSA / allometricScaling;
                    }
                }
            }

            // Set age
            if (!string.IsNullOrEmpty(KineticModelDefinition.IdAgeParameter)) {
                // Get individual age
                // TODO: current code assumes age in same unit as kinetic model parameter
                var age = individual.GetAge();
                if (!age.HasValue || double.IsNaN(age.Value)) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdAgeParameter, out var ageParameterValue)
                        && !double.IsNaN(ageParameterValue.Value)
                    ) {
                        // Fallback on age from kinetic model parametrisation
                        age = ageParameterValue.Value;
                    } else if (_modelParameterDefinitions[KineticModelDefinition.IdAgeParameter].DefaultValue.HasValue) {
                        // Fallback on default age from kinetic model definition
                        age = _modelParameterDefinitions[KineticModelDefinition.IdAgeParameter].DefaultValue.Value;
                    } else {
                        throw new Exception($"Cannot set required parameter for age for PBK model [{KineticModelDefinition.Name}].");
                    }
                }
                physiologicalParameters[KineticModelDefinition.IdAgeParameter] = age.Value;
            }

            // Set sex
            if (!string.IsNullOrEmpty(KineticModelDefinition.IdSexParameter)) {
                // TODO: implicit assumption of Female = 1, Male = 2 should become explicit
                var sex = individual.GetGender();
                if (sex == GenderType.Undefined) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdSexParameter, out var paramValue)
                        && !double.IsNaN(paramValue.Value)
                    ) {
                        // Fallback on age from kinetic model parametrisation
                        sex = (GenderType)paramValue.Value;
                    } else if (_modelParameterDefinitions[KineticModelDefinition.IdSexParameter].DefaultValue.HasValue) {
                        // Fallback on default age from kinetic model definition
                        sex = (GenderType)_modelParameterDefinitions[KineticModelDefinition.IdSexParameter].DefaultValue;
                    } else {
                        throw new Exception($"Cannot set required parameter for sex for PBK model [{KineticModelDefinition.Name}].");
                    }
                }
                physiologicalParameters[KineticModelDefinition.IdSexParameter] = (double)sex;
            }
        }

        /// <summary>
        /// Correct doses for number of doses per day based on exposure route.
        /// </summary>
        protected virtual List<double> getUnitDoses(
            IDictionary<string, KineticModelInstanceParameter> parameters,
            List<double> doses,
            ExposurePathType route
        ) {
            var result = new List<double>();
            switch (route) {
                case ExposurePathType.Oral:
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposurePathType.Dermal:
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposurePathType.Inhalation:
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation));
                    break;
                default:
                    throw new Exception("Route not recognized");
            }
            return result;
        }

        /// <summary>
        /// Draw random doses
        /// </summary>
        protected static List<double> drawSimulatedDoses(List<double> doseRecords, int numberOfDoses, IRandom random) {
            var doses = new List<double>();
            for (var i = 0; i < numberOfDoses; i++) {
                var ix = random.Next(0, doseRecords.Count);
                doses.Add(doseRecords[ix]);
            }
            return doses;
        }

        protected Dictionary<ExposurePathType, List<int>> getExposureEventTimings(
            ICollection<ExposurePathType> routes,
            int timeMultiplier,
            int numberOfDays,
            bool specifyEvents
        ) {
            var result = routes.ToDictionary(
                r => r,
                r => getEventTimings(
                    r,
                    timeMultiplier,
                    numberOfDays,
                    specifyEvents
                )
            );
            return result;
        }

        protected List<int> getEventTimings(
            ExposurePathType route,
            int timeMultiplier,
            int numberOfDays,
            bool specifyEvents
        ) {
            if (specifyEvents) {
                switch (route) {
                    case ExposurePathType.Oral:
                        return getAllEvents(KineticModelInstance.SelectedEvents, timeMultiplier, numberOfDays);
                    case ExposurePathType.Dermal:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal, timeMultiplier, numberOfDays);
                    case ExposurePathType.Inhalation:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation, timeMultiplier, numberOfDays);
                    default:
                        throw new Exception("Route not recognized");
                }
            } else {
                switch (route) {
                    case ExposurePathType.Oral:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDay, timeMultiplier, numberOfDays);
                    case ExposurePathType.Dermal:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal, timeMultiplier, numberOfDays);
                    case ExposurePathType.Inhalation:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation, timeMultiplier, numberOfDays);
                    default:
                        throw new Exception("Route not recognized");
                }
            }
        }

        /// <summary>
        /// Get all events based on specification of numberOfDoses.
        /// </summary>
        private List<int> getAllEvents(int numberOfDoses, int timeMultiplier, int numberOfDays) {
            if (numberOfDoses <= 0) {
                numberOfDoses = 1;
            }
            var interval = timeMultiplier / numberOfDoses;
            var events = new List<int>();
            for (var d = 0; d < numberOfDays; d++) {
                for (var n = 0; n < numberOfDoses; n++) {
                    events.Add(n * interval + d * timeMultiplier);
                }
            }
            return events;
        }

        /// <summary>
        /// Get all events based on specification of selected events/hours
        /// </summary>
        private List<int> getAllEvents(int[] selectedEvents, int timeMultiplier, int numberOfDays) {
            if (timeMultiplier != 24) {
                throw new Exception("Specification of events/hours is only implemented for resolution = 24 hours.");
            }
            var events = new List<int>();
            for (var d = 0; d < numberOfDays; d++) {
                for (var n = 0; n < selectedEvents.Length; n++) {
                    events.Add(selectedEvents[n] + d * timeMultiplier - 1);
                }
            }
            return events;
        }

        /// <summary>
        /// Gets the output mappings for the specified target units.
        /// </summary>
        /// <param name="targetUnits"></param>
        /// <returns></returns>
        protected List<TargetOutputMapping> getTargetOutputMappings(ICollection<TargetUnit> targetUnits) {
            var result = new List<TargetOutputMapping>();
            foreach (var targetUnit in targetUnits) {
                var output = KineticModelDefinition.Outputs
                    .FirstOrDefault(c => c.TargetUnit.Target == targetUnit.Target);
                if (output == null) {
                    var msg = $"No output found in PBK model [{KineticModelDefinition.Id}] for target [{targetUnit.Target.GetDisplayName()}].";
                    throw new Exception(msg);
                }
                var codeCompartment = output.Id;
                if (output.Species?.Count > 0) {
                    foreach (var species in output.Species) {
                        var substance = !string.IsNullOrEmpty(species.IdSubstance)
                            ? KineticModelInstance.KineticModelSubstances
                                .FirstOrDefault(r => r.SubstanceDefinition?.Id == species.IdSubstance)?.Substance
                            : KineticModelInstance.Substances.FirstOrDefault();
                        if (substance != null) {
                            var record = new TargetOutputMapping() {
                                CompartmentId = codeCompartment,
                                SpeciesId = species.IdSpecies,
                                Substance = substance,
                                OutputDefinition = output,
                                TargetUnit = targetUnit
                            };
                            result.Add(record);
                        } else {
                            // TODO: what to do when output substance is not defined?
                            // It seems reasonable to allow this when the missing substance is a
                            // metabolite and to throw an exception when the missing substance is
                            // the parent/input substance.
                        }
                    }
                } else {
                    var record = new TargetOutputMapping() {
                        CompartmentId = codeCompartment,
                        SpeciesId = codeCompartment,
                        Substance = KineticModelInstance.Substances.Single(),
                        OutputDefinition = output,
                        TargetUnit = targetUnit
                    };
                    result.Add(record);
                }
            }
            return result;
        }
    }
}
