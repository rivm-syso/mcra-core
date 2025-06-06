using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.ConsumerProductCollection;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {
    public sealed class ChronicConsumerProductExposureCalculator {
        private readonly Dictionary<Individual, List<IndividualConsumerProductUseFrequency>> _cpIndividualDayCache;
        private readonly Dictionary<ConsumerProduct, ConsumerProductCollection> _cpConcentrationCollections;
        private readonly Dictionary<(ConsumerProduct, Compound), (double concentration, double occurrencePercentage)> _cpConcentrations;
        private readonly Dictionary<(ConsumerProduct, Compound, ExposureRoute), double> _cpFractions;
        private readonly Dictionary<ConsumerProduct, ConsumerProductApplicationAmount> _cpApplicationAmounts;
        private readonly ICollection<Compound> _substances;
        private readonly ICollection<ExposureRoute> _exposureRoutes;

        public ChronicConsumerProductExposureCalculator(
            ICollection<IndividualConsumerProductUseFrequency> cpUseFrequencies,
            ICollection<ConsumerProductExposureFraction> cpExposureFractions,
            ICollection<ConsumerProductApplicationAmount> cpApplicationAmounts,
            ICollection<ConsumerProductConcentration> cpConcentrations,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes
        ) {
            _exposureRoutes = routes;
            _substances = substances;
            _cpConcentrationCollections = cpConcentrations
                    .AsParallel()
                    .GroupBy(c => c.Product)
                    .Select(c => new ConsumerProductCollection() {
                        ConsumerProduct = c.Key,
                        SubstanceSampleCollection = c.GroupBy(c => c.Substance)
                            .ToDictionary(c => c.Key, c => c.Select(r => r).ToList())
                    }).ToDictionary(c => c.ConsumerProduct);

            _cpIndividualDayCache = new Dictionary<Individual, List<IndividualConsumerProductUseFrequency>>();
            foreach (var c in cpUseFrequencies) {
                if (!_cpIndividualDayCache.TryGetValue(c.Individual, out var useFrequencies)) {
                    _cpIndividualDayCache.Add(c.Individual, [c]);
                } else {
                    useFrequencies.Add(c);
                }
            }
            _cpFractions = cpExposureFractions.ToDictionary(c => (c.Product, c.Substance, c.Route), c => c.ExposureFraction);
            _cpApplicationAmounts = cpApplicationAmounts.ToDictionary(c => c.Product);
            _cpConcentrations = new Dictionary<(ConsumerProduct, Compound), (double concentration, double occurrencePercentage)>();
        }

        public List<ConsumerProductIndividualDayIntake> CalculateConsumerProductExposures(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            ProgressState progressState
        ) {
            var cpIndividualDayExposures = simulatedIndividualDays
                .Select(calculateIndividualDayExposure)
                .ToList();
            return cpIndividualDayExposures;
        }

        private ConsumerProductIndividualDayIntake calculateIndividualDayExposure(SimulatedIndividualDay sid) {
            if (!_cpIndividualDayCache.TryGetValue((sid.SimulatedIndividual.Individual), out var useFrequencies)) {
                useFrequencies = [];
            }

            var intakesPerConsumerProduct = new List<IIntakePerConsumerProduct>(useFrequencies.Count);
            foreach (var useFrequency in useFrequencies) {
                var exposuresPerSubstance = new List<ConsumerProductExposurePerSubstance>();
                foreach (var substance in _substances) {
                    foreach (var route in _exposureRoutes) {
                        if (_cpApplicationAmounts.TryGetValue(useFrequency.Product, out var cpApplicationAmount)
                            && _cpFractions.TryGetValue((useFrequency.Product, substance, route), out var cpFraction)
                            && _cpConcentrationCollections.TryGetValue(useFrequency.Product, out var cpCollection)
                        ) {
                            double concentration;
                            double occurrencePercentage;
                            if (!_cpConcentrations.TryGetValue((useFrequency.Product, substance), out var cpc)) {
                                (concentration, occurrencePercentage) = getConcentration(substance, cpCollection);
                                _cpConcentrations[(useFrequency.Product, substance)] = (concentration, occurrencePercentage);
                            } else {
                                concentration = cpc.concentration;
                                occurrencePercentage = cpc.occurrencePercentage;
                            }
                            var ipc = new ConsumerProductExposurePerSubstance() {
                                IntakePortion = new IntakePortion() {
                                    Amount = useFrequency.Frequency * cpApplicationAmount.Amount * cpFraction,
                                    Concentration = (float)(concentration * occurrencePercentage),
                                },
                                Compound = substance,
                                ExposureRoute = route,
                            };
                            exposuresPerSubstance.Add(ipc);
                        }
                    }
                }
                var intakePerCP = new IntakePerConsumerProduct() {
                    Product = useFrequency.Product,
                    IntakesPerSubstance = [.. exposuresPerSubstance.Cast<IIntakePerCompound>()],
                };
                intakesPerConsumerProduct.Add(intakePerCP);
            }
            var consumerProductIndividualDayIntake = new ConsumerProductIndividualDayIntake() {
                SimulatedIndividualDayId = sid.SimulatedIndividualDayId,
                SimulatedIndividual = sid.SimulatedIndividual,
                Day = sid.Day,
                IntakesPerConsumerProduct = [.. intakesPerConsumerProduct.Cast<IIntakePerConsumerProduct>()],
            };

            return consumerProductIndividualDayIntake;
        }

        private static (double, double) getConcentration(Compound substance, ConsumerProductCollection cpc) {
            var concentration = cpc.SubstanceSampleCollection[substance].Average(c => c.Concentration);
            var occurrencePercentage = cpc.SubstanceSampleCollection[substance]
                .Average(c => c.OccurrencePercentage ?? 100d) / 100d;
            return (concentration, occurrencePercentage);
        }
    }
}
