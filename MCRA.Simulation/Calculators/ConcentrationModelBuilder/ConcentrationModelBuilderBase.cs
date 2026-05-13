using MCRA.Data.Compiled.Interfaces;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelBuilder {
    /// <summary>
    /// Builder class for common behavior of concentration models.
    /// </summary>
    public abstract class ConcentrationModelBuilderBase {

        // For non-dietary sources air, dust, soil, etc., we assume that the occurence frequency is 100%, i.e., all
        // products contain the substance
        private static readonly double _occurenceFrequency = 1D;

        public IDictionary<Compound, ConcentrationModel> CreateByConcentrations<T>(
            ICollection<T> concentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnitSpec unit
        ) where T : ISubstanceConcentration {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            var groups = concentrations?
                .GroupBy(c => c.Substance)
                .ToList() ?? [];
            foreach (var group in groups) {
                var concentrationModel = createConcentrationModel(
                    group.Key,
                    group,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor
                );
                concentrationModel.Unit = unit;
                concentrationModels[group.Key] = concentrationModel;
            }
            return concentrationModels;
        }

        public IDictionary<Compound, ConcentrationModel> CreateByDistributions<T>(
            ICollection<T> concentrationDistributions,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnitSpec unit
        ) where T : IConcentrationDistribution {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            foreach (var distribution in concentrationDistributions) {
                var concentrationModel = createConcentrationModel(
                    distribution,
                    nonDetectsHandlingMethod,
                    lorReplacementFactor,
                    unit
                );
                concentrationModels[distribution.Substance] = concentrationModel;
            }
            return concentrationModels;
        }

        public static (double occurenceFraction, double correctedOccurenceFraction) CalculateOccurrenceFractions(
           double occurrenceFrequency,
           ResidueCollection residueCollection) {
            double occurenceFraction;
            double correctedOccurenceFraction;
            if (residueCollection.NumberOfResidues > 0) {
                occurenceFraction = !(double.IsNaN(occurrenceFrequency)) ? occurrenceFrequency : 1D - residueCollection.FractionZeros;
                correctedOccurenceFraction = occurenceFraction;
                correctedOccurenceFraction = Math.Max(correctedOccurenceFraction, residueCollection.FractionPositives);
                correctedOccurenceFraction = Math.Min(correctedOccurenceFraction, 1 - residueCollection.FractionZeros);
            } else {
                occurenceFraction = !double.IsNaN(occurrenceFrequency) ? occurrenceFrequency : 1D;
                correctedOccurenceFraction = occurenceFraction;
            }

            return (occurenceFraction, correctedOccurenceFraction);
        }

        protected abstract ConcentrationModel getModel<T>(T distribution);

        protected abstract double getAlignmentFactor<T>(
            ConcentrationUnitSpec targetUnit,
            T concentrationDistribution
        ) where T : IConcentrationDistribution;

        private static ConcentrationModel createConcentrationModel<T>(
            Compound substance,
            IEnumerable<T> concentrations,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) where T : ISubstanceConcentration {
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

            var (occurenceFraction, correctedOccurenceFraction) =
                CalculateOccurrenceFractions(_occurenceFrequency, residueCollection);
            var concentrationModel = new CMEmpirical() {
                Compound = substance,
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                Residues = residueCollection,
                FractionOfLor = lorReplacementFactor,
                OccurenceFraction = occurenceFraction,
                CorrectedOccurenceFraction = correctedOccurenceFraction
            };
            concentrationModel.CalculateParameters();

            return concentrationModel;
        }

        private ConcentrationModel createConcentrationModel<T>(
            T distribution,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor,
            ConcentrationUnitSpec unit
        ) where T : IConcentrationDistribution {
            var alignmentFactor = getAlignmentFactor(unit, distribution);
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
            concentrationModel.Unit = unit;

            concentrationModel.CalculateParameters();
            return concentrationModel;
        }
    }
}
