using MCRA.Data.Compiled.Interfaces;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelBuilder {

    /// <summary>
    /// Builder class for dust concentration models.
    /// </summary>
    public abstract class ConcentrationModelBuilderBase {
        /// <summary> 
        /// Creates concentration models for concentrations.
        /// </summary>
        public IDictionary<Compound, ConcentrationModel> Create<T>(
            ICollection<T> concentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) where T : ISubstanceConcentration {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            var groups = concentrations
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
        private static ConcentrationModel createConcentrationModel<T> (
            Compound substance,
            IEnumerable<T> concentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) where T: ISubstanceConcentration{
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
        /// Creates concentration models for concentrations.
        /// </summary>
        public IDictionary<Compound, ConcentrationModel> Create<T>(
            ICollection<T> concentrationDistributions,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnit targetConcentrationUnit
        ) where T : IConcentrationDistribution {
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

        private ConcentrationModel createConcentrationModel<T>(
            T distribution,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnit targetConcentrationUnit
        ) where T : IConcentrationDistribution {
            var alignmentFactor = getAlignmentFactor(targetConcentrationUnit, distribution);
            var concentrationDistribution = new ConcentrationDistribution() {
                Mean = distribution.Mean * alignmentFactor,
                CV = distribution.CvVariability,
            };
            var concentrationModel = getModel(distribution);

            var occurrenceFraction = distribution.OccurrencePercentage.HasValue
                ? distribution.OccurrencePercentage.Value / 100
                : 1D;
            concentrationModel.Compound = distribution.Substance;
            concentrationModel.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            concentrationModel.DesiredModelType = concentrationModel.ModelType;
            concentrationModel.OccurenceFraction = occurrenceFraction;
            concentrationModel.CorrectedOccurenceFraction = occurrenceFraction;
            concentrationModel.ConcentrationDistribution = concentrationDistribution;
            concentrationModel.ConcentrationUnit = targetConcentrationUnit;

            concentrationModel.CalculateParameters();
            return concentrationModel;
        }

        protected abstract ConcentrationModel getModel<T>(T distribution);

        protected virtual double getAlignmentFactor<T>(ConcentrationUnit targetConcentrationUnit, T distribution) {
            return 1;
        }
    }
}
