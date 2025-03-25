using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
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
        public List<Compound> OutputSubstances => KineticModelInstance.Substances;

        // Model definition
        public KineticModelDefinition KineticModelDefinition => KineticModelInstance.KineticModelDefinition;
        protected IDictionary<ExposureRoute, KineticModelInputDefinition> _modelInputDefinitions;
        protected IDictionary<string, KineticModelParameterDefinition> _modelParameterDefinitions;

        // Run/simulation settings
        public PbkSimulationSettings SimulationSetings { get; }
        protected readonly int _timeUnitMultiplier;
        protected readonly int _evaluationPeriod;
        protected readonly int _steps;

        public PbkModelCalculatorBase(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) {
            KineticModelInstance = kineticModelInstance;
            SimulationSetings = simulationSettings;
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
            _evaluationPeriod = simulationSettings.NumberOfSimulatedDays * _timeUnitMultiplier;
            _steps = _evaluationPeriod * KineticModelDefinition.EvaluationFrequency;
        }

        /// <summary>
        /// Computes peak target (internal) substance amounts.
        /// </summary>
        public List<AggregateIndividualDayExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposureRoute> routes,
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
                routes,
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
                    SimulatedIndividual = individualExposure.SimulatedIndividual,
                    SimulatedIndividualDayId = individualExposure.SimulatedIndividualDayId,
                    Day = individualExposure.Day,
                    ExternalIndividualDayExposures = [individualExposure],
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
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var externalIndividualExposures = individualExposures
                .ToDictionary(r => r.SimulatedIndividual.Id, r => r.ExternalIndividualDayExposures);

            // Contains for all individuals the exposure pattern.
            var targetExposures = calculate(
                externalIndividualExposures,
                exposureUnit,
                Substance,
                routes,
                targetUnits,
                ExposureType.Chronic,
                false,
                generator,
                progressState
            );

            var result = new List<AggregateIndividualExposure>();
            foreach (var individualExposure in individualExposures) {
                var individualTargetExposure = targetExposures[individualExposure.SimulatedIndividual.Id];
                var internalIndividualExposure = new AggregateIndividualExposure() {
                    SimulatedIndividual = individualExposure.SimulatedIndividual,
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
            ExposureRoute route,
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
                [route],
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
        public IDictionary<ExposureRoute, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
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
            var individual = new SimulatedIndividual(
                new Individual(0) {
                    BodyWeight = externalIndividualExposures.Average(c => c.SimulatedIndividual.BodyWeight)
                }, 0
            );
            return computeAbsorptionFactors(
                Substance,
                individual,
                routes,
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
        public IDictionary<ExposureRoute, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
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
            var individual = new SimulatedIndividual(
                new(0) {
                    BodyWeight = externalIndividualDayExposures.Average(c => c.SimulatedIndividual.BodyWeight),
                }, 0);

            return computeAbsorptionFactors(
                Substance,
                individual,
                routes,
                exposurePerRoutes,
                ExposureType.Acute,
                exposureUnit,
                targetUnit,
                generator
            );
        }

        public double Forward(
            SimulatedIndividual individual,
            double dose,
            ExposureRoute route,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator = null
        ) {
            var externalExposure = ExternalIndividualDayExposure
                .FromSingleDose(
                    route,
                    Substance,
                    dose,
                    exposureUnit,
                    individual
                );
            var internalExposure = Forward(
                externalExposure,
                route,
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
            SimulatedIndividual individual,
            double dose,
            TargetUnit internalDoseUnit,
            ExposureRoute externalExposureRoute,
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
            ICollection<ExposureRoute> selectedExposureRoutes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        );

        private Dictionary<ExposureRoute, double> computeAbsorptionFactors(
            Compound substance,
            SimulatedIndividual individual,
            ICollection<ExposureRoute> routes,
            IDictionary<ExposureRoute, double> exposurePerRoutes,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new Dictionary<ExposureRoute, double>();
            foreach (var route in routes) {
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
                } else {
                    result[route] = double.NaN;
                }
            }
            return result;
        }

        /// <summary>
        /// Get external individual day exposures of the specified route and substance.
        /// </summary>
        protected List<double> getRouteSubstanceIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            Compound substance,
            ExposureRoute route
        ) {
            var exposures = externalIndividualDayExposures
                .Select(e => e.GetExposure(route, substance))
                .ToList();
            return exposures;
        }

        /// <summary>
        /// Computes the average of the positive, chronic, substance exposures per route.
        /// NOTE: uses samplingweights to account for weighing of individuals.
        /// </summary>
        private static IDictionary<ExposureRoute, double> computeAverageSubstanceExposurePerRoute(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            Compound substance,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit
        ) {
            var exposurePerRoute = new Dictionary<ExposureRoute, double>();
            foreach (var route in routes) {
                var positives = externalIndividualExposures
                    .Where(e => e.HasPositives(route, substance))
                    .ToList();

                if (positives.Any()) {
                    var averageBodyWeight = positives.Average(c => c.SimulatedIndividual.BodyWeight);
                    var sumOfWeights = externalIndividualExposures.Sum(c => c.SimulatedIndividual.SamplingWeight);
                    var exposures = positives
                       .Select(r => r.GetExposure(route, substance))
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
        private static Dictionary<ExposureRoute, double> computeAverageSubstanceExposurePerRoute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            Compound substance,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes
        ) {
            var exposurePerRoute = new Dictionary<ExposureRoute, double>();
            foreach (var route in routes) {
                var positives = externalIndividualDayExposures.Where(e => e.HasPositives(route, substance)).ToList();
                if (positives.Any()) {
                    var averageBodyWeight = positives.Average(c => c.SimulatedIndividual.BodyWeight);
                    var sumOfWeights = positives.Sum(c => c.SimulatedIndividual.SamplingWeight);
                    var exposures = positives
                       .Select(r => r.GetExposure(route, substance))
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

        protected void setPhysiologicalParameterValues(
            Dictionary<string, double> physiologicalParameters,
            SimulatedIndividual individual
        ) {
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
                var age = individual.Age;
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
                var sex = individual.Gender;
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
            ExposureRoute route
        ) {
            var result = new List<double>();
            switch (route) {
                case ExposureRoute.Oral:
                    doses.ForEach(c => result.Add(c / SimulationSetings.NumberOfDosesPerDay));
                    break;
                case ExposureRoute.Dermal:
                    doses.ForEach(c => result.Add(c / SimulationSetings.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposureRoute.Inhalation:
                    doses.ForEach(c => result.Add(c / SimulationSetings.NumberOfDosesPerDayNonDietaryInhalation));
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

        protected Dictionary<ExposureRoute, List<int>> getExposureEventTimings(
            ICollection<ExposureRoute> routes,
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
            ExposureRoute route,
            int timeMultiplier,
            int numberOfDays,
            bool specifyEvents
        ) {
            if (specifyEvents) {
                return route switch {
                    ExposureRoute.Oral => getAllEvents(SimulationSetings.SelectedEvents, timeMultiplier, numberOfDays),
                    ExposureRoute.Dermal => getAllEvents(SimulationSetings.NumberOfDosesPerDayNonDietaryDermal, timeMultiplier, numberOfDays),
                    ExposureRoute.Inhalation => getAllEvents(SimulationSetings.NumberOfDosesPerDayNonDietaryInhalation, timeMultiplier, numberOfDays),
                    _ => throw new Exception("Route not recognized"),
                };
            } else {
                return route switch {
                    ExposureRoute.Oral => getAllEvents(SimulationSetings.NumberOfDosesPerDay, timeMultiplier, numberOfDays),
                    ExposureRoute.Dermal => getAllEvents(SimulationSetings.NumberOfDosesPerDayNonDietaryDermal, timeMultiplier, numberOfDays),
                    ExposureRoute.Inhalation => getAllEvents(SimulationSetings.NumberOfDosesPerDayNonDietaryInhalation, timeMultiplier, numberOfDays),
                    _ => throw new Exception("Route not recognized"),
                };
            }
        }

        protected List<int> getRepeatedDailyEventTimings(
            ExposureRoute route,
            int timeMultiplier,
            int numberOfDays
        ) {
            return route switch {
                ExposureRoute.Oral => getAllEvents(1, timeMultiplier, numberOfDays),
                ExposureRoute.Dermal => getAllEvents(1, timeMultiplier, numberOfDays),
                ExposureRoute.Inhalation => getAllEvents(1, timeMultiplier, numberOfDays),
                _ => throw new Exception("Route not recognized"),
            }; ;
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
