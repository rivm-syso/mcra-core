using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation;
using MCRA.Simulation.Calculators.ConsumerProductExposureFractionCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed class ConsumerProductExposureCalculator {
        private static readonly ConsumerProduct _sumOfConsumerProducts = new() { Code = "Sum", Name = "Sum of consumer products" };

        private readonly IDictionary<(ConsumerProduct, Compound), ConcentrationModel> _concentrationModels;
        private readonly Dictionary<(ConsumerProduct, Compound, ExposureRoute), ConsumerProductExposureFractionSGs> _exposureFractions;
        private readonly Dictionary<ConsumerProduct, ConsumerProductApplicationAmountSGs> _applicationAmounts;

        public ConsumerProductExposureCalculator() { }

        public ConsumerProductExposureCalculator(
            ICollection<ConsumerProductExposureFraction> cpExposureFractions,
            ICollection<ConsumerProductApplicationAmount> cpApplicationAmounts,
            IDictionary<(ConsumerProduct, Compound), ConcentrationModel> cpConcentrationModels
        ) {
            _concentrationModels = cpConcentrationModels;
            _exposureFractions = cpExposureFractions
                .GroupBy(c => (p: c.Product, s: c.Substance, r: c.Route))
                .Select(c => {
                    var subgroups = c.Where(r => r.AgeLower.HasValue || r.Sex != GenderType.Undefined).ToList();
                    var fallbackApplicationAmount = c.FirstOrDefault(r => !r.AgeLower.HasValue && r.Sex == GenderType.Undefined);
                    return new ConsumerProductExposureFractionSGs() {
                        Product = c.Key.p,
                        Substance = c.Key.s,
                        Route = c.Key.r,
                        ExposureFraction = fallbackApplicationAmount?.ExposureFraction,
                        CPEFSubgroups = subgroups
                    };
                })
                .ToDictionary(c => (c.Product, c.Substance, c.Route));


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

        /// <summary>
        /// Compute individual exposures based on uses and concentrations.
        /// </summary>
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

        /// <summary>
        /// Compute exposure based on distributions.
        /// </summary>
        public List<ConsumerProductIndividualExposure> Compute(
            ICollection<SimulatedIndividual> individuals,
            ICollection<ConsumerProductExposureDistribution> models,
            ICollection<ExposureRoute> routes,
            ConsumerProductAssessmentMatchingMethod filterMethod,
            ICollection<Compound> substances,
            ExposureUnitTriple targetUnit,
            IRandom random
        ) {
            var lookup = models.ToLookup(c => (c.ExposureRoute, c.Substance));
            var seed = random.Next();
            var exposures = individuals
                .Select(si => computeFromExposureDistributionModels(
                    si,
                    routes,
                    substances,
                    lookup,
                    filterMethod,
                    targetUnit,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, si.Id))
                ))
                .ToList();
            return exposures;
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
                        if (_exposureFractions.TryGetValue((useFrequency.Product, substance, route), out var cpExposureFraction)
                            && _concentrationModels.TryGetValue((useFrequency.Product, substance), out var cpcModel)
                        ) {
                            var exposureFractionModel = new ConsumerProductExposureFractionResolver(cpExposureFraction);
                            ConsumerProductExposurePerSubstance ipc = null;
                            (var concentration, var occurrenceFraction) = getConcentration(cpcModel);
                            var cpUnitSpec = cpcModel.Unit as ConsumerProductConcentrationUnitSpec;
                            if (cpUnitSpec.IsPerUnit()) {
                                // example:
                                // use frequency = 4 cigarattes per day
                                // product concentration = 0.00139 mg/unit
                                // target unit = ug/day
                                // Intake = 4 unit/day * 0.00139 mg/unit * 1000 ug/mg = 5.56 ug/day
                                var exposureFraction = exposureFractionModel.ExposureFraction.ExposureFraction.Value;
                                var alignmentFactor = cpUnitSpec.Unit.GetSubstanceAmountUnit()
                                    .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, substance.MolecularMass);
                                ipc = new ConsumerProductExposurePerSubstance(
                                    substance,
                                    Amount: useFrequency.Frequency * concentration * occurrenceFraction
                                         * exposureFraction * alignmentFactor
                                    );
                            } else if (model != null) {
                                // example:
                                // application amount = 0.00328 g
                                // substance concentration = 0.482 mg/kg
                                // target unit = ug/day
                                // intake = 0.00328 g * 0.001 kg/g * 0.482 mg/kg * 1000 ug/mg = 0.00158 ug
                                var exposureFraction = exposureFractionModel.GetExposureFraction(useFrequency.Individual.Age, useFrequency.Individual.Gender);
                                var applicationAmount = model.Draw(random, useFrequency.Individual.Age, useFrequency.Individual.Gender);
                                var alignmentToConcentration = cpApplicationAmount.AmountUnit
                                    .GetMultiplicationFactor(cpUnitSpec.Unit.GetConcentrationMassUnit());
                                var alignmentToTargetUnit = cpUnitSpec.Unit.GetSubstanceAmountUnit()
                                    .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, substance.MolecularMass);
                                var alignmentFactor = alignmentToConcentration * alignmentToTargetUnit;
                                ipc = new ConsumerProductExposurePerSubstance(
                                    substance,
                                    Amount: useFrequency.Frequency * (double)applicationAmount * concentration
                                        * exposureFraction * occurrenceFraction * alignmentFactor
                                    );
                            }
                            if (ipc != null) {
                                exposuresPerSubstance.Add(ipc);
                            }
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

        /// <summary>
        /// Computes/draws the CP exposure for the specified routes and substances from
        /// exposure distribution models.
        /// <returns></returns>
        private ConsumerProductIndividualExposure computeFromExposureDistributionModels(
            SimulatedIndividual individual,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            ILookup<(ExposureRoute route, Compound substance), ConsumerProductExposureDistribution> models,
            ConsumerProductAssessmentMatchingMethod filterMethod,
            ExposureUnitTriple targetUnit,
            IRandom random
        ) {
            var intakesPerConsumerProduct = new List<IIntakePerConsumerProduct>();
            var intakesPerRouteSubstance = new Dictionary<ExposureRoute, List<IIntakePerCompound>>();

            foreach (var route in routes) {
                var exposuresPerSubstance = new List<ConsumerProductExposurePerSubstance>();
                foreach (var substance in substances) {
                    var model = match(individual, route, substance, models, filterMethod);
                    if (model != null) {
                        var alignmentFactor = model.Unit.GetSubstanceAmountUnit()
                            .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, substance.MolecularMass);
                        var concentration = model.Draw(random);
                        var ipc = new ConsumerProductExposurePerSubstance(
                            substance,
                            Amount: concentration * alignmentFactor
                        );
                        if (ipc != null) {
                            exposuresPerSubstance.Add(ipc);
                        }
                    }
                }
                intakesPerRouteSubstance.Add(route, [.. exposuresPerSubstance.Cast<IIntakePerCompound>()]);
            }
            var intakePerProduct = new IntakePerConsumerProduct() {
                Product = _sumOfConsumerProducts,
                IntakesPerSubstance = intakesPerRouteSubstance,
            };
            intakesPerConsumerProduct.Add(intakePerProduct);
            var consumerProductIndividualDayIntake = new ConsumerProductIndividualExposure() {
                SimulatedIndividual = individual,
                IntakesPerProduct = [.. intakesPerConsumerProduct.Cast<IIntakePerConsumerProduct>()],
            };
            return consumerProductIndividualDayIntake;
        }

        private static (double, double) getConcentration(ConcentrationModel model) {
            var concentration = model.GetDistributionMean(NonDetectsHandlingMethod.ReplaceByZero);
            var occurrencePercentage = model.CorrectedOccurenceFraction;
            return (concentration, occurrencePercentage);
        }

        private static ConsumerProductExposureDistribution match(
            SimulatedIndividual individual,
            ExposureRoute route,
            Compound substance,
            ILookup<(ExposureRoute route, Compound substance), ConsumerProductExposureDistribution> models,
            ConsumerProductAssessmentMatchingMethod matchingMethod
        ) {
            if (models.Contains((route, substance))) {
                var candidates = models[(route, substance)];
                var result = candidates
                    .Where(model => {
                        bool matchesAgeMin = model.AgeMin.HasValue && individual.Age.HasValue && individual.Age >= model.AgeMin.Value;
                        bool matchesAgeMax = model.AgeMax.HasValue && individual.Age.HasValue && individual.Age <= model.AgeMax.Value;
                        bool ageMatches = matchesAgeMin && matchesAgeMax;
                        bool genderMatches = model.Gender == individual.Gender;
                        return matchingMethod switch {
                            ConsumerProductAssessmentMatchingMethod.MatchOnGender => genderMatches,
                            ConsumerProductAssessmentMatchingMethod.MatchOnAge => ageMatches,
                            ConsumerProductAssessmentMatchingMethod.MatchOnAgeAndGender => genderMatches && ageMatches,
                            ConsumerProductAssessmentMatchingMethod.AtRandom => true,
                            _ => false
                        };
                    });
                return result.FirstOrDefault();
            }
            return null;
        }
    }
}
