using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ParameterDistributionModels;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustExposureCalculator {

        public DustExposureCalculator() { }

        /// <summary>
        /// Returns all dust exposures.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="dustConcentrationDistributions"></param>
        /// <param name="dustIngestions"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        public static List<IndividualDustExposureRecord> ComputeDustExposure(
            ICollection<Individual> individuals,
            ICollection<DustConcentrationDistribution> dustConcentrationDistributions,
            ICollection<DustIngestion> dustIngestions,
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            List<ExposureRoute> exposureRoutes,
            DustExposuresModuleConfig dustExposuresModuleConfig,
            Compound substance
        ) {
            if (individuals == null) {
                return null;
            }

            // TODO: random generator or seed should be passed as an argument
            var seed = 37;
            var random = new McraRandomGenerator(seed);

            // TODO: exposure unit should be passed as an argument
            var targetUnit = ExposureUnitTriple.CreateDietaryExposureUnit(
                ConsumptionUnit.g,
                dustConcentrationDistributions.First().ConcentrationUnit,
                BodyWeightUnit.kg,
                false
            );

            var substanceDustAvailabilityFraction = calculateSubstanceDustAvailabilityFraction(
                dustAvailabilityFractions,
                substance,
                random
            );

            var substanceDustConcentrationDistributions = dustConcentrationDistributions
                .Where(r => r.Substance == substance)
                .Select(r => r.Concentration);

            var timeDustExposure = dustExposuresModuleConfig.DustTimeExposed;

            var result = new List<IndividualDustExposureRecord>();
            foreach (var individual in individuals) {
                
                var age = individual.GetAge();
                var sex = individual.GetGender();
                var bodyWeight = individual.BodyWeight;

                var individualDustConcentration = substanceDustConcentrationDistributions.DrawRandom();

                var individualDustIngestion = calculateDustIngestion(dustIngestions, age, sex, random);
                var individualDustAdherenceAmount = calculateDustAdherenceAmount(dustAdherenceAmounts, age, sex, random);
                var individualDustBodyExposureFraction = calculateDustBodyExposureFraction(dustBodyExposureFractions, age, sex, random);

                // TODO: implement GetBSA (extension) method in individual containing this logic
                var bodySurface = Convert.ToDouble(individual.IndividualPropertyValues.Where(c => c.IndividualProperty.Name == "BSA").First().Value);

                foreach (var exposureRoute in exposureRoutes) {
                    var item = new IndividualDustExposureRecord() {
                        IdIndividual = individual.Id.ToString(),
                        Individual = individual,
                        Substance = substance,
                        ExposureRoute = exposureRoute,
                        Exposure = exposureRoute == ExposureRoute.Inhalation
                            ? computeInhalation(
                                bodyWeight,
                                individualDustIngestion,
                                individualDustConcentration
                            ) : computeDermal(
                                  bodyWeight,
                                  substanceDustAvailabilityFraction,
                                  individualDustAdherenceAmount,
                                  timeDustExposure,
                                  bodySurface,
                                  individualDustBodyExposureFraction,
                                  individualDustConcentration
                            ),
                        ExposureUnit = targetUnit
                    };
                    result.Add(item);
                }                
            }
            return result;
        }

        private static double calculateSubstanceDustAvailabilityFraction(
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            Compound substance,
            McraRandomGenerator random
        ) {
            var dustAvailabilityFraction = dustAvailabilityFractions
                .Where(r => substance == r.Substance | r.Substance == null)
                .SingleOrDefault();

            var substanceDustAvailabilityFraction = double.NaN;
            if (dustAvailabilityFraction.DistributionType != ProbabilityDistribution.Unspecified) {

                // TODO: reconsider use of the probability distribution factory and models from kinetic
                // conversion; these should be more generic when we reuse these in other calculators.
                // Also reconsider the GetValueOrDefault; if it is null, then null should be passed. It should
                // be up to the distribution to handle null values or throw proper exceptions.
                var model = ProbabilityDistributionFactory
                    .createProbabilityDistributionModel(dustAvailabilityFraction.DistributionType);
                model.Initialize(
                    dustAvailabilityFraction.Value,
                    dustAvailabilityFraction.CvVariability.GetValueOrDefault()
                );
                substanceDustAvailabilityFraction = model.Sample(random);
            } else {
                throw new NotImplementedException();
            }

            return substanceDustAvailabilityFraction;
        }

        private static double calculateDustIngestion(
            ICollection<DustIngestion> dustIngestions,
            double? age,
            GenderType sex,
            McraRandomGenerator random
        ) {
            var dustIngestion = dustIngestions
                .Where(r => age >= r.AgeLower | r.AgeLower == null)
                .Where(r => r.Sex == sex | r.Sex == GenderType.Undefined)
                .Last();

            var individualDustIngestion = double.NaN;
            if (dustIngestion.DistributionType != ProbabilityDistribution.Unspecified) {

                // TODO: reconsider use of the probability distribution factory and models from kinetic
                // conversion; these should be more generic when we reuse these in other calculators.
                // Also reconsider the GetValueOrDefault; if it is null, then null should be passed. It should
                // be up to the distribution to handle null values or throw proper exceptions.
                var model = ProbabilityDistributionFactory
                    .createProbabilityDistributionModel(dustIngestion.DistributionType);
                model.Initialize(
                    dustIngestion.Value,
                    dustIngestion.CvVariability.GetValueOrDefault()
                );
                individualDustIngestion = model.Sample(random);
            } else {
                throw new NotImplementedException();
            }

            return individualDustIngestion;
        }

        private static double calculateDustAdherenceAmount(
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            double? age,
            GenderType sex,
            McraRandomGenerator random
        ) {
            var dustAdherenceAmount = dustAdherenceAmounts
                .Where(r => age >= r.AgeLower | r.AgeLower == null)
                .Where(r => r.Sex == sex | r.Sex == GenderType.Undefined)
                .Last();

            var individualDustAdherenceAmount = double.NaN;
            if (dustAdherenceAmount.DistributionType != ProbabilityDistribution.Unspecified) {

                // TODO: reconsider use of the probability distribution factory and models from kinetic
                // conversion; these should be more generic when we reuse these in other calculators.
                // Also reconsider the GetValueOrDefault; if it is null, then null should be passed. It should
                // be up to the distribution to handle null values or throw proper exceptions.
                var model = ProbabilityDistributionFactory
                    .createProbabilityDistributionModel(dustAdherenceAmount.DistributionType);
                model.Initialize(
                    dustAdherenceAmount.Value,
                    dustAdherenceAmount.CvVariability.GetValueOrDefault()
                );
                individualDustAdherenceAmount = model.Sample(random);
            } else {
                throw new NotImplementedException();
            }

            return individualDustAdherenceAmount;
        }

        private static double calculateDustBodyExposureFraction(
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            double? age,
            GenderType sex,
            McraRandomGenerator random
        ) {
            var dustBodyExposureFraction = dustBodyExposureFractions
                .Where(r => age >= r.AgeLower | r.AgeLower == null)
                .Where(r => r.Sex == sex | r.Sex == GenderType.Undefined)
                .Last();

            var individualDustBodyExposureFraction = double.NaN;
            if (dustBodyExposureFraction.DistributionType != ProbabilityDistribution.Unspecified) {

                // TODO: reconsider use of the probability distribution factory and models from kinetic
                // conversion; these should be more generic when we reuse these in other calculators.
                // Also reconsider the GetValueOrDefault; if it is null, then null should be passed. It should
                // be up to the distribution to handle null values or throw proper exceptions.
                var model = ProbabilityDistributionFactory
                    .createProbabilityDistributionModel(dustBodyExposureFraction.DistributionType);
                model.Initialize(
                    dustBodyExposureFraction.Value,
                    dustBodyExposureFraction.CvVariability.GetValueOrDefault()
                );
                individualDustBodyExposureFraction = model.Sample(random);
            } else {
                throw new NotImplementedException();
            }

            return individualDustBodyExposureFraction;
        }

        private static double computeInhalation(
            double bodyWeight,
            double dustExposureDeterminant,
            double substanceConcentration
        ) {
            var result = 1D / bodyWeight * dustExposureDeterminant * substanceConcentration;
            return result;
        }

        private static double computeDermal(
            double bodyWeight,
            double fractionSubstanceDustAvailable,
            double dustAdheringToSkin,
            double timeDustExposure,
            double bodySurface,
            double fractionBodySurfaceExposed,
            double substanceConcentration
        ) {
            var result = 1D / bodyWeight * fractionSubstanceDustAvailable * dustAdheringToSkin *
                timeDustExposure / 24D * bodySurface * fractionBodySurfaceExposed * substanceConcentration;
            return result;
        }
    }
}
