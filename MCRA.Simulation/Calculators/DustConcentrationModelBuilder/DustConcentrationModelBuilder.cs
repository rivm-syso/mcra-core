using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.DustConcentrationModelBuilder {

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

            var residueCollection = new ResidueCollection() {
                Positives = positiveResidues,
                CensoredValuesCollection = censoredValues
            };

            var concentrationModel = new CMEmpirical() {
                Compound = substance,
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                Residues = residueCollection,
                FractionOfLor = lorReplacementFactor,
                CorrectedOccurenceFraction = 1,
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
            double lorReplacementFactor,
            ConcentrationUnit targetConcentrationUnit
        ) {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            foreach (var distribution in concentrationDistributions) {
                var concentrationModel = createConcentrationModel(
                    distribution,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor,
                    targetConcentrationUnit
                );
                concentrationModels[distribution.Substance] = concentrationModel;
            }
            return concentrationModels;
        }

        private ConcentrationModel createConcentrationModel(
            DustConcentrationDistribution distribution,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnit targetConcentrationUnit
        ) {
            var alignmentFactor = distribution.Unit
                        .GetConcentrationAlignmentFactor(targetConcentrationUnit, distribution.Substance.MolecularMass);
            var concentrationDistribution = new ConcentrationDistribution() {
                Mean = distribution.Mean * alignmentFactor,
                CV = distribution.CvVariability,
            };
            ConcentrationModel concentrationModel = distribution.DistributionType switch {
                DustConcentrationDistributionType.Constant => new CMConstant(),
                DustConcentrationDistributionType.LogNormal => new CMSummaryStatistics(),
                _ => throw new NotImplementedException($"Unsupported concentration model type {distribution.DistributionType} for dust distributions."),
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
            concentrationModel.ConcentrationUnit = targetConcentrationUnit;

            concentrationModel.CalculateParameters();
            return concentrationModel;
        }
    }
}
