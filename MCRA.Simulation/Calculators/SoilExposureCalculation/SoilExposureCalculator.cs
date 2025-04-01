using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public class SoilExposureCalculator {

        public SoilExposureCalculator() { }

        /// <summary>
        /// Computes soil exposures for the provided collection of individuals.
        /// </summary>
        public static List<SoilIndividualDayExposure> ComputeSoilExposure(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<SoilConcentrationDistribution> soilConcentrationDistributions,
            ICollection<SoilIngestion> soilIngestions,
            ConcentrationUnit soilConcentrationUnit,
            ExternalExposureUnit soilIngestionUnit,
            ExposureUnitTriple targetUnit,
            IRandom soilExposureDeterminantsRandomGenerator
        ) {
            var needsAge = soilIngestions.All(r => r.AgeLower.HasValue);
            if (needsAge && individualDays.Any(r => !r.SimulatedIndividual.Age.HasValue)) {
                throw new Exception("Missing values for age in individuals.");
            }

            var needsSex = soilIngestions.All(r => r.Sex != GenderType.Undefined);
            if (needsSex && individualDays.Any(r => r.SimulatedIndividual.Gender == GenderType.Undefined)) {
                throw new Exception("Missing values for gender in individuals.");
            }

            var soilIngestionsRandomGenerator = new McraRandomGenerator(soilExposureDeterminantsRandomGenerator.Next());
            var soilConcentrationsRandomGenerator = new McraRandomGenerator(soilExposureDeterminantsRandomGenerator.Next());

            // Note: we use grams for expressing soil amounts;
            // - we align concentrations so that they are expressed per gram soil;
            // - we align soil ingestions so that these are expressed as gram soil per day;
            // - we align dermal contact so that it is expressed as gram soil per surface area per day.
            var soilConcentrationAmountFactor = soilConcentrationUnit.GetSubstanceAmountUnit()
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var soilConcentrationMassFactor = soilConcentrationUnit.GetConcentrationMassUnit()
                .GetMultiplicationFactor(ConcentrationMassUnit.Grams);
            var concentrationAlignmentFactor = soilConcentrationAmountFactor * soilConcentrationMassFactor;
            var alignedSoilConcentrationDistributions = soilConcentrationDistributions
                .GroupBy(r => r.Substance)
                .ToDictionary(r => r.Key, r => r.Select(c => c.Concentration * concentrationAlignmentFactor));

            var result = new List<SoilIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var age = individualDay.SimulatedIndividual.Age;
                var sex = individualDay.SimulatedIndividual.Gender;

                // Compute inhalation exposure
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
                var individualSoilIngestion = calculateSoilIngestion(
                    soilIngestions,
                    age,
                    sex,
                    soilIngestionUnit,
                    soilIngestionsRandomGenerator
                );
                var soilExposurePerSubstance = computeInhalationExposures(
                    substances,
                    individualSoilIngestion,
                    alignedSoilConcentrationDistributions,
                    soilConcentrationsRandomGenerator
                );
                exposuresPerPath[new(ExposureSource.Soil, ExposureRoute.Oral)] = soilExposurePerSubstance;

                var soilIndividualDayExposure = new SoilIndividualDayExposure(exposuresPerPath) {
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDay.SimulatedIndividual
                };
                result.Add(soilIndividualDayExposure);
            }
            return result;
        }

        private static List<IIntakePerCompound> computeInhalationExposures(
            ICollection<Compound> substances,
            double individualSoilIngestion,
            Dictionary<Compound, IEnumerable<double>> adjustedSoilConcentrationDistributions,
            McraRandomGenerator soilConcentrationsRandomGenerator
        ) {
            // TODO: create random generator per substance
            var soilExposurePerSubstance = new List<IIntakePerCompound>();
            foreach (var substance in substances) {
                if (adjustedSoilConcentrationDistributions.TryGetValue(substance, out var soilConcentrations)) {
                    var individualSoilConcentration = soilConcentrations
                        .DrawRandom(soilConcentrationsRandomGenerator);
                    var exposure = new SoilExposurePerSubstance {
                        Compound = substance,
                        Amount = individualSoilIngestion * individualSoilConcentration
                    };
                    soilExposurePerSubstance.Add(exposure);
                }
            }

            return soilExposurePerSubstance;
        }

        private static double calculateSoilIngestion(
            ICollection<SoilIngestion> soilIngestions,
            double? age,
            GenderType? sex,
            ExternalExposureUnit soilIngestionUnit,
            McraRandomGenerator random
        ) {
            var soilIngestionAlignmentFactor = soilIngestionUnit
                .GetSubstanceAmountUnit()
                .GetMultiplicationFactor(SubstanceAmountUnit.Grams);

            var soilIngestion = soilIngestions
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Where(r => r.Sex == sex || r.Sex == GenderType.Undefined)
                .Last();

            var distribution = SoilIngestionProbabilityDistributionFactory
                .createProbabilityDistribution(soilIngestion);

            var individualSoilIngestion = soilIngestion.DistributionType == SoilIngestionDistributionType.Constant
                ? soilIngestion.Value
                : distribution.Draw(random);

            return soilIngestionAlignmentFactor * individualSoilIngestion;
        }
    }
}
