using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.SampleCompoundCollections.MissingValueImputation {
    public class MissingvalueImputationCalculator {

        private IConcentrationModelCalculationSettings _settings;

        public MissingvalueImputationCalculator(IConcentrationModelCalculationSettings settings
        ) {
            _settings = settings;
        }

        /// <summary>
        /// Replace missing values:
        /// 1) Do not use authorized use data: then set agricultural use to 100% for all food / compound combinations
        /// 2) Use agricultural use: agricultural use = 100% for allowed food / compound combinations
        /// </summary>
        /// <param name="sampleCompoundCollections"></param>
        /// <param name="compoundConcentrationModels"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        public void ImputeMissingValues(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationModel> compoundConcentrationModels,
            IDictionary<Compound, double> relativePotencyFactors,
            int seed,
            CompositeProgressState progressState = null
        ) {
            var generatedValuesPerFoodCompound = new ConcurrentDictionary<(Food, Compound), List<double>>();

            var cancelToken = progressState?.CancellationToken ?? new();
            // Draw random values for missing values
            Parallel.ForEach(
                sampleCompoundCollections,
                new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                sampleCompoundCollection => {
                    var random = new McraRandomGenerator(RandomUtils.CreateSeed(seed, sampleCompoundCollection.Food.Code));
                    var compounds = sampleCompoundCollection.SampleCompoundRecords
                        .SelectMany(sc => sc.SampleCompounds)
                        .GroupBy(c => c.Key)
                        .OrderBy(c => c.Key.Code, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    foreach (var compound in compounds) {
                        var mvCount = compound.Count(mv => mv.Value.IsMissingValue);
                        var drawValues = new List<double>(mvCount);
                        if (compoundConcentrationModels.TryGetValue((sampleCompoundCollection.Food, compound.Key), out var model)) {
                            for (int i = 0; i < mvCount; i++) {
                                drawValues.Add(model.DrawFromDistribution(random, _settings.NonDetectsHandlingMethod));
                            }
                            if (_settings.CorrelateImputedValueWithSamplePotency) {
                                drawValues = drawValues.OrderByDescending(r => r).ToList();
                            }
                        } else {
                            //Since mrl implementation always, a model is generated so only if no data are available AND no mrl, this point can be reached
                            drawValues = Enumerable.Repeat(0D, mvCount).ToList();
                        }
                        generatedValuesPerFoodCompound.TryAdd((sampleCompoundCollection.Food, compound.Key), drawValues);
                    }
                }
            );

            Parallel.ForEach(
                sampleCompoundCollections,
                new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                sampleCompoundCollection => {
                    var sampleCompoundRecords = sampleCompoundCollection.SampleCompoundRecords;
                    if (relativePotencyFactors != null && _settings.CorrelateImputedValueWithSamplePotency) {
                        // Re-order, then impute missing values
                        sampleCompoundRecords = sampleCompoundRecords.OrderByDescending(c => c.ImputedCumulativePotency(relativePotencyFactors)).ToList();
                    }
                    foreach (var sampleCompoundRecord in sampleCompoundRecords) {
                        var sampleCompounds = sampleCompoundRecord.SampleCompounds.Values;
                        foreach (var sampleCompound in sampleCompounds) {
                            if (sampleCompound.IsMissingValue) {
                                sampleCompound.Residue = generatedValuesPerFoodCompound[(sampleCompoundCollection.Food, sampleCompound.ActiveSubstance)].First();
                                generatedValuesPerFoodCompound[(sampleCompoundCollection.Food, sampleCompound.ActiveSubstance)].RemoveAt(0);
                                sampleCompound.ResType = ResType.VAL;
                            }
                        }
                    }
                }
            );
        }

        public void ReplaceImputeMissingValuesByZero(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new();
            Parallel.ForEach(
                sampleCompoundCollections,
                new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                sampleCompoundCollection => {
                    foreach (var sampleCompoundRecord in sampleCompoundCollection.SampleCompoundRecords) {
                        foreach (var sampleCompound in sampleCompoundRecord.SampleCompounds) {
                            if (sampleCompound.Value.IsMissingValue) {
                                sampleCompound.Value.Residue = 0D;
                                sampleCompound.Value.ResType = ResType.VAL;
                            }
                        }
                    }
                }
            );
        }
    }
}
