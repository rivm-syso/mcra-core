using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.SampleCompoundCollections.NonDetectsImputation {
    public class CensoredValuesImputationCalculator {

        private readonly IConcentrationModelCalculationSettings _settings;

        public CensoredValuesImputationCalculator(IConcentrationModelCalculationSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Replaces the non-detects in the sample compound collections using the substance concentration models.
        /// </summary>
        /// <param name="sampleCompoundCollections"></param>
        /// <param name="compoundConcentrationModels"></param>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        public void ReplaceCensoredValues(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationModel> compoundConcentrationModels,
            int seed,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            Parallel.ForEach(
                sampleCompoundCollections,
                new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                sampleCompoundCollection => {
                    var food = sampleCompoundCollection.Food;
                    var random = new McraRandomGenerator(RandomUtils.CreateSeed(seed, food.Code));
                    foreach (var sampleCompoundRecord in sampleCompoundCollection.SampleCompoundRecords) {
                        var sampleCompounds = sampleCompoundRecord.SampleCompounds.Values
                            .OrderBy(c => c.ActiveSubstance.Code, StringComparer.OrdinalIgnoreCase);
                        foreach (var sampleCompound in sampleCompounds) {
                            var compound = sampleCompound.ActiveSubstance;
                            var lor = 0D;
                            if (sampleCompound.IsCensoredValue) {
                                if (compoundConcentrationModels.TryGetValue((food, compound), out var model)) {
                                    var fraction = model.FractionCensored / model.Residues.FractionCensoredValues;

                                    // Note: we only want to know whether we want to draw a censored value,
                                    // the LOR that we want to have is determined by the analytical method
                                    // So: we cannot rely on the DrawAccordingToNonDetectsHandlingMethod
                                    // because this may not return the LOR specific for this analytical method
                                    var drawCensoredValue = model.Residues.FractionCensoredValues > 0
                                        && _settings.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero
                                        && random.NextDouble() < fraction;

                                    if (_settings.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                                        lor = drawCensoredValue ? sampleCompound.Lor * model.FractionOfLor : 0;
                                    } else if (_settings.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                                        if (sampleCompound.IsNonDetect) {
                                            lor = drawCensoredValue ? sampleCompound.Lod * model.FractionOfLor : 0;
                                        } else {
                                            lor = drawCensoredValue ? sampleCompound.Lod + model.FractionOfLor * (sampleCompound.Loq - sampleCompound.Lod) : 0;
                                        }
                                    } else if (_settings.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem) {
                                        if (sampleCompound.IsNonDetect) {
                                            lor = 0;
                                        } else {
                                            lor = drawCensoredValue ? model.FractionOfLor * sampleCompound.Loq : 0;
                                        }
                                    }

                                }
                                sampleCompound.Residue = lor;
                                sampleCompound.ResType = ResType.VAL;
                            }
                        }
                    }
                }
            );
        }
    }
}
