using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.IndividualExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {
    public class AirExposureCalculator {

        /// <summary>
        /// Computes air exposures for the provided collection of individuals.
        /// </summary>
        public static List<AirIndividualExposure> ComputeAirExposure(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            List<ExposureRoute> routes,
            IDictionary<Compound, ConcentrationModel> indoorConcentrationModels,
            IDictionary<Compound, ConcentrationModel> outdoorConcentrationModels,
            ICollection<AirIndoorFraction> airIndoorFractions,
            ICollection<AirVentilatoryFlowRate> ventilatoryFlowRates,
            AirConcentrationUnit airConcentrationUnit,
            ExposureUnitTriple targetUnit,
            IRandom exposureDeterminantsRandomGenerator
        ) {
            ventilatoryFlowRates = [.. ventilatoryFlowRates.OrderBy(x => x.AgeLower)];
            airIndoorFractions = [.. airIndoorFractions.OrderBy(x => x.AgeLower)];
            var needsAge = ventilatoryFlowRates.All(r => r.AgeLower.HasValue)
                || airIndoorFractions.All(r => r.AgeLower.HasValue);
            if (needsAge && individualDays.Any(r => r.SimulatedIndividual.Age == null)) {
                throw new Exception("Missing values for age in individuals.");
            }

            var needsSex = ventilatoryFlowRates.All(r => r.Sex != GenderType.Undefined);
            if (needsSex && individualDays.Any(r => r.SimulatedIndividual.Gender == GenderType.Undefined)) {
                throw new Exception("Missing values for sex in individuals.");
            }

            var flowRateRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());
            var indoorConcentrationRandomGenerator = new McraRandomGenerator(exposureDeterminantsRandomGenerator.Next());

            var concentrationAmountAlignmentFactor = airConcentrationUnit.GetSubstanceAmountUnit()
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var concentrationVolumeAlignmentFactor = 1 / airConcentrationUnit.GetConcentrationVolumeUnit()
                .GetMultiplicationFactor(VolumeUnit.Cubicmeter);
            var concentrationAlignmentFactor = concentrationAmountAlignmentFactor * concentrationVolumeAlignmentFactor;

            var result = new List<AirIndividualExposure>();
            foreach (var individualDay in individualDays) {
                var age = individualDay.SimulatedIndividual.Age;
                var sex = individualDay.SimulatedIndividual.Gender;

                // Compute indoor exposure
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
                if (routes.Contains(ExposureRoute.Inhalation)) {
                    var individualFlowRate = calculateFlowRate(
                        ventilatoryFlowRates,
                        age,
                        sex,
                        flowRateRandomGenerator
                    );
                    var indoorFraction = calculateIndoorFraction(airIndoorFractions, age);
                    var airExposurePerSubstance = computeExposures(
                        substances,
                        individualFlowRate,
                        indoorFraction,
                        indoorConcentrationModels,
                        outdoorConcentrationModels,
                        indoorConcentrationRandomGenerator
                    );
                    exposuresPerPath[new(ExposureSource.Air, ExposureRoute.Inhalation)] = airExposurePerSubstance;
                }

                var individualExposure = new AirIndividualExposure(individualDay.SimulatedIndividual, exposuresPerPath);
                result.Add(individualExposure);
            }
            return result;
        }

        private static List<IIntakePerCompound> computeExposures(
            ICollection<Compound> substances,
            double individualFlowRate,
            double indoorFraction,
            IDictionary<Compound, ConcentrationModel> indoorConcentrationModels,
            IDictionary<Compound, ConcentrationModel> outdoorConcentrationModels,
            IRandom concentrationsRandomGenerator
        ) {
            // TODO: create random generator per substance
            // Note: it is assumed that indoor and outdoor concentrations are independent.
            var exposurePerSubstance = new List<IIntakePerCompound>();
            foreach (var substance in substances) {
                var amount = 0d;
                if (indoorConcentrationModels?.TryGetValue(substance, out var concentrationModel) ?? false) {
                    var individualIndoorConcentration = concentrationModel
                        .DrawFromDistribution(concentrationsRandomGenerator, NonDetectsHandlingMethod.ReplaceByZero);
                    amount += individualFlowRate * individualIndoorConcentration * indoorFraction;
                }
                if (outdoorConcentrationModels?.TryGetValue(substance, out concentrationModel) ?? false) {
                    var individualOutdoorConcentration = concentrationModel
                        .DrawFromDistribution(concentrationsRandomGenerator, NonDetectsHandlingMethod.ReplaceByZero);
                    amount += individualFlowRate * individualOutdoorConcentration * (1 - indoorFraction);
                }
                var exposure = new ExposurePerSubstance {
                    Compound = substance,
                    Amount = amount
                };
                exposurePerSubstance.Add(exposure);
            }
            return exposurePerSubstance;
        }

        private static double calculateFlowRate(
            ICollection<AirVentilatoryFlowRate> ventilatoryFlowRates,
            double? age,
            GenderType? sex,
            IRandom random
        ) {
            var flowRate = ventilatoryFlowRates
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Where(r => r.Sex == sex || r.Sex == GenderType.Undefined)
                .Last();

            var distribution = AirVentilatoryFlowRateProbabilityDistributionFactory
                .createProbabilityDistribution(flowRate);
            var individualFlowRate = flowRate.DistributionType == VentilatoryFlowRateDistributionType.Constant
                ? flowRate.Value
                : distribution.Draw(random);
            return individualFlowRate;
        }

        private static double calculateIndoorFraction(
            ICollection<AirIndoorFraction> indoorFractions,
            double? age
        ) {
            var indoorFraction = indoorFractions
                .Where(r => age >= r.AgeLower || r.AgeLower == null)
                .Last();
            return indoorFraction.Fraction;
        }
    }
}
