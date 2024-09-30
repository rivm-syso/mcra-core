using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
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
        public static List<DustIndividualDayExposure> ComputeDustExposure(
            ICollection<IIndividualDay> individuals,
            ICollection<Compound> substances,
            ICollection<DustConcentrationDistribution> dustConcentrationDistributions,
            ICollection<DustIngestion> dustIngestions,
            ICollection<DustAdherenceAmount> dustAdherenceAmounts,
            ICollection<DustAvailabilityFraction> dustAvailabilityFractions,
            ICollection<DustBodyExposureFraction> dustBodyExposureFractions,
            List<ExposureRoute> exposureRoutes,
            ExposureUnitTriple targetUnit,
            double timeDustExposure
        ) {
            if (individuals == null) {
                return null;
            }

            // TODO: random generator or seed should be passed as an argument
            var seed = 37;
            var random = new McraRandomGenerator(seed);
            var ingestionsRandomGenerator = new McraRandomGenerator(seed);            

            var substanceDustAvailabilityFraction = calculateSubstanceDustAvailabilityFraction(
                dustAvailabilityFractions,
                substances,
                random
            );          

            var result = new List<DustIndividualDayExposure>();
            foreach (var individual in individuals) {

                // TODO: dietary individuals might not have age/gender/BW/BSA                
                var age = individual.Individual.GetAge();
                var sex = individual.Individual.GetGender();
                // TODO: implement GetBSA (extension) method in individual containing this logic
                var bodySurface = Convert.ToDouble(individual.Individual.IndividualPropertyValues.Where(c => c.IndividualProperty.Name == "BSA").First().Value);

                var individualDustIngestion = calculateDustIngestion(dustIngestions, age, sex, ingestionsRandomGenerator);
                var individualDustAdherenceAmount = calculateDustAdherenceAmount(dustAdherenceAmounts, age, sex, random);
                var individualDustBodyExposureFraction = calculateDustBodyExposureFraction(dustBodyExposureFractions, age, sex, random);
                                
                var exposuresPerRoute = new Dictionary<ExposureRoute, List<DustExposurePerSubstance>>();
                foreach (var exposureRoute in exposureRoutes) {
                    var dustExposurePerSubstance = new List<DustExposurePerSubstance>();
                    foreach (var substance in substances) {
                        var substanceDustConcentrationDistributions = dustConcentrationDistributions
                            .Where(r => r.Substance == substance)
                            .Select(r => r.Concentration);
                        // TODO: implement and use new random generator
                        var individualDustConcentration = substanceDustConcentrationDistributions
                            .DrawRandom();

                        var exposure = new DustExposurePerSubstance {
                            Compound = substance,
                            Amount = exposureRoute == ExposureRoute.Inhalation
                                ? computeInhalation(                                    
                                    individualDustIngestion,
                                    individualDustConcentration
                                ) : computeDermal(                                      
                                      substanceDustAvailabilityFraction[substance],
                                      individualDustAdherenceAmount,
                                      timeDustExposure,
                                      bodySurface,
                                      individualDustBodyExposureFraction,
                                      individualDustConcentration
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
                    ExposureUnit = targetUnit,
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
                result.Add(substance, substanceDustAvailabilityFraction);
            }

            return result;
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
            double dustIngestion,
            double substanceConcentration
        ) {
            var result = dustIngestion * substanceConcentration;
            return result;
        }

        private static double computeDermal(
            double fractionSubstanceDustAvailable,
            double dustAdheringToSkin,
            double timeDustExposure,
            double bodySurface,
            double fractionBodySurfaceExposed,
            double substanceConcentration
        ) {
            var result = fractionSubstanceDustAvailable * dustAdheringToSkin *
                timeDustExposure / 24D * bodySurface * fractionBodySurfaceExposed * substanceConcentration;
            return result;
        }
    }
}
