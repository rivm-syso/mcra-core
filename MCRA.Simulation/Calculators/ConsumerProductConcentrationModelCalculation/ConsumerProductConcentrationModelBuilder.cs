using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConsumerProductConcentrationModelCalculation {

    /// <summary>
    /// Builder class for consumer product concentration models.
    /// </summary>
    public class ConsumerProductConcentrationModelBuilder {

        /// <summary>
        /// Creates concentration models for consumer product concentrations.
        /// </summary>
        /// <param name="cpConcentrations"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="lorReplacementFactor"></param>
        /// <returns></returns>
        public IDictionary<(ConsumerProduct, Compound), ConcentrationModel> Create(
            ICollection<ConsumerProductConcentration> cpConcentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var concentrationModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel>();
            var groups = cpConcentrations
                .GroupBy(c => (c.Product, c.Substance))
                .ToList();

            foreach (var group in groups) {
                var concentrationModel = createConcentrationModel(
                        group,
                        nonDetectsHandlingMethod,
                        lorReplacementFactor
                    );
                concentrationModels[(group.Key.Product, group.Key.Substance)] = concentrationModel;
            }
            return concentrationModels;
        }

        /// <summary>
        /// Fit Empirical distribution
        /// </summary>
        /// <param name="concentrations"></param>
        /// <returns></returns>
        private ConcentrationModel createConcentrationModel(
            IEnumerable<ConsumerProductConcentration> concentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var positiveResidues = concentrations
                .Where(c => c.Concentration > 0)
                .Select(c => c.Concentration)
                .ToList();

            var censoredValues = concentrations
                .Where(c => c.Concentration == 0)
                .Select(c => new CensoredValue() {
                    LOD = double.NaN,
                    LOQ = double.NaN,
                    ResType = ResType.LOQ
                })
                .ToList();

            var substanceResidueCollection = new CompoundResidueCollection() {
                Positives = positiveResidues,
                CensoredValuesCollection = censoredValues
            };

            var concentrationModel = new CMEmpirical() {
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                Residues = substanceResidueCollection,
                FractionOfLor = lorReplacementFactor,
                CorrectedWeightedAgriculturalUseFraction = 1
            };
            concentrationModel.CalculateParameters();

            return concentrationModel;
        }

        /// <summary>
        /// Creates concentration models for consumer product concentrations.
        /// </summary>
        /// <param name="cpConcentrations"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="lorReplacementFactor"></param>
        /// <returns></returns>
        public IDictionary<(ConsumerProduct, Compound), ConcentrationModel> Create(
            ICollection<ConsumerProductConcentrationDistribution> concentrationDistributions,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var concentrationModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel>();

            foreach (var distribution in concentrationDistributions) {
                var concentrationModel = createConcentrationModel(
                    distribution,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor
                );
                concentrationModels[(distribution.Product, distribution.Substance)] = concentrationModel;
            }
            return concentrationModels;
        }

        /// <summary>
        /// Fit CensoredLognormal model, fallback (currently) is Empirical distribution
        /// </summary>
        /// <param name="distribution"></param>
        /// <returns></returns>
        private ConcentrationModel createConcentrationModel(
            ConsumerProductConcentrationDistribution distribution,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var substanceResidueCollection = new CompoundResidueCollection();

            var concentrationModel = new CMSummaryStatistics() {
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                //Residues = substanceResidueCollection,
                FractionOfLor = lorReplacementFactor,
                CorrectedWeightedAgriculturalUseFraction = distribution.OccurrencePercentage.HasValue
                    ? distribution.OccurrencePercentage.Value / 100
                    : 1D,
                ConcentrationDistribution = new ConcentrationDistribution() {
                    Mean = distribution.Mean,
                    CV = distribution.CvVariability,
                }
            };
            concentrationModel.CalculateParameters();
            return concentrationModel;
        }
    }
}
