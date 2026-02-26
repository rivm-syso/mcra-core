using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed class ConsumerProductExposureCalculator {
        private readonly IDictionary<(ConsumerProduct, Compound), ConcentrationModel> _concentrationModels;
        private readonly Dictionary<(ConsumerProduct, Compound, ExposureRoute), double> _exposureFractions;
        private readonly Dictionary<ConsumerProduct, ConsumerProductApplicationAmountSGs> _applicationAmounts;

        public ConsumerProductExposureCalculator(
            ICollection<ConsumerProductExposureFraction> cpExposureFractions,
            ICollection<ConsumerProductApplicationAmount> cpApplicationAmounts,
            IDictionary<(ConsumerProduct, Compound), ConcentrationModel> cpConcentrationModels
        ) {
            _concentrationModels = cpConcentrationModels;
            _exposureFractions = cpExposureFractions.ToDictionary(c => (c.Product, c.Substance, c.Route), c => c.ExposureFraction);
            _applicationAmounts = cpApplicationAmounts
                 .GroupBy(c => c.Product)
                 .Select(c => {
                     var subgroups = c.Where(r => r.AgeLower.HasValue || r.Sex != GenderType.Undefined).ToList();
                     if (c.Select(c => c.DistributionType).Distinct().Count() > 1) {
                         throw new Exception($"For product {c.Key.Name} {c.Key.Code} more than one distribution types are defined.");
                     }
                     var fallbackApplicationAmount = c.FirstOrDefault(r => !r.AgeLower.HasValue && r.Sex == GenderType.Undefined);
                     return new ConsumerProductApplicationAmountSGs() {
                         Product = c.Key,
                         Amount = fallbackApplicationAmount?.Amount,
                         AmountUnit = fallbackApplicationAmount != null ? fallbackApplicationAmount.Unit : c.First().Unit,
                         CvVariability = fallbackApplicationAmount?.CvVariability,
                         DistributionType = fallbackApplicationAmount?.DistributionType ?? c.First().DistributionType,
                         CPAASubgroups = subgroups
                     };
                 })
                 .ToDictionary(c => c.Product);
        }

        public List<ConsumerProductIndividualExposure> Compute(
            ICollection<SimulatedIndividual> individuals,
            ICollection<IndividualConsumerProductUseFrequency> cpUseFrequencies,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            ExposureUnitTriple targetUnit
        ) {
            var useFrequencyLookup = cpUseFrequencies.ToLookup(r => r.Individual);
            var cpIndividualDayExposures = individuals
                .Select(si => calculateIndividualDayExposure(
                    si,
                    useFrequencyLookup.Contains(si.Individual) ? [.. useFrequencyLookup[si.Individual]] : [],
                    routes,
                    substances,
                    targetUnit,
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
            ExposureUnitTriple targetUnit,
            IRandom random
        ) {
            var intakesPerConsumerProduct = new List<IIntakePerConsumerProduct>(useFrequencies.Count);
            foreach (var useFrequency in useFrequencies) {
                IConsumerProductApplicationAmountModel model = null;
                if (_applicationAmounts.TryGetValue(useFrequency.Product, out var cpApplicationAmount)) {
                    model = ConsumerProductApplicationAmountCalculatorFactory.Create(cpApplicationAmount);
                }
                var intakesPerRouteSubstance = new Dictionary<ExposureRoute, List<IIntakePerCompound>>();
                foreach (var route in routes) {
                    var exposuresPerSubstance = new List<ConsumerProductExposurePerSubstance>();
                    foreach (var substance in substances) {
                        if (model != null
                            && _exposureFractions.TryGetValue((useFrequency.Product, substance, route), out var cpFraction)
                            && _concentrationModels.TryGetValue((useFrequency.Product, substance), out var cpcModel)
                        ) {
                            (var concentration, var occurrenceFraction) = getConcentration(substance, cpcModel);
                            var applicationAmount = model.Draw(random, useFrequency.Individual.Age, useFrequency.Individual.Gender);
                            var alignmentFactor = cpApplicationAmount.AmountUnit.GetMultiplicationFactor(targetUnit.ConcentrationMassUnit);
                            var ipc = new ConsumerProductExposurePerSubstance() {
                                UseAmount = useFrequency.Frequency * (double)applicationAmount * cpFraction,
                                Concentration = concentration * occurrenceFraction * alignmentFactor,
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

        private static (double, double) getConcentration(Compound substance, ConcentrationModel model) {
            var concentration = model.GetDistributionMean(NonDetectsHandlingMethod.ReplaceByZero);
            var occurrencePercentage = model.CorrectedOccurenceFraction;
            return (concentration, occurrencePercentage);
        }
    }
}
