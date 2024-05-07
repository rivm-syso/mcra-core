using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public abstract class PbkModelCalculatorBase : IKineticModelCalculator {

        protected readonly IDictionary<ExposurePathType, double> _absorptionFactors;

        // Model instance
        public KineticModelInstance KineticModelInstance { get; }
        public Compound Substance => KineticModelInstance.InputSubstance;
        public List<Compound> OutputSubstances => KineticModelInstance.Substances.ToList();

        // Model definition
        public KineticModelDefinition KineticModelDefinition => KineticModelInstance.KineticModelDefinition;
        protected IDictionary<string, KineticModelOutputDefinition> _modelOutputs;
        protected Dictionary<string, (Compound substance, KineticModelOutputDefinition outputDef)> _outputSubstancesMappings;
        protected IDictionary<ExposurePathType, KineticModelInputDefinition> _modelInputDefinitions;
        protected Dictionary<string, KineticModelOutputDefinition> _modelOutputDefinitions;
        protected IDictionary<string, KineticModelParameterDefinition> _modelParameterDefinitions;
        protected IDictionary<string, KineticModelOutputDefinition> _selectedModelOutputDefinition;

        // Run/simulation settings
        protected readonly double _timeUnitMultiplier;
        protected readonly int _numberOfDays;
        protected readonly int _evaluationPeriod;
        protected readonly int _steps;

        public PbkModelCalculatorBase(
            KineticModelInstance kineticModelInstance,
            IDictionary<ExposurePathType, double> defaultAbsorptionFactors
        ) {
            _absorptionFactors = defaultAbsorptionFactors;
            KineticModelInstance = kineticModelInstance;

            // Lookups/dictionaries for model definition elements
            _modelInputDefinitions = KineticModelDefinition.Forcings
                .ToDictionary(r => r.Route);
            _modelParameterDefinitions = KineticModelDefinition.Parameters
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
            _modelOutputDefinitions = KineticModelDefinition.Outputs
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
            _modelOutputs = KineticModelDefinition.GetModelOutputs();
            _selectedModelOutputDefinition = kineticModelInstance.CompartmentCodes
                .ToDictionary(code => code, c => _modelOutputDefinitions[c]);
            _outputSubstancesMappings = getSubstanceOutputMappings();

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
        public List<IndividualDayTargetExposureCollection> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var individualDayExposureRoutes = individualDayExposures
                .ToDictionary(
                    r => r.SimulatedIndividualDayId, 
                    r => new List<IExternalIndividualDayExposure>() { r }
                );
            var targetExposures = calculate(
                individualDayExposureRoutes,
                Substance,
                exposureRoutes,
                ExposureType.Acute,
                exposureUnit,
                false,
                generator,
                progressState
            );

            //TODO, relative compartments weights are NaN when SBML model is not run (no exposures), therefor skip these values
            var compartments = targetExposures.SelectMany(c => c.Value)
                .Where(c => !double.IsNaN(c.CompartmentInfo.relativeCompartmentWeight))
                .Select(c => c.CompartmentInfo)
                .GroupBy(c => c.compartment)
                .Select(c => (compartment: c.Key, relativeCompartmentWeight: c.Average(r => r.relativeCompartmentWeight)))
                .ToList();

            var collections = new List<IndividualDayTargetExposureCollection>();
            foreach (var (compartment, relativeCompartmentWeight) in compartments) {
                var result = individualDayExposures
                    .Select((r, i) => {
                        if (targetExposures.TryGetValue(i, out var exposures)) {
                            return new IndividualDaySubstanceTargetExposure() {
                                SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                                SubstanceTargetExposures = exposures.Where(c => c.CompartmentInfo.compartment == compartment)
                                    .Select(c => (ISubstanceTargetExposure)c)
                                    .ToList()
                            };
                        } else {
                            return new IndividualDaySubstanceTargetExposure();
                        }
                    })
                    .ToList();

                var targetCollection = new IndividualDayTargetExposureCollection {
                    Compartment = compartment,
                    TargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, BiologicalMatrixConverter.FromString(compartment)),
                    IndividualDaySubstanceTargetExposures = result
                };
                collections.Add(targetCollection);
            }
            return collections;
        }


        /// <summary>
        /// Override: computes long term target (internal) substance amounts.
        /// </summary>
        public List<IndividualTargetExposureCollection> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var individualExposureRoutes = individualExposures
                .ToDictionary(r => r.SimulatedIndividualId, r => r.ExternalIndividualDayExposures);
            // Contains for all individuals the exposure pattern.
            var targetExposures = calculate(
                individualExposureRoutes,
                Substance,
                exposureRoutes,
                ExposureType.Chronic,
                exposureUnit,
                false,
                generator,
                progressState
            );
            var collections = new List<IndividualTargetExposureCollection>();
            //TODO, relative compartments weights are NaN when SBML model is not run (no exposures), therefor skip these values
            var compartments = targetExposures.SelectMany(c => c.Value)
                .Where(c => !double.IsNaN(c.CompartmentInfo.relativeCompartmentWeight))
                .Select(c => c.CompartmentInfo)
                .GroupBy(c => c.compartment)
                .Select(c => (compartment: c.Key, relativeCompartmentWeight: c.Average(r => r.relativeCompartmentWeight)))
                .ToList();
            foreach (var (compartment, relativeCompartmentWeight) in compartments) {
                var result = individualExposures
                    .Select((r, i) => {
                        if (targetExposures.TryGetValue(i, out var exposures)) {
                            return new IndividualSubstanceTargetExposure() {
                                Individual = r.Individual,
                                SimulatedIndividualId = r.SimulatedIndividualId,
                                IndividualSamplingWeight = r.IndividualSamplingWeight,
                                SubstanceTargetExposures = exposures.Where(c => c.CompartmentInfo.compartment == compartment)
                                    .Select(c => (ISubstanceTargetExposure)c)
                                    .ToList()
                            };
                        } else {
                            return new IndividualSubstanceTargetExposure();
                        }
                    })
                    .ToList();
                var targetCollection = new IndividualTargetExposureCollection {
                    Compartment = compartment,
                    TargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, BiologicalMatrixConverter.FromString(compartment)),
                    IndividualSubstanceTargetExposures = result
                };
                collections.Add(targetCollection);
            }
            return collections;
        }

        /// <summary>
        /// Override: computes substance internal exposure for the specified individual
        /// day exposure.
        /// </summary>
        public ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            IRandom generator = null
        ) {
            var individualExposureRoutes = new Dictionary<int, List<IExternalIndividualDayExposure>> {
                { 0, new List<IExternalIndividualDayExposure> { externalIndividualDayExposure } }
            };

            var exposureRoutes = new List<ExposurePathType>() { exposureRoute };
            var internalExposures = calculate(
                individualExposureRoutes,
                Substance,
                exposureRoutes,
                exposureType,
                exposureUnit,
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
        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
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
                Substance,
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
        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
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
                Substance,
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
        public double CalculateTargetDose(
            double dose,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IRandom generator = null
        ) {
            var individual = new Individual(0) {
                BodyWeight = bodyWeight
            };
            var externalExposure = ExternalIndividualDayExposure
                .FromSingleDose(
                    exposureRoute,
                    Substance,
                    dose,
                    exposureUnit,
                    individual
                );
            var internalExposure = CalculateInternalDoseTimeCourse(
                externalExposure,
                exposureRoute,
                exposureType,
                exposureUnit,
                generator
            );
            //TODO, needs further implementation
            var relativeCompartmentWeights = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);
            var relativeCompartmentWeight = relativeCompartmentWeights.First().Value;
            var targetDose = exposureUnit.IsPerBodyWeight()
                ? internalExposure.SubstanceAmount / (bodyWeight * relativeCompartmentWeight)
                : internalExposure.SubstanceAmount;
            return targetDose;
        }

        protected virtual ICollection<(string compartment, double weight)> GetNominalRelativeCompartmentWeights() {
            //TODO, this needs further implementation. Not correct for combinations of kinetic model instances, should be the reference substance
            var result = new List<(string, double)> { (string.Empty, 1D) };
            return result;
        }

        /// <summary>
        /// Override: uses bisection search to find the external dose corresponding to 
        /// the specified internal dose. The kinetic model is applied using nominal 
        /// values (i.e., without variability).
        /// </summary>
        public double Reverse(
            double dose,
            ExposurePathType externalExposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
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
                    externalExposureRoute,
                    exposureType,
                    exposureUnit,
                    bodyWeight,
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
        /// Restore output compartments of the selected biological matrix to the output variables in kinetic model
        /// Currently the order of the substance compartments is dependent on the order in the XML.
        /// It is assumed that the Parent (P) is the first one followed by the metabolites (M1, M2 etc)
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, (Compound, KineticModelOutputDefinition)> getSubstanceOutputMappings() {
            var result = new Dictionary<string, (Compound, KineticModelOutputDefinition)>();

            var codeCompartments = _selectedModelOutputDefinition.Keys.ToList();
            foreach (var KineticModelInstanceCodeCompartment in codeCompartments) {
                var output = _modelOutputDefinitions[KineticModelInstanceCodeCompartment];
                if (output.Substances?.Any() ?? false) {
                    if (output.Substances.Any()) {
                        if (KineticModelInstance.KineticModelDefinition.Format == PbkImplementationFormat.DeSolve) {
                            foreach (var compartmentCode in output.Substances) {
                                var instance = KineticModelInstance.KineticModelSubstances
                                    .Single(c => c.SubstanceDefinition.Id == compartmentCode);
                                var outputIdentifier = $"{KineticModelInstanceCodeCompartment}_{compartmentCode}";
                                result.Add(outputIdentifier, (instance.Substance, output));
                            }
                        } else {
                            foreach (var compartmentCode in output.Substances) {
                                result.Add(compartmentCode, (KineticModelInstance.Substances.First(), output));
                            }
                        }
                    } else {
                        result.Add(KineticModelInstanceCodeCompartment, (KineticModelInstance.Substances.First(), output));
                    }
                } else {
                    result.Add(KineticModelInstanceCodeCompartment, (KineticModelInstance.Substances.First(), output));
                }
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
                unforcedRoutesSubstanceAmount += _absorptionFactors[route] 
                    * relativeCompartmentWeight
                    * getRouteSubstanceIndividualDayExposures(exposuresByRoute, substance, route).Average();
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
            //TODO
            var relativeCompartmentWeight = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);
            foreach (var route in exposureRoutes) {
                if (exposurePerRoutes.ContainsKey(route)) {
                    var internalDose = CalculateTargetDose(
                        exposurePerRoutes.First().Value,
                        route,
                        exposureType,
                        exposureUnit,
                        nominalBodyWeight,
                        generator
                    );
                    //TODO, needs further implementation
                    absorptionFactors[route] = exposureUnit.IsPerBodyWeight()
                        ? internalDose / exposurePerRoutes.First().Value
                        : 1 / relativeCompartmentWeight.First().Value * (internalDose / exposurePerRoutes[route]);
                } else {
                    absorptionFactors.Add(route, _absorptionFactors.First().Value);
                }
            }
            return absorptionFactors;
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
