using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductAplicationAmountCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed class ConsumerProductExposureCalculator {
        private readonly Dictionary<ConsumerProduct, ConsumerProductSampleSubstanceCollections> _concentrationCollections;
        private readonly Dictionary<(ConsumerProduct, Compound), (double concentration, double occurrencePercentage)> _concentrations;
        private readonly Dictionary<(ConsumerProduct, Compound, ExposureRoute), double> _exposureFractions;
        private readonly Dictionary<ConsumerProduct, ConsumerProductApplicationAmount> _applicationAmounts;

        public ConsumerProductExposureCalculator(
            ICollection<ConsumerProductExposureFraction> cpExposureFractions,
            ICollection<ConsumerProductApplicationAmount> cpApplicationAmounts,
            ICollection<ConsumerProductConcentration> cpConcentrations
        ) {
            _concentrationCollections = cpConcentrations
                .AsParallel()
                .GroupBy(c => c.Product)
                .Select(c => new ConsumerProductSampleSubstanceCollections() {
                    ConsumerProduct = c.Key,
                    SubstanceSampleCollection = c.GroupBy(c => c.Substance)
                        .ToDictionary(c => c.Key, c => c.Select(r => r).ToList())
                })
                .ToDictionary(c => c.ConsumerProduct);
            _exposureFractions = cpExposureFractions.ToDictionary(c => (c.Product, c.Substance, c.Route), c => c.ExposureFraction);
            _applicationAmounts = cpApplicationAmounts.ToDictionary(c => c.Product);
            _concentrations = [];
        }

        public List<ConsumerProductIndividualExposure> Compute(
            ICollection<SimulatedIndividual> individuals,
            ICollection<IndividualConsumerProductUseFrequency> cpUseFrequencies,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances
        ) {
            var useFrequencyLookup = cpUseFrequencies.ToLookup(r => r.Individual);
            var cpIndividualDayExposures = individuals
                .Select(si => calculateIndividualDayExposure(
                    si,
                    useFrequencyLookup.Contains(si.Individual) ? [.. useFrequencyLookup[si.Individual]] : [],
                    routes,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(si.Id, (int)RandomSource.CPE_ConsumerProductExposureDeterminants))
                ))
                .ToList();
            return cpIndividualDayExposures;
        }

        private ConsumerProductIndividualExposure calculateIndividualDayExposure(
            SimulatedIndividual individual,
            List<IndividualConsumerProductUseFrequency> useFrequencies,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var intakesPerConsumerProduct = new List<IIntakePerConsumerProduct>(useFrequencies.Count);
            foreach (var useFrequency in useFrequencies) {
                var intakesPerRouteSubstance = new Dictionary<ExposureRoute, List<IIntakePerCompound>>();
                foreach (var route in routes) {
                    var exposuresPerSubstance = new List<ConsumerProductExposurePerSubstance>();
                    foreach (var substance in substances) {
                        if (_applicationAmounts.TryGetValue(useFrequency.Product, out var cpApplication)
                            && _exposureFractions.TryGetValue((useFrequency.Product, substance, route), out var cpFraction)
                            && _concentrationCollections.TryGetValue(useFrequency.Product, out var cpCollection)
                        ) {
                            var distribution = ApplicationAmountProbabilityDistributionFactory.createProbabilityDistribution(cpApplication);
                            double occurrencePercentage;
                            double concentration;
                            if (!_concentrations.TryGetValue((useFrequency.Product, substance), out var cpc)) {
                                (concentration, occurrencePercentage) = getConcentration(substance, cpCollection);
                                _concentrations[(useFrequency.Product, substance)] = (concentration, occurrencePercentage);
                            } else {
                                concentration = cpc.concentration;
                                occurrencePercentage = cpc.occurrencePercentage;
                            }
                            var applicationAmount = cpApplication.DistributionType == ApplicationAmountDistributionType.Constant
                                ? cpApplication.Amount
                                : distribution.Draw(random);
                            var ipc = new ConsumerProductExposurePerSubstance() {
                                UseAmount = useFrequency.Frequency * applicationAmount * cpFraction,
                                Concentration = concentration * occurrencePercentage,
                                Compound = substance
                            };
                            exposuresPerSubstance.Add(ipc);
                        }
                    }
                    intakesPerRouteSubstance.Add(route, [.. exposuresPerSubstance.Cast<IIntakePerCompound>()]);
                }
                var intakePerProduct = new IntakePerConsumerProduct() {
                    Product = useFrequency.Product,
                    IntakesPerSubstance = intakesPerRouteSubstance,
                };
                intakesPerConsumerProduct.Add(intakePerProduct);
            }
            var consumerProductIndividualDayIntake = new ConsumerProductIndividualExposure() {
                SimulatedIndividual = individual,
                IntakesPerProduct = [.. intakesPerConsumerProduct.Cast<IIntakePerConsumerProduct>()],
            };

            return consumerProductIndividualDayIntake;
        }

        private static (double, double) getConcentration(Compound substance, ConsumerProductSampleSubstanceCollections cpc) {
            var concentration = cpc.SubstanceSampleCollection[substance].Average(c => c.Concentration);
            //TODO maybe a separate table is needed for occurence percentages, see issue #2223
            var occurrencePercentage = cpc.SubstanceSampleCollection[substance]
                .Average(c => c.OccurrencePercentage ?? 100d) / 100d;
            return (concentration, occurrencePercentage);
        }
    }
}
