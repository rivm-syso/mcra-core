using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.BioaccessibilityFractionModels;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.IndividualExposures;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustExposureCalculator {

        public DustExposureCalculator() { }

        /// <summary>
        /// Computes dust exposures for the provided collection of individuals.
        /// </summary>
        public static List<DustIndividualExposure> ComputeDustExposure(
            ICollection<SimulatedIndividual> simulatedIndividuals,
            ICollection<Compound> substances,
            List<ExposureRoute> routes,
            IDictionary<Compound, ConcentrationModel> concentrationModels,
            ICollection<DustIngestion> dustIngestions,
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            ICollection<DustBioaccessibilityFraction> bioaccessibilityFractions,
            ConcentrationUnit dustConcentrationUnit,
            ExternalExposureUnit dustIngestionUnit,
            double timeDustExposure,
            ExposureUnitTriple targetUnit,
            IRandom exposureDeterminantsRandomGenerator
        ) {
            var needsAge = dustIngestions.All(r => r.AgeLower.HasValue)
                || dustAdherenceAmounts.All(r => r.AgeLower.HasValue)
                || dustBodyExposureFractions.All(r => r.AgeLower.HasValue);
            if (needsAge && simulatedIndividuals.Any(i => i.Age == null)) {
                throw new Exception("Missing values for age in individuals.");
            }

            var needsSex = dustIngestions.All(r => r.Sex != GenderType.Undefined)
                || dustAdherenceAmounts.All(r => r.Sex != GenderType.Undefined)
                || dustBodyExposureFractions.All(r => r.Sex != GenderType.Undefined);
            if (needsSex && simulatedIndividuals.Any(i => i.Gender == GenderType.Undefined)) {
                throw new Exception("Missing values for gender in individuals.");
            }

            var needsBsa = routes.Contains(ExposureRoute.Dermal);
            if (needsBsa && simulatedIndividuals.Any(i => !i.BodySurfaceArea.HasValue && !i.Height.HasValue)) {
                throw new Exception("Missing values for body surface area (BSA) in individuals.");
            }

            var availabilityRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());
            var ingestionsRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());
            var adherenceAmountsRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());
            var exposureFractionsRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());
            var concentrationsRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());
            var bioaccessibilityRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());

            // Note: we use grams for expressing dust amounts;
            // - we align concentrations so that they are expressed per gram dust;
            // - we align dust ingestions so that these are expressed as gram dust per day;
            // - we align dermal contact so that it is expressed as gram dust per surface area per day.
            var concentrationAmountFactor = dustConcentrationUnit.GetSubstanceAmountUnit()
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var concentrationMassFactor = dustConcentrationUnit.GetConcentrationMassUnit()
                .GetMultiplicationFactor(ConcentrationMassUnit.Grams);
            var concentrationAlignmentFactor = concentrationAmountFactor * concentrationMassFactor;

            // Compute availability fractions for ingestion exposure
            var substanceDustAvailabilityFraction = routes.Contains(ExposureRoute.Dermal)
                ? calculateSubstanceDustAvailabilityFraction(
                        dustAvailabilityFractions,
                        substances,
                        availabilityRandomGenerator
                    )
                : null;

            // Create bioaccessibility models
            var bioaccessibilityModels = BioaccessibilityFractionModelBuilder
                .Create(substances, bioaccessibilityFractions);

            var result = new List<DustIndividualExposure>();
            foreach (var simulatedIndividual in simulatedIndividuals) {
                var age = simulatedIndividual.Age;
                var sex = simulatedIndividual.Gender;

                // Compute ingestion exposure
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
                if (routes.Contains(ExposureRoute.Oral)) {
                    var individualDustIngestion = calculateDustIngestion(
                        dustIngestions,
                        age,
                        sex,
                        dustIngestionUnit,
                        ingestionsRandomGenerator
                    );

                    var dustExposurePerSubstance = computeIngestionExposures(
                        substances,
                        individualDustIngestion,
                        concentrationModels,
                        bioaccessibilityModels,
                        bioaccessibilityRandomGenerator.Next(),
                        concentrationsRandomGenerator.Next()
                    );
                    exposuresPerPath[new(ExposureSource.Dust, ExposureRoute.Oral)] = dustExposurePerSubstance;
                }

                // Compute dermal exposure
                if (routes.Contains(ExposureRoute.Dermal)) {
                    var bodySurfaceArea = simulatedIndividual.BodySurfaceArea.Value;
                    var individualDustAdherenceAmount = calculateDustAdherenceAmount(
                        dustAdherenceAmounts,
                        age,
                        sex,
                        adherenceAmountsRandomGenerator
                    );
                    var individualDustBodyExposureFraction = calculateDustBodyExposureFraction(
                        dustBodyExposureFractions,
                        age,
                        sex,
                        exposureFractionsRandomGenerator
                    );
                    var dustExposurePerSubstance = computeDermalExposures(
                        substances,
                        timeDustExposure,
                        concentrationsRandomGenerator,
                        substanceDustAvailabilityFraction,
                        concentrationModels,
                        bodySurfaceArea,
                        individualDustAdherenceAmount,
                        individualDustBodyExposureFraction
                    );
                    exposuresPerPath[new(ExposureSource.Dust, ExposureRoute.Dermal)] = dustExposurePerSubstance;
                }

                var dustIndividualDayExposure = new DustIndividualExposure(simulatedIndividual, exposuresPerPath);
                result.Add(dustIndividualDayExposure);
            }
            return result;
        }

        private static List<IIntakePerCompound> computeDermalExposures(
            ICollection<Compound> substances,
            double timeDustExposure,
            IRandom dustConcentrationsRandomGenerator,
            Dictionary<Compound, double> substanceDustAvailabilityFraction,
            IDictionary<Compound, ConcentrationModel> concentrationModels,
            double bodySurfaceArea,
            double individualDustAdherenceAmount,
            double individualDustBodyExposureFraction
        ) {
            // TODO: create random generator per substance
            var dustExposurePerSubstance = new List<IIntakePerCompound>();
            foreach (var substance in substances) {

                if (concentrationModels.TryGetValue(substance, out var concentrationModel)) {
                    var individualDustConcentration = concentrationModel
                        .DrawFromDistribution(dustConcentrationsRandomGenerator, NonDetectsHandlingMethod.ReplaceByZero);
                    var exposure = new ExposurePerSubstance {
                        Compound = substance,
                        Amount = substanceDustAvailabilityFraction[substance]
                            * individualDustAdherenceAmount * timeDustExposure / 24D
                            * bodySurfaceArea * individualDustBodyExposureFraction
                            * individualDustConcentration
                    };
                    dustExposurePerSubstance.Add(exposure);
                }
            }
            return dustExposurePerSubstance;
        }

        private static List<IIntakePerCompound> computeIngestionExposures(
            ICollection<Compound> substances,
            double individualDustIngestion,
            IDictionary<Compound, ConcentrationModel> concentrationModels,
            IDictionary<Compound, IBioaccessibilityFractionModel> bioaccessibilityFractionModels,
            int bioaccessibilitySeed,
            int concentrationSeed
        ) {
            var result = new List<IIntakePerCompound>();
            foreach (var substance in substances) {
                var concentrationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(concentrationSeed, substance.GetHashCode()));
                var bioaccessibilityRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(bioaccessibilitySeed, substance.GetHashCode()));
                if (concentrationModels.TryGetValue(substance, out var concentrationModel)) {
                    var individualDustConcentration = concentrationModel
                        .DrawFromDistribution(concentrationRandomGenerator, NonDetectsHandlingMethod.ReplaceByZero);
                    var bioaccessibilityFraction = bioaccessibilityFractionModels
                        .TryGetValue(substance, out var bioaccessibityFractionModel) 
                            ? bioaccessibityFractionModel.Draw(bioaccessibilityRandomGenerator) 
                            : 1d;
                    var exposure = new ExposurePerSubstance {
                        Compound = substance,
                        Amount = individualDustIngestion * individualDustConcentration * bioaccessibilityFraction
                    };
                    result.Add(exposure);
                }
            }

            return result;
        }

        private static Dictionary<Compound, double> calculateSubstanceDustAvailabilityFraction(
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var result = new Dictionary<Compound, double>();
            foreach (var substance in substances) {
                var dustAvailabilityFraction = dustAvailabilityFractions
                    .Where(r => r.Substance == null || r.Substance == substance)
                    .SingleOrDefault();

                if (dustAvailabilityFraction != null) {
                    var distribution = DustAvailabilityFractionProbabilityDistributionFactory
                        .createProbabilityDistribution(dustAvailabilityFraction);

                    var substanceDustAvailabilityFraction =
                        dustAvailabilityFraction.DistributionType == DustAvailabilityFractionDistributionType.Constant
                            ? dustAvailabilityFraction.Value
                            : distribution.Draw(random);

                    result.Add(substance, substanceDustAvailabilityFraction);
                }
            }

            return result;
        }

        private static double calculateDustIngestion(
            ICollection<DustIngestion> dustIngestions,
            double? age,
            GenderType? sex,
            ExternalExposureUnit dustIngestionUnit,
            IRandom random
        ) {
            var dustIngestionAlignmentFactor = dustIngestionUnit
                .GetSubstanceAmountUnit()
                .GetMultiplicationFactor(SubstanceAmountUnit.Grams);

            var dustIngestion = dustIngestions
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Where(r => r.Sex == sex || r.Sex == GenderType.Undefined)
                .Last();

            var distribution = DustIngestionProbabilityDistributionFactory
                .createProbabilityDistribution(dustIngestion);

            var individualDustIngestion = dustIngestion.DistributionType == DustIngestionDistributionType.Constant
                ? dustIngestion.Value
                : distribution.Draw(random);

            return dustIngestionAlignmentFactor * individualDustIngestion;
        }

        private static double calculateDustAdherenceAmount(
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            double? age,
            GenderType? sex,
            IRandom random
        ) {
            var dustAdherenceAmount = dustAdherenceAmounts
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Where(r => r.Sex == sex || r.Sex == GenderType.Undefined)
                .Last();

            var distribution = DustAdherenceAmountProbabilityDistributionFactory
                .createProbabilityDistribution(dustAdherenceAmount);

            var individualDustAdherenceAmount = dustAdherenceAmount.DistributionType
                == DustAdherenceAmountDistributionType.Constant
                ? dustAdherenceAmount.Value : distribution.Draw(random);

            return individualDustAdherenceAmount;
        }

        private static double calculateDustBodyExposureFraction(
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            double? age,
            GenderType? sex,
            IRandom random
        ) {
            var dustBodyExposureFraction = dustBodyExposureFractions
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Where(r => r.Sex == sex || r.Sex == GenderType.Undefined)
                .Last();

            var distribution = DustBodyExposureFractionProbabilityDistributionFactory
                .createProbabilityDistribution(dustBodyExposureFraction);

            var individualDustBodyExposureFraction = dustBodyExposureFraction.DistributionType
                == DustBodyExposureFractionDistributionType.Constant
                ? dustBodyExposureFraction.Value : distribution.Draw(random);

            return individualDustBodyExposureFraction;
        }
    }
}
