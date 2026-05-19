using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConsumerProductConcentrationModelCalculation {

    /// <summary>
    /// Builder class for consumer product concentration models.
    /// </summary>
    public class ConsumerProductConcentrationModelBuilder {

        /// <summary>
        /// Creates concentration models for consumer product concentrations.
        /// </summary>
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
                .Where(c => c.Concentration < 0)
                .Select(c => new CensoredValue() {
                    LOD = double.NaN,
                    LOQ = Math.Abs(c.Concentration),
                    ResType = ResType.LOQ
                })
                .ToList();

            var zerosCount = concentrations
                .Count(c => c.Concentration == 0);

            var residueCollection = new ResidueCollection() {
                Positives = positiveResidues,
                CensoredValuesCollection = censoredValues,
                ZerosCount = zerosCount
            };

            var concentrationModel = new CMEmpirical() {
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                Residues = residueCollection,
                FractionOfLor = lorReplacementFactor,
                CorrectedOccurenceFraction = 1,
                FractionTrueZeros = (double)concentrations.Count() / zerosCount
            };
            concentrationModel.CalculateParameters();

            return concentrationModel;
        }

        /// <summary>
        /// Creates concentration models for consumer product concentrations.
        /// </summary>
        public IDictionary<(ConsumerProduct, Compound), ConcentrationModel> Create(
            ICollection<ConsumerProductConcentrationDistribution> concentrationDistributions,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnit targetConcentrationUnit
        ) {
            var concentrationModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel>();
            foreach (var distribution in concentrationDistributions) {
                var concentrationModel = createConcentrationModel(
                    distribution,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor,
                    targetConcentrationUnit
                );
                concentrationModels[(distribution.Product, distribution.Substance)] = concentrationModel;
            }
            return concentrationModels;
        }

        /// <summary>
        /// Create concentration model from input data on concentration distributions.
        /// </summary>
        private ConcentrationModel createConcentrationModel(
            ConsumerProductConcentrationDistribution distribution,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnit targetUnit
        ) {
            var alignmentFactor = distribution.Unit
                        .GetConcentrationAlignmentFactor(targetUnit, distribution.Substance.MolecularMass);
            var concentrationDistribution = new ConcentrationDistribution() {
                Mean = distribution.Mean * alignmentFactor,
                CV = distribution.CvVariability,
            };
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                ConsumerProductConcentrationDistributionType.Constant => new CMConstant(),
                ConsumerProductConcentrationDistributionType.LogNormal => new CMSummaryStatistics(),
                ConsumerProductConcentrationDistributionType.Uniform => new CMSummaryStatistics(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for consumer product distributions."),
            };

            var occurrenceFraction = distribution.OccurrencePercentage.HasValue
                ? distribution.OccurrencePercentage.Value / 100
                : 1D;
            concentrationModel.Compound = distribution.Substance;
            concentrationModel.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            concentrationModel.DesiredModelType = concentrationModel.ModelType;
            concentrationModel.OccurenceFraction = occurrenceFraction;
            concentrationModel.CorrectedOccurenceFraction = occurrenceFraction;
            concentrationModel.ConcentrationDistribution = new ConcentrationDistribution() {
                Mean = distribution.Mean,
                CV = distribution.CvVariability,
            };
            concentrationModel.Unit = new StandardConcentrationUnitSpec(targetUnit);

            concentrationModel.CalculateParameters();
            return concentrationModel;
        }
    }
}
