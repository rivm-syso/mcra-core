using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.IndividualExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public class SoilExposureCalculator {

        public SoilExposureCalculator() { }

        /// <summary>
        /// Computes soil exposures for the provided collection of individuals.
        /// </summary>
        public static List<SoilIndividualExposure> ComputeSoilExposure(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            IDictionary<Compound, ConcentrationModel> concentrationModels,
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

            var ingestionsRandomGenerator = new McraRandomGenerator(soilExposureDeterminantsRandomGenerator.Next());
            var concentrationsRandomGenerator = new McraRandomGenerator(soilExposureDeterminantsRandomGenerator.Next());

            // Note: we use grams for expressing soil amounts;
            // - we align concentrations so that they are expressed per gram soil;
            // - we align soil ingestions so that these are expressed as gram soil per day;
            // - we align dermal contact so that it is expressed as gram soil per surface area per day.
            var concentrationAmountFactor = soilConcentrationUnit.GetSubstanceAmountUnit()
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var concentrationMassFactor = soilConcentrationUnit.GetConcentrationMassUnit()
                .GetMultiplicationFactor(ConcentrationMassUnit.Grams);
            var concentrationAlignmentFactor = concentrationAmountFactor * concentrationMassFactor;

            var result = new List<SoilIndividualExposure>();
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
                    ingestionsRandomGenerator
                );
                var soilExposurePerSubstance = computeInhalationExposures(
                    substances,
                    individualSoilIngestion,
                    concentrationModels,
                    concentrationsRandomGenerator
                );
                exposuresPerPath[new(ExposureSource.Soil, ExposureRoute.Oral)] = soilExposurePerSubstance;

                var soilIndividualExposure = new SoilIndividualExposure(individualDay.SimulatedIndividual, exposuresPerPath);
                result.Add(soilIndividualExposure);
            }
            return result;
        }

        private static List<IIntakePerCompound> computeInhalationExposures(
            ICollection<Compound> substances,
            double individualSoilIngestion,
            IDictionary<Compound, ConcentrationModel> concentrationModels,
            IRandom soilConcentrationsRandomGenerator
        ) {
            // TODO: create random generator per substance
            var soilExposurePerSubstance = new List<IIntakePerCompound>();
            foreach (var substance in substances) {
                if (concentrationModels.TryGetValue(substance, out var concentrationModel)) {
                    var individualSoilConcentration = concentrationModel
                        .DrawFromDistribution(soilConcentrationsRandomGenerator, NonDetectsHandlingMethod.ReplaceByZero);
                    var exposure = new ExposurePerSubstance {
                        Compound = substance,
                        Amount = individualSoilIngestion * individualSoilConcentration
                    };
                    soilExposurePerSubstance.Add(exposure);
                }
            }

            return soilExposurePerSubstance;
        }

        private static double calculateSoilIngestion(
            ICollection<SoilIngestion> ingestions,
            double? age,
            GenderType? sex,
            ExternalExposureUnit ingestionUnit,
            IRandom random
        ) {
            var alignmentFactor = ingestionUnit
                .GetSubstanceAmountUnit()
                .GetMultiplicationFactor(SubstanceAmountUnit.Grams);

            var ingestion = ingestions
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Where(r => r.Sex == sex || r.Sex == GenderType.Undefined)
                .Last();

            var distribution = SoilIngestionProbabilityDistributionFactory
                .createProbabilityDistribution(ingestion);

            var individualSoilIngestion = ingestion.DistributionType == SoilIngestionDistributionType.Constant
                ? ingestion.Value
                : distribution.Draw(random);

            return alignmentFactor * individualSoilIngestion;
        }
    }
}
