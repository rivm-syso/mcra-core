using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.ReverseDoseCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionCalculation {
    public class PbkKineticConversionCalculator : IKineticConversionCalculator {

        public KineticConversionModelType ModelType => KineticConversionModelType.PbkModel;

        private readonly IPbkModelCalculator _pbkModelCalculator;

        private readonly PbkSimulationSettings _simulationSettings;

        public Compound Substance => _pbkModelCalculator.Substance;

        public KineticModelInstance KineticModelInstance => _pbkModelCalculator.KineticModelInstance;

        public List<Compound> OutputSubstances => _pbkModelCalculator.OutputSubstances;

        public PbkKineticConversionCalculator(
            IPbkModelCalculator pbkModelCalculator
        ) {
            _pbkModelCalculator = pbkModelCalculator;
            _simulationSettings = pbkModelCalculator.SimulationSettings;
        }

        public PbkKineticConversionCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) {
            _pbkModelCalculator = PbkModelCalculatorFactory.Create(kineticModelInstance, simulationSettings);
            _simulationSettings = simulationSettings;
        }

        /// <summary>
        /// Computes peak target (internal) substance concentrations.
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
                .Select(r => (r.SimulatedIndividual, new List<IExternalIndividualDayExposure>() { r }))
                .ToList();

            var targetExposures = _pbkModelCalculator
                .Calculate(
                    externalIndividualExposures,
                    exposureUnit,
                    routes,
                    targetUnits,
                    ExposureType.Acute,
                    generator,
                    progressState
                );

            var result = new List<AggregateIndividualDayExposure>();
            for (int i = 0; i < individualDayExposures.Count; i++) {
                var externalIndividualExposure = individualDayExposures.ElementAt(i);
                var targetIndividualExposure = ComputeSubstanceTargetExposures(
                    targetExposures[i],
                    ExposureType.Acute,
                    _simulationSettings.NonStationaryPeriod
                );
                var internalIndividualExposure = new AggregateIndividualDayExposure() {
                    SimulatedIndividual = externalIndividualExposure.SimulatedIndividual,
                    SimulatedIndividualDayId = externalIndividualExposure.SimulatedIndividualDayId,
                    Day = externalIndividualExposure.Day,
                    ExternalIndividualDayExposures = [externalIndividualExposure],
                    InternalTargetExposures = targetIndividualExposure
                };
                result.Add(internalIndividualExposure);
            }
            return result;
        }

        /// <summary>
        /// Computes long term target (internal) substance concentrations.
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
                .Select(r => (r.SimulatedIndividual, r.ExternalIndividualDayExposures))
                .ToList();

            // Contains for all individuals the exposure pattern.
            var targetExposures = _pbkModelCalculator.Calculate(
                externalIndividualExposures,
                exposureUnit,
                routes,
                targetUnits,
                ExposureType.Chronic,
                generator,
                progressState
            );

            var result = new List<AggregateIndividualExposure>();
            for (int i = 0; i < individualExposures.Count; i++) {
                var externalIndividualExposure = individualExposures.ElementAt(i);
                var targetIndividualExposure = ComputeSubstanceTargetExposures(
                    targetExposures[i],
                    ExposureType.Chronic,
                    _simulationSettings.NonStationaryPeriod
                );
                var internalIndividualExposure = new AggregateIndividualExposure() {
                    SimulatedIndividual = externalIndividualExposure.SimulatedIndividual,
                    ExternalIndividualDayExposures = externalIndividualExposure.ExternalIndividualDayExposures,
                    InternalTargetExposures = targetIndividualExposure
                };
                result.Add(internalIndividualExposure);
            }
            return result;
        }

        public static Dictionary<ExposureTarget, Dictionary<Compound, ISubstanceTargetExposure>> ComputeSubstanceTargetExposures(
            PbkSimulationOutput simulationOutput,
            ExposureType exposureType,
            int nonStationaryPeriod
        ) {
            var result = simulationOutput.SubstanceTargetLevelTimeSeries
                .Select(r => {
                    var steadyStateTargetExposure = r.ComputeSteadyStateTargetExposure(
                        nonStationaryPeriod
                    );
                    var peakTargetExposure = r.ComputePeakTargetExposure(
                        nonStationaryPeriod
                    );
                    return new SubstanceTargetExposurePattern() {
                        TimeSeries = r,
                        SteadyStateTargetExposure = steadyStateTargetExposure,
                        PeakTargetExposure = peakTargetExposure,
                        Exposure = exposureType == ExposureType.Acute
                            ? peakTargetExposure
                            : steadyStateTargetExposure
                    };
                })
                .GroupBy(r => r.Target)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .ToDictionary(
                            r => r.Substance,
                            r => r as ISubstanceTargetExposure
                        )
                );
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
            var individualExposureRoutes = new List<(SimulatedIndividual, List<IExternalIndividualDayExposure>)> {
                (externalIndividualDayExposure.SimulatedIndividual,
                new List<IExternalIndividualDayExposure> { externalIndividualDayExposure })
            };
            var targetExposures = _pbkModelCalculator.Calculate(
                individualExposureRoutes,
                exposureUnit,
                [route],
                [targetUnit],
                exposureType,
                generator,
                new ProgressState()
            );
            var targetIndividualExposure = ComputeSubstanceTargetExposures(
                targetExposures.First(),
                exposureType,
                _simulationSettings.NonStationaryPeriod
            );
            return targetIndividualExposure[targetUnit.Target].Values.First();
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
        public List<KineticConversionFactorResultRecord> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                externalIndividualExposures,
                _pbkModelCalculator.KineticModelInstance.Substances.First(),
                routes,
                exposureUnit
            );
            // TODO: not the right place to compute average
            // exposures per route and define nominal individual.
            var individual = new Individual(0) {
                BodyWeight = externalIndividualExposures.Average(c => c.SimulatedIndividual.BodyWeight),
            };
            individual.SetPropertyValue(
                property: new() {
                    PropertyType = IndividualPropertyType.Numeric,
                    Name = "Age"
                },
                doubleValue: externalIndividualExposures.Average(c => c.SimulatedIndividual.Age)
            );

            return computeAbsorptionFactors(
                Substance,
                new(individual, 0),
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
        public List<KineticConversionFactorResultRecord> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var exposurePerRoutes = computeAverageSubstanceExposurePerRoute(
                externalIndividualDayExposures,
                _pbkModelCalculator.KineticModelInstance.Substances.First(),
                exposureUnit,
                routes
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
        /// Override: find the external dose corresponding to the specified internal dose.
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
            var reverseDoseCalculator = new ReverseDoseCalculator() {
                Precision = _simulationSettings.PrecisionReverseDoseCalculation,
            };
            return reverseDoseCalculator.Reverse(
                _pbkModelCalculator,
                individual,
                dose,
                internalDoseUnit,
                externalExposureRoute,
                externalExposureUnit,
                exposureType,
                generator
            );
        }

        private List<KineticConversionFactorResultRecord> computeAbsorptionFactors(
            Compound substance,
            SimulatedIndividual individual,
            ICollection<ExposureRoute> routes,
            IDictionary<ExposureRoute, double> exposurePerRoutes,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new List<KineticConversionFactorResultRecord>();
            foreach (var route in routes) {
                double factor = double.NaN;
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
                    factor = internalDose / externalDose;
                }
                result.Add(new KineticConversionFactorResultRecord() {
                    ExposureRoute = route,
                    Substance = substance,
                    ExternalExposureUnit = exposureUnit,
                    TargetUnit = targetUnit,
                    Factor = factor
                });
            }
            return result;
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
                    var sumOfSamplingWeights = externalIndividualExposures.Sum(c => c.SimulatedIndividual.SamplingWeight);
                    var exposures = positives
                       .Select(r => r.GetExposure(route, substance, isPerPerson: true) * r.SimulatedIndividual.SamplingWeight)
                       .ToList();
                    var exposure = exposures.Sum() / sumOfSamplingWeights;
                    if (exposureUnit.IsPerBodyWeight) {
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
                    if (exposureUnit.IsPerBodyWeight) {
                        exposure = exposure / averageBodyWeight;
                    }
                    exposurePerRoute.Add(route, exposure);
                }
            }
            return exposurePerRoute;
        }
    }
}
