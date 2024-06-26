﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public abstract class PbkModelCalculatorBase : LinearDoseAggregationCalculator {

        // Model instance
        public KineticModelInstance KineticModelInstance { get; }
        public override Compound InputSubstance => KineticModelInstance.InputSubstance;
        public override List<Compound> OutputSubstances => KineticModelInstance.Substances.ToList();

        // Model definition
        public KineticModelDefinition KineticModelDefinition => KineticModelInstance.KineticModelDefinition;
        protected IDictionary<string, KineticModelOutputDefinition> _modelOutputs;
        protected Dictionary<string, (Compound substance, KineticModelOutputDefinition outputDef)> _outputSubstancesMappings;
        protected IDictionary<ExposurePathType, KineticModelInputDefinition> _modelInputDefinitions;
        protected Dictionary<string, KineticModelOutputDefinition> _modelOutputDefinitions;
        protected IDictionary<string, KineticModelParameterDefinition> _modelParameterDefinitions;
        protected KineticModelOutputDefinition _selectedModelOutputDefinition;

        // Run/simulation settings
        protected readonly double _timeUnitMultiplier;
        protected readonly int _numberOfDays;
        protected readonly int _evaluationPeriod;
        protected readonly int _steps;

        public PbkModelCalculatorBase(
            KineticModelInstance kineticModelInstance,
            IDictionary<ExposurePathType, double> defaultAbsorptionFactors
        ) : base(kineticModelInstance.InputSubstance, defaultAbsorptionFactors) {
            KineticModelInstance = kineticModelInstance;

            // Lookups/dictionaries for model definition elements
            _modelInputDefinitions = KineticModelDefinition.Forcings
                .ToDictionary(r => r.Route);
            _modelParameterDefinitions = KineticModelDefinition.Parameters
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
            _modelOutputDefinitions = KineticModelDefinition.Outputs
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
            _modelOutputs = KineticModelDefinition.GetModelOutputs();
            _outputSubstancesMappings = getSubstanceOutputMappings();
            _selectedModelOutputDefinition = _modelOutputDefinitions[kineticModelInstance.CodeCompartment];

            // Multiplier for converting days to the time-scale of the model
            _timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(KineticModelDefinition.TimeScale);

            // Simulation run-settings: total number of days, total evaluation period of the model and the number of steps
            _numberOfDays = KineticModelInstance.NumberOfDays;
            _evaluationPeriod = _numberOfDays * (int)_timeUnitMultiplier;
            _steps = _evaluationPeriod * KineticModelDefinition.EvaluationFrequency;
        }

        /// <summary>
        /// Computes peak target (internal) substance amounts.
        /// </summary>
        public override List<IndividualDaySubstanceTargetExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double relativeCompartmentWeight,
            ProgressState progressState,
            IRandom generator
        ) {
            var individualDayExposureRoutes = individualDayExposures
                .ToDictionary(r => r.SimulatedIndividualDayId, r => new List<IExternalIndividualDayExposure>() { r });
            var targetExposures = calculate(
                individualDayExposureRoutes,
                substance,
                exposureRoutes,
                ExposureType.Acute,
                exposureUnit,
                relativeCompartmentWeight,
                false,
                generator,
                progressState
            );
            var result = individualDayExposures
                .Select((r, i) => {
                    if (targetExposures.TryGetValue(i, out var exposures)) {
                        return new IndividualDaySubstanceTargetExposure() {
                            SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                            SubstanceTargetExposures = exposures
                                .Select(c => c as ISubstanceTargetExposure)
                                .ToList()
                        };
                    } else {
                        return new IndividualDaySubstanceTargetExposure();
                    }
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Override: computes substance internal exposure for the specified individual
        /// day exposure.
        /// </summary>
        public override ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double relativeCompartmentWeight,
            IRandom generator = null
        ) {
            var individualExposureRoutes = new Dictionary<int, List<IExternalIndividualDayExposure>> {
                { 0, new List<IExternalIndividualDayExposure> { externalIndividualDayExposure } }
            };
            var exposureRoutes = new List<ExposurePathType>() { exposureRoute };
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
        public override Dictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var individualExposures = aggregateIndividualExposures
                .Cast<IExternalIndividualExposure>()
                .ToList();
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                individualExposures,
                KineticModelInstance.Substances.First(),
                _modelInputDefinitions.Keys,
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
        public override Dictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var individualDayExposures = aggregateIndividualDayExposures.Cast<IExternalIndividualDayExposure>().ToList();
            //TODO fix exposure route for SBML (Oral), for Cosmos it is Dietary.
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                individualDayExposures,
                KineticModelInstance.Substances.First(),
                exposureUnit,
                _modelInputDefinitions.Keys
            );
            return computeAbsorptionFactors(
                substance,
                exposureRoutes,
                exposurePerRoutes,
                ExposureType.Acute,
                exposureUnit,
                nominalBodyWeight,
                generator
            );
        }

        /// <summary>
        /// Override
        /// </summary>
        public override double CalculateTargetDose(
            double dose,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            double relativeCompartmentWeight,
            IRandom generator = null
        ) {
            var individual = new Individual(0) {
                BodyWeight = bodyWeight
            };
            var externalExposure = ExternalIndividualDayExposure
                .FromSingleDose(
                    exposureRoute,
                    substance,
                    dose,
                    exposureUnit,
                    individual
                );
            var internalExposure = CalculateInternalDoseTimeCourse(
                externalExposure,
                substance,
                exposureRoute,
                exposureType,
                exposureUnit,
                relativeCompartmentWeight,
                generator
            );
            var targetDose = exposureUnit.IsPerBodyWeight()
                ? internalExposure.SubstanceAmount / (bodyWeight * relativeCompartmentWeight)
                : internalExposure.SubstanceAmount;
            return targetDose;
        }

        /// <summary>
        /// Override: uses bisection search to find the external dose corresponding to 
        /// the specified internal dose. The kinetic model is applied using nominal 
        /// values (i.e., without variability).
        /// </summary>
        public override double Reverse(
            double dose,
            Compound substance,
            ExposurePathType externalExposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            double relativeCompartmentWeight,
            IRandom generator
        ) {
            var precision = 0.001;
            var xLower = 10E-6 * dose;
            var xUpper = 10E6 * dose;
            var xMiddle = double.NaN;
            for (var i = 0; i < 1000; i++) {
                xMiddle = (xLower + xUpper) / 2;
                var fMiddle = CalculateTargetDose(
                    xMiddle,
                    substance,
                    externalExposureRoute,
                    exposureType,
                    exposureUnit,
                    bodyWeight,
                    relativeCompartmentWeight,
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
        /// Override: computes long term target (internal) substance amounts.
        /// </summary>
        public override List<IndividualSubstanceTargetExposure> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double relativeCompartmentWeight,
            ProgressState progressState,
            IRandom generator
        ) {
            var individualExposureRoutes = individualExposures
                .ToDictionary(r => r.SimulatedIndividualId, r => r.ExternalIndividualDayExposures);
            // Contains for all individuals the exposure pattern.
            var targetExposures = calculate(
                individualExposureRoutes,
                substance,
                exposureRoutes,
                ExposureType.Chronic,
                exposureUnit,
                relativeCompartmentWeight,
                false,
                generator,
                progressState
            );
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
        /// Restore output compartments of the selected biological matrix to the output variables in kinetic model
        /// Currently the order of the substance compartments is dependent on the order in the XML.
        /// It is assumed that the Parent (P) is the first one followed by the metabolites (M1, M2 etc)
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, (Compound, KineticModelOutputDefinition)> getSubstanceOutputMappings() {
            var result = new Dictionary<string, (Compound, KineticModelOutputDefinition)>();
            var output = _modelOutputDefinitions[KineticModelInstance.CodeCompartment];
            if (output.Substances?.Any() ?? false) {
                if (output.Substances.Any()) {
                    if (KineticModelInstance.KineticModelDefinition.Format == PbkImplementationFormat.DeSolve) {
                        foreach (var substanceCode in output.Substances) {
                            var instance = KineticModelInstance.KineticModelSubstances
                                .Single(c => c.SubstanceDefinition.Id == substanceCode);
                            var outputIdentifier = $"{KineticModelInstance.CodeCompartment}_{substanceCode}";
                            result.Add(outputIdentifier, (instance.Substance, output));
                        }
                    } else {
                        foreach (var substanceCode in output.Substances) {
                            result.Add(substanceCode, (KineticModelInstance.Substances.First(), output));
                        }
                    }
                } else {
                    result.Add(KineticModelInstance.CodeCompartment, (KineticModelInstance.Substances.First(), output));
                }
            } else {
                result.Add(KineticModelInstance.CodeCompartment, (KineticModelInstance.Substances.First(), output));
            }
            return result;
        }

        /// <summary>
        /// Runs the PBK model for the provided external individual exposures.
        /// </summary>
        protected abstract Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double relativeCompartmentWeight,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        );

        /// <summary>
        /// Calculates exposure for routes not supported by PBK model using
        /// fallback absorption factors.
        /// </summary>
        protected double calculateUnforcedRoutesSubstanceAmount(
            List<IExternalIndividualDayExposure> exposuresByRoute,
            Compound substance,
            ICollection<ExposurePathType> unforcedRoutes,
            double relativeCompartmentWeight
        ) {
            var unforcedRoutesSubstanceAmount = 0d;
            foreach (var route in unforcedRoutes) {
                unforcedRoutesSubstanceAmount += _absorptionFactors[route] * relativeCompartmentWeight * getRouteSubstanceIndividualDayExposures(exposuresByRoute, substance, route).Average();
            }
            return unforcedRoutesSubstanceAmount;
        }

        private Dictionary<ExposurePathType, double> computeAbsorptionFactors(
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<ExposurePathType, double> exposurePerRoutes,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var absorptionFactors = new Dictionary<ExposurePathType, double>();
            var relativeCompartmentWeight = GetNominalRelativeCompartmentWeight();
            foreach (var route in exposureRoutes) {
                if (exposurePerRoutes.Keys.Contains(route)) {
                    var internalDose = CalculateTargetDose(
                        exposurePerRoutes[route],
                        substance,
                        route,
                        exposureType,
                        exposureUnit,
                        nominalBodyWeight,
                        relativeCompartmentWeight,
                        generator
                    );
                    absorptionFactors[route] = exposureUnit.IsPerBodyWeight()
                        ? internalDose / exposurePerRoutes[route]
                        : 1 / relativeCompartmentWeight * (internalDose / exposurePerRoutes[route]);
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
            if (!string.IsNullOrEmpty(KineticModelDefinition.IdGenderParameter)) {
                // TODO: implicit assumption of Female = 1, Male = 2 should become explicit
                var sex = individual.GetGender();
                if (sex == GenderType.Undefined) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdGenderParameter, out var paramValue)
                        && !double.IsNaN(paramValue.Value)
                    ) {
                        // Fallback on age from kinetic model parametrisation
                        sex = (GenderType)paramValue.Value;
                    } else if (_modelParameterDefinitions[KineticModelDefinition.IdGenderParameter].DefaultValue.HasValue) {
                        // Fallback on default age from kinetic model definition
                        sex = (GenderType)_modelParameterDefinitions[KineticModelDefinition.IdGenderParameter].DefaultValue;
                    } else {
                        throw new Exception($"Cannot set required parameter for sex for PBK model [{KineticModelDefinition.Name}].");
                    }
                }
                physiologicalParameters[KineticModelDefinition.IdGenderParameter] = (double)sex;
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
                case ExposurePathType.Dietary:
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposurePathType.Oral:
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDayNonDietaryOral));
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
                    case ExposurePathType.Dietary:
                        return getAllEvents(KineticModelInstance.SelectedEvents, timeMultiplier, numberOfDays);
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
                    case ExposurePathType.Dietary:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDay, timeMultiplier, numberOfDays);
                    case ExposurePathType.Oral:
                        return getAllEvents(KineticModelInstance.NumberOfDosesPerDayNonDietaryOral, timeMultiplier, numberOfDays);
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
    }
}
