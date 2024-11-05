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
            ICollection<IIndividualDay> individuals,
            ICollection<Compound> substances,
            ICollection<DustConcentrationDistribution> dustConcentrationDistributions,
            ICollection<DustIngestion> dustIngestions,
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            List<ExposureRoute> exposureRoutes,
            ConcentrationUnit dustConcentrationUnit,
            ExternalExposureUnit dustIngestionUnit,
            ExposureUnitTriple targetUnit,
            IRandom dustExposureDeterminantsRandomGenerator,
            double timeDustExposure
        ) {
            if (individuals == null) {
                return null;
            }

            // TODO: random per substance?
            var dustAvailabilityRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustIngestionsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustAdherenceAmountsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustExposureFractionsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());
            var dustConcentrationsRandomGenerator = new McraRandomGenerator(dustExposureDeterminantsRandomGenerator.Next());

            var substanceDustAvailabilityFraction = calculateSubstanceDustAvailabilityFraction(
                dustAvailabilityFractions,
                substances,
                dustAvailabilityRandomGenerator
            );

            var targetAmountUnit = targetUnit.SubstanceAmountUnit;

            var targetConcentrationMassUnit = ConcentrationMassUnit.Grams;
            var targetSubstanceAmountUnit = SubstanceAmountUnit.Grams;

            var dustConcentrationAmountFactor = dustConcentrationUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetAmountUnit);
            var dustConcentrationMassFactor = dustConcentrationUnit.GetConcentrationMassUnit().GetMultiplicationFactor(targetConcentrationMassUnit);
            var dustConcentrationFactor = dustConcentrationAmountFactor * dustConcentrationMassFactor;

            var adjustedDustConcentrationDistributions = dustConcentrationDistributions
                .Select(r => {
                    var conc = r.Concentration * dustConcentrationFactor;
                    return new {
                        r.Substance,
                        conc
                    };
                });

            var dustIngestionFactor = dustIngestionUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetSubstanceAmountUnit);

            // TODO: correction for time?
            var adjustedDustIngestions = dustIngestions
                .Select(r => {
                    var ingestion = r.Value * dustIngestionFactor;
                    var variability = r.CvVariability * dustIngestionFactor;
                    return new DustIngestion {
                        idSubgroup = r.idSubgroup,
                        AgeLower = r.AgeLower,
                        Sex = r.Sex,
                        DistributionType = r.DistributionType,
                        Value = ingestion,
                        CvVariability = variability,
                        ExposureUnit = r.ExposureUnit
                    };
                })
                .ToList();

            var needsAge = adjustedDustIngestions.All(r => r.AgeLower.HasValue) |
                dustAdherenceAmounts.All(r => r.AgeLower.HasValue) |
                dustBodyExposureFractions.All(r => r.AgeLower.HasValue);
            if (needsAge & individuals.Any(r => r.Individual.GetAge() == null)) {
                throw new Exception("Missing values for age in individuals.");
            }

            var needsSex = adjustedDustIngestions.All(r => r.Sex != GenderType.Undefined) |
                dustAdherenceAmounts.All(r => r.Sex != GenderType.Undefined) |
                dustBodyExposureFractions.All(r => r.Sex != GenderType.Undefined);

            if (needsSex & individuals.Any(r => r.Individual.GetGender() == GenderType.Undefined)) {
                throw new Exception("Missing values for gender in individuals.");
            }

            var needsBsa = exposureRoutes.Contains(ExposureRoute.Dermal);
            if (needsBsa & individuals.Any(r => r.Individual.GetBsa() == null)) {
                throw new Exception("Missing values for body surface area (BSA) in individuals.");
            }

            if (targetUnit.IsPerBodyWeight() & individuals.Any(r => double.IsNaN(r.Individual.BodyWeight))) {
                throw new Exception("Missing values for body weight in individuals.");
            }

            var result = new List<DustIndividualDayExposure>();
            foreach (var individual in individuals) {
                var age = individual.Individual.GetAge();
                var sex = individual.Individual.GetGender();
                var bodySurface = individual.Individual.GetBsa();
                var weight = individual.Individual.BodyWeight;

                var individualDustIngestion = calculateDustIngestion(adjustedDustIngestions, age, sex, dustIngestionsRandomGenerator);
                var individualDustAdherenceAmount = calculateDustAdherenceAmount(dustAdherenceAmounts, age, sex, dustAdherenceAmountsRandomGenerator);
                var individualDustBodyExposureFraction = calculateDustBodyExposureFraction(dustBodyExposureFractions, age, sex, dustExposureFractionsRandomGenerator);

                var exposuresPerRoute = new Dictionary<ExposureRoute, List<DustExposurePerSubstance>>();
                foreach (var exposureRoute in exposureRoutes) {
                    var dustExposurePerSubstance = new List<DustExposurePerSubstance>();
                    foreach (var substance in substances) {
                        var substanceDustConcentrationDistributions = adjustedDustConcentrationDistributions
                            .Where(r => r.Substance == substance)
                            .Select(r => r.conc);
                        var individualDustConcentration = substanceDustConcentrationDistributions
                            .DrawRandom(dustConcentrationsRandomGenerator);

                        var exposure = new DustExposurePerSubstance {
                            Compound = substance,
                            Amount = exposureRoute == ExposureRoute.Inhalation
                                ? computeInhalation(
                                    individualDustIngestion,
                                    individualDustConcentration,
                                    weight,
                                    targetUnit.IsPerBodyWeight()
                                ) : computeDermal(
                                      substanceDustAvailabilityFraction[substance],
                                      individualDustAdherenceAmount,
                                      timeDustExposure,
                                      bodySurface,
                                      individualDustBodyExposureFraction,
                                      individualDustConcentration,
                                      weight,
                                      targetUnit.IsPerBodyWeight()
                                )
                        };
                        dustExposurePerSubstance.Add(exposure);
                    }
                    exposuresPerRoute[exposureRoute] = dustExposurePerSubstance;
                }

                var dustIndividualDayExposure = new DustIndividualDayExposure() {
                    SimulatedIndividualId = individual.SimulatedIndividualId,
                    IndividualSamplingWeight = individual.IndividualSamplingWeight,
                    SimulatedIndividualDayId = individual.SimulatedIndividualDayId,
                    Individual = individual.Individual,
                    ExposurePerSubstanceRoute = exposuresPerRoute
                };
                result.Add(dustIndividualDayExposure);
            }
            return result;
        }

        private static Dictionary<Compound, double> calculateSubstanceDustAvailabilityFraction(
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<Compound> substances,
            McraRandomGenerator random
        ) {
            var result = new Dictionary<Compound, double>();
            foreach (var substance in substances) {
                var dustAvailabilityFraction = dustAvailabilityFractions
                    .Where(r => substance == r.Substance | r.Substance == null)
                    .SingleOrDefault();

                var distribution = DustAvailabilityFractionProbabilityDistributionFactory
                    .createProbabilityDistribution(dustAvailabilityFraction);

                var substanceDustAvailabilityFraction =
                    dustAvailabilityFraction.DistributionType.Equals(DustAvailabilityFractionDistributionType.Constant) ?
                    dustAvailabilityFraction.Value : distribution.Draw(random);

                result.Add(substance, substanceDustAvailabilityFraction);
            }

            return result;
        }

        private static double calculateDustIngestion(
            ICollection<DustIngestion> dustIngestions,
            double? age,
            GenderType? sex,
            McraRandomGenerator random
        ) {
            var dustIngestion = dustIngestions
                .Where(r => age >= r.AgeLower | r.AgeLower == null)
                .Where(r => r.Sex == sex | r.Sex == GenderType.Undefined)
                .Last();

            var distribution = DustIngestionProbabilityDistributionFactory
                .createProbabilityDistribution(dustIngestion);

            var individualDustIngestion =
                dustIngestion.DistributionType.Equals(DustIngestionDistributionType.Constant) ?
                dustIngestion.Value : distribution.Draw(random);

            return individualDustIngestion;
        }

        private static double calculateDustAdherenceAmount(
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            double? age,
            GenderType? sex,
            McraRandomGenerator random
        ) {
            var dustAdherenceAmount = dustAdherenceAmounts
                .Where(r => age >= r.AgeLower | r.AgeLower == null)
                .Where(r => r.Sex == sex | r.Sex == GenderType.Undefined)
                .Last();

            var distribution = DustAdherenceAmountProbabilityDistributionFactory
                .createProbabilityDistribution(dustAdherenceAmount);

            var individualDustAdherenceAmount = 
                dustAdherenceAmount.DistributionType.Equals(DustAdherenceAmountDistributionType.Constant) ?
                dustAdherenceAmount.Value : distribution.Draw(random);

            return individualDustAdherenceAmount;
        }

        private static double calculateDustBodyExposureFraction(
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            double? age,
            GenderType? sex,
            McraRandomGenerator random
        ) {
            var dustBodyExposureFraction = dustBodyExposureFractions
                .Where(r => age >= r.AgeLower | r.AgeLower == null)
                .Where(r => r.Sex == sex | r.Sex == GenderType.Undefined)
                .Last();

            var distribution = DustBodyExposureFractionProbabilityDistributionFactory
                .createProbabilityDistribution(dustBodyExposureFraction);

            var individualDustBodyExposureFraction =
                dustBodyExposureFraction.DistributionType.Equals(DustBodyExposureFractionDistributionType.Constant) ?
                dustBodyExposureFraction.Value : distribution.Draw(random);

            return individualDustBodyExposureFraction;
        }

        private static double computeInhalation(
            double dustIngestion,
            double substanceConcentration,
            double? bodyWeight,
            bool isPerBodyWeight
        ) {
            var result = dustIngestion * substanceConcentration;
            if (isPerBodyWeight) {
                result = result / (double)bodyWeight;
            }
            return result;
        }

        private static double computeDermal(
            double fractionSubstanceDustAvailable,
            double dustAdheringToSkin,
            double timeDustExposure,
            double? bodySurface,
            double fractionBodySurfaceExposed,
            double substanceConcentration,
            double? bodyWeight,
            bool isPerBodyWeight
        ) {
            var result = fractionSubstanceDustAvailable * dustAdheringToSkin *
                timeDustExposure / 24D * bodySurface * fractionBodySurfaceExposed * substanceConcentration;
            if (isPerBodyWeight) {
                result = result / (double)bodyWeight;
            }
            return (double)result;
        }
    }
}
