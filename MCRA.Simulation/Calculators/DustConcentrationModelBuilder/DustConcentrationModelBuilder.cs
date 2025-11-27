using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.DustConcentrationModelCalculation {

    /// <summary>
    /// Builder class for dust concentration models.
    /// </summary>
    public class DustConcentrationModelBuilder {

        /// <summary>
        /// Creates concentration models for dust concentrations.
        /// </summary>
        public IDictionary<Compound, ConcentrationModel> Create(
            ICollection<DustConcentration> dustConcentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            var groups = dustConcentrations
                .GroupBy(c => c.Substance)
                .ToList();
            foreach (var group in groups) {
                var concentrationModel = createConcentrationModel(
                    group.Key,
                    group,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor
                );
                concentrationModels[group.Key] = concentrationModel;
            }
            return concentrationModels;
        }

        /// <summary>
        /// Fit Empirical distribution
        /// </summary>
        private ConcentrationModel createConcentrationModel(
            Compound substance,
            IEnumerable<DustConcentration> concentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var positiveResidues = concentrations
                .Where(c => c.Concentration > 0)
                .Select(c => c.Concentration)
                .ToList();

            var censoredValues = concentrations
                .Where(c => c.Concentration < 0)
                .Select(c => new CensoredValue() {
                    LOD = double.NaN,
                    LOQ = Math.Abs(c.Concentration),
                    ResType = ResType.LOQ
                })
                .ToList();

            var zerosCount = concentrations
                .Where(c => c.Concentration == 0)
                .Count();

            var substanceResidueCollection = new CompoundResidueCollection() {
                Positives = positiveResidues,
                CensoredValuesCollection = censoredValues
            };

            var concentrationModel = new CMEmpirical() {
                Compound = substance,
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                Residues = substanceResidueCollection,
                FractionOfLor = lorReplacementFactor,
                CorrectedWeightedAgriculturalUseFraction = 1,
                FractionTrueZeros = (double)concentrations.Count() / zerosCount,
            };
            concentrationModel.CalculateParameters();

            return concentrationModel;
        }

        /// <summary>
        /// Creates concentration models for dust concentrations.
        /// </summary>
        public IDictionary<Compound, ConcentrationModel> Create(
            ICollection<DustConcentrationDistribution> concentrationDistributions,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            foreach (var distribution in concentrationDistributions) {
                var concentrationModel = createConcentrationModel(
                    distribution,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor
                );
                concentrationModels[distribution.Substance] = concentrationModel;
            }
            return concentrationModels;
        }

        /// <summary>
        /// Fit CensoredLognormal model, fallback (currently) is Empirical distribution
        /// </summary>
        private ConcentrationModel createConcentrationModel(
            DustConcentrationDistribution distribution,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var occurrenceFraction = distribution.OccurrencePercentage.HasValue
                ? distribution.OccurrencePercentage.Value / 100
                : 1D;
            var concentrationModel = new CMSummaryStatistics() {
                Compound = distribution.Substance,
                FractionNonDetects = 0,
                FractionCensored = 0,
                FractionNonQuantifications = 0,
                FractionPositives = occurrenceFraction,
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                FractionOfLor = lorReplacementFactor,
                DesiredModelType = ConcentrationModelType.SummaryStatistics,
                WeightedAgriculturalUseFraction = occurrenceFraction,
                CorrectedWeightedAgriculturalUseFraction = occurrenceFraction,
                ConcentrationDistribution = new ConcentrationDistribution() {
                    Mean = distribution.Mean,
                    CV = distribution.CvVariability,
                },
                ConcentrationUnit = distribution.Unit,
                Residues = null,
                FractionTrueZeros = 1 - occurrenceFraction
            };
            concentrationModel.CalculateParameters();
            return concentrationModel;
        }
    }
}
