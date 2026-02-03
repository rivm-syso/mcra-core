using MCRA.Data.Compiled.Objects;
using MCRA.General;
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
            ICollection<AirConcentration> indoorAirConcentrations,
            ICollection<AirConcentration> outdoorAirConcentrations,
            ICollection<AirIndoorFraction> airIndoorFractions,
            ICollection<AirVentilatoryFlowRate> airVentilatoryFlowRates,
            AirConcentrationUnit airConcentrationUnit,
            ExposureUnitTriple targetUnit,
            IRandom airExposureDeterminantsRandomGenerator
        ) {
            airVentilatoryFlowRates = [.. airVentilatoryFlowRates.OrderBy(x => x.AgeLower)];
            airIndoorFractions = [.. airIndoorFractions.OrderBy(x => x.AgeLower)];
            var needsAge = airVentilatoryFlowRates.All(r => r.AgeLower.HasValue)
                || airIndoorFractions.All(r => r.AgeLower.HasValue);
            if (needsAge && individualDays.Any(r => r.SimulatedIndividual.Age == null)) {
                throw new Exception("Missing values for age in individuals.");
            }

            var needsSex = airVentilatoryFlowRates.All(r => r.Sex != GenderType.Undefined);
            if (needsSex && individualDays.Any(r => r.SimulatedIndividual.Gender == GenderType.Undefined)) {
                throw new Exception("Missing values for sex in individuals.");
            }

            var flowRateRandomGenerator = new McraRandomGenerator(airExposureDeterminantsRandomGenerator.Next());
            var indoorConcentrationRandomGenerator = new McraRandomGenerator(airExposureDeterminantsRandomGenerator.Next());

            var concentrationAmountAlignmentFactor = airConcentrationUnit.GetSubstanceAmountUnit()
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var concentrationVolumeAlignmentFactor = 1 / airConcentrationUnit.GetConcentrationVolumeUnit()
                .GetMultiplicationFactor(VolumeUnit.Cubicmeter);
            var concentrationAlignmentFactor = concentrationAmountAlignmentFactor * concentrationVolumeAlignmentFactor;
            var alignedIndoorAirConcentrationDistributions = indoorAirConcentrations
                .GroupBy(r => r.Substance)
                .ToDictionary(r => r.Key, r => r.Select(c => c.Concentration * concentrationAlignmentFactor));
            var alignedOutdoorAirConcentrationDistributions = outdoorAirConcentrations
                .GroupBy(r => r.Substance)
                .ToDictionary(r => r.Key, r => r.Select(c => c.Concentration * concentrationAlignmentFactor));

            var result = new List<AirIndividualExposure>();
            foreach (var individualDay in individualDays) {
                var age = individualDay.SimulatedIndividual.Age;
                var sex = individualDay.SimulatedIndividual.Gender;

                // Compute indoor exposure
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
                if (routes.Contains(ExposureRoute.Inhalation)) {
                    var individualFlowRate = calculateFlowRate(
                        airVentilatoryFlowRates,
                        age,
                        sex,
                        flowRateRandomGenerator
                    );
                    var indoorFraction = calculateIndoorFraction(airIndoorFractions, age);
                    var airExposurePerSubstance = computeExposures(
                        substances,
                        individualFlowRate,
                        indoorFraction,
                        alignedIndoorAirConcentrationDistributions,
                        alignedOutdoorAirConcentrationDistributions,
                        indoorConcentrationRandomGenerator
                    );
                    exposuresPerPath[new(ExposureSource.Air, ExposureRoute.Inhalation)] = airExposurePerSubstance;
                }

                var airIndividualExposure = new AirIndividualExposure(individualDay.SimulatedIndividual, exposuresPerPath);
                result.Add(airIndividualExposure);
            }
            return result;
        }

        private static List<IIntakePerCompound> computeExposures(
            ICollection<Compound> substances,
            double individualFlowRate,
            double indoorFraction,
            Dictionary<Compound, IEnumerable<double>> adjustedIndoorAirConcentrations,
            Dictionary<Compound, IEnumerable<double>> adjustedOutdoorAirConcentrations,
            IRandom airConcentrationsRandomGenerator
        ) {
            // TODO: create random generator per substance
            // Note: it is assumed that indoor and outdoor concentrations are independent.
            var airExposurePerSubstance = new List<IIntakePerCompound>();
            foreach (var substance in substances) {
                var amount = 0d;
                if (adjustedIndoorAirConcentrations.TryGetValue(substance, out var indoorConcentrations)) {
                    var individualIndoorConcentration = indoorConcentrations
                        .DrawRandom(airConcentrationsRandomGenerator);
                    amount += individualFlowRate * individualIndoorConcentration * indoorFraction;
                }
                if (adjustedOutdoorAirConcentrations.TryGetValue(substance, out var outdoorConcentrations)) {
                    var individualOutdoorConcentration = outdoorConcentrations
                        .DrawRandom(airConcentrationsRandomGenerator);
                    amount += individualFlowRate * individualOutdoorConcentration * (1 - indoorFraction);
                }
                var exposure = new ExposurePerSubstance {
                    Compound = substance,
                    Amount = amount
                };
                airExposurePerSubstance.Add(exposure);
            }
            return airExposurePerSubstance;
        }

        private static double calculateFlowRate(
            ICollection<AirVentilatoryFlowRate> airVentilatoryFlowRates,
            double? age,
            GenderType? sex,
            IRandom random
        ) {
            var flowRate = airVentilatoryFlowRates
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
