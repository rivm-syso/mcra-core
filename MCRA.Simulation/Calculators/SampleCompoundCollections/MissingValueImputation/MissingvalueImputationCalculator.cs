using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
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
            var generatedValuesPerFoodCompound = new ConcurrentDictionary<Food, Dictionary<Compound, Queue<double>>>();

            var cancelToken = progressState?.CancellationToken ?? new();
            var correlateWithSamplePotency = _settings.CorrelateImputedValueWithSamplePotency;

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

                    var queueDict = new Dictionary<Compound, Queue<double>>(compounds.Count);

                    foreach (var compound in compounds) {
                        var mvCount = compound.Count(mv => mv.Value.IsMissingValue);
                        var drawValues = new double[mvCount]; //initializes double array, all values are 0 by default
                        if (compoundConcentrationModels.TryGetValue((sampleCompoundCollection.Food, compound.Key), out var model)) {
                            for (int i = 0; i < mvCount; i++) {
                                drawValues[i] = model.DrawFromDistribution(random, _settings.NonDetectsHandlingMethod);
                            }
                        }
                        var drawQueue = correlateWithSamplePotency
                            ? new Queue<double>(drawValues.OrderDescending())
                            : new Queue<double>(drawValues);

                        queueDict.Add(compound.Key, drawQueue);
                    }
                    generatedValuesPerFoodCompound.TryAdd(sampleCompoundCollection.Food, queueDict);
                }
            );

            Parallel.ForEach(
                sampleCompoundCollections,
                new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                sampleCompoundCollection => {
                    var sampleCompoundRecords = sampleCompoundCollection.SampleCompoundRecords;
                    if (relativePotencyFactors != null && correlateWithSamplePotency) {
                        // Re-order, then impute missing values
                        sampleCompoundRecords = sampleCompoundRecords.OrderByDescending(c => c.ImputedCumulativePotency(relativePotencyFactors)).ToList();
                    }
                    var queueDict = generatedValuesPerFoodCompound[sampleCompoundCollection.Food];

                    foreach (var sampleCompoundRecord in sampleCompoundRecords) {
                        var sampleCompounds = sampleCompoundRecord.SampleCompounds.Values;
                        foreach (var sampleCompound in sampleCompounds) {
                            if (sampleCompound.IsMissingValue) {
                                var drawQueue = queueDict[sampleCompound.ActiveSubstance];
                                sampleCompound.Residue = drawQueue.Dequeue();
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
