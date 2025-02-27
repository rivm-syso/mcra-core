using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustExposureCalculator {

        public DustExposureCalculator() { }

        /// <summary>
        /// Computes dust exposures for the provided collection of individuals.
        /// </summary>
        public static List<DustIndividualDayExposure> ComputeDustExposure(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            List<ExposureRoute> routes,
            ICollection<DustConcentrationDistribution> dustConcentrationDistributions,
            ICollection<DustIngestion> dustIngestions,
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            ConcentrationUnit dustConcentrationUnit,
            ExternalExposureUnit dustIngestionUnit,
            double timeDustExposure,
            ExposureUnitTriple targetUnit,
            IRandom dustExposureDeterminantsRandomGenerator
        ) {
            var needsAge = dustIngestions.All(r => r.AgeLower.HasValue)
                || dustAdherenceAmounts.All(r => r.AgeLower.HasValue)
                || dustBodyExposureFractions.All(r => r.AgeLower.HasValue);
            if (needsAge && individualDays.Any(r => r.Individual.GetAge() == null)) {
                throw new Exception("Missing values for age in individuals.");
            }

            var needsSex = dustIngestions.All(r => r.Sex != GenderType.Undefined)
                || dustAdherenceAmounts.All(r => r.Sex != GenderType.Undefined)
                || dustBodyExposureFractions.All(r => r.Sex != GenderType.Undefined);
            if (needsSex && individualDays.Any(r => r.Individual.GetGender() == GenderType.Undefined)) {
                throw new Exception("Missing values for gender in individuals.");
            }

            var needsBsa = routes.Contains(ExposureRoute.Dermal);
            if (needsBsa && individualDays.Any(r => r.Individual.GetBsa() == null)) {
                throw new Exception("Missing values for body surface area (BSA) in individuals.");
            }

            var dustAvailabilityRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustIngestionsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustAdherenceAmountsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustExposureFractionsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustConcentrationsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());

            // Note: we use grams for expressing dust amounts;
            // - we align concentrations so that they are expressed per gram dust;
            // - we align dust ingestions so that these are expressed as gram dust per day;
            // - we align dermal contact so that it is expressed as gram dust per surface area per day.
            var dustConcentrationAmountFactor = dustConcentrationUnit.GetSubstanceAmountUnit()
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var dustConcentrationMassFactor = dustConcentrationUnit.GetConcentrationMassUnit()
                .GetMultiplicationFactor(ConcentrationMassUnit.Grams);
            var concentrationAlignmentFactor = dustConcentrationAmountFactor * dustConcentrationMassFactor;
            var alignedDustConcentrationDistributions = dustConcentrationDistributions
                .GroupBy(r => r.Substance)
                .ToDictionary(r => r.Key, r => r.Select(c => c.Concentration * concentrationAlignmentFactor));

            // Compute availability fractions for ingestion exposure
            var substanceDustAvailabilityFraction = routes.Contains(ExposureRoute.Oral)
                ? calculateSubstanceDustAvailabilityFraction(
                        dustAvailabilityFractions,
                        substances,
                        dustAvailabilityRandomGenerator
                    )
                : null;

            var result = new List<DustIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var age = individualDay.Individual.GetAge();
                var sex = individualDay.Individual.GetGender();

                // Compute ingestion exposure
                var exposuresPerRoute = new Dictionary<ExposureRoute, List<DustExposurePerSubstance>>();
                if (routes.Contains(ExposureRoute.Oral)) {
                    var individualDustIngestion = calculateDustIngestion(
                        dustIngestions,
                        age,
                        sex,
                        dustIngestionUnit,
                        dustIngestionsRandomGenerator
                    );
                    var dustExposurePerSubstance = computeIngestionExposures(
                        substances,
                        individualDustIngestion,
                        alignedDustConcentrationDistributions,
                        dustConcentrationsRandomGenerator
                    );
                    exposuresPerRoute[ExposureRoute.Oral] = dustExposurePerSubstance;
                }

                // Compute dermal exposure
                if (routes.Contains(ExposureRoute.Dermal)) {
                    var bodySurfaceArea = individualDay.Individual.GetBsa();
                    var individualDustAdherenceAmount = calculateDustAdherenceAmount(
                        dustAdherenceAmounts,
                        age,
                        sex,
                        dustAdherenceAmountsRandomGenerator
                    );
                    var individualDustBodyExposureFraction = calculateDustBodyExposureFraction(
                        dustBodyExposureFractions,
                        age,
                        sex,
                        dustExposureFractionsRandomGenerator
                    );
                    var dustExposurePerSubstance = computeDermalExposures(
                        substances,
                        timeDustExposure,
                        dustConcentrationsRandomGenerator,
                        substanceDustAvailabilityFraction,
                        alignedDustConcentrationDistributions,
                        bodySurfaceArea.Value,
                        individualDustAdherenceAmount,
                        individualDustBodyExposureFraction
                    );
                    exposuresPerRoute[ExposureRoute.Dermal] = dustExposurePerSubstance;
                }

                var dustIndividualDayExposure = new DustIndividualDayExposure() {
                    SimulatedIndividualId = individualDay.SimulatedIndividualId,
                    IndividualSamplingWeight = individualDay.IndividualSamplingWeight,
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    Individual = individualDay.Individual,
                    ExposurePerSubstanceRoute = exposuresPerRoute
                };
                result.Add(dustIndividualDayExposure);
            }
            return result;
        }

        private static List<DustExposurePerSubstance> computeDermalExposures(
            ICollection<Compound> substances,
            double timeDustExposure,
            McraRandomGenerator dustConcentrationsRandomGenerator,
            Dictionary<Compound, double> substanceDustAvailabilityFraction,
            Dictionary<Compound, IEnumerable<double>> adjustedDustConcentrationDistributions,
            double bodySurfaceArea,
            double individualDustAdherenceAmount,
            double individualDustBodyExposureFraction
        ) {
            // TODO: create random generator per substance
            var dustExposurePerSubstance = new List<DustExposurePerSubstance>();
            foreach (var substance in substances) {
                if (adjustedDustConcentrationDistributions.TryGetValue(substance, out var dustConcentrations)) {
                    var individualDustConcentration = dustConcentrations
                        .DrawRandom(dustConcentrationsRandomGenerator);
                    var exposure = new DustExposurePerSubstance {
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

        private static List<DustExposurePerSubstance> computeIngestionExposures(
            ICollection<Compound> substances,
            double individualDustIngestion,
            Dictionary<Compound, IEnumerable<double>> adjustedDustConcentrationDistributions,
            McraRandomGenerator dustConcentrationsRandomGenerator
        ) {
            // TODO: create random generator per substance
            var dustExposurePerSubstance = new List<DustExposurePerSubstance>();
            foreach (var substance in substances) {
                if (adjustedDustConcentrationDistributions.TryGetValue(substance, out var dustConcentrations)) {
                    var individualDustConcentration = dustConcentrations
                        .DrawRandom(dustConcentrationsRandomGenerator);
                    var exposure = new DustExposurePerSubstance {
                        Compound = substance,
                        Amount = individualDustIngestion * individualDustConcentration
                    };
                    dustExposurePerSubstance.Add(exposure);
                }
            }

            return dustExposurePerSubstance;
        }

        private static Dictionary<Compound, double> calculateSubstanceDustAvailabilityFraction(
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<Compound> substances,
            McraRandomGenerator random
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
            McraRandomGenerator random
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
            McraRandomGenerator random
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
            McraRandomGenerator random
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
