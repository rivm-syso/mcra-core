using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation {
    public class FoodSubstanceResidueCollectionsBuilder {

        /// <summary>
        /// Creates substance residue collections for the provided substances and
        /// sample substance collections.
        /// </summary>
        /// <param name="compounds"></param>
        /// <param name="sampleCompoundCollection"></param>
        /// <param name="occurrencePatternsByFoodSubstance"></param>
        /// <param name="substanceAuthorisations"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), FoodSubstanceResidueCollection> Create(
            ICollection<Compound> compounds,
            ICollection<SampleCompoundCollection> sampleCompoundCollection,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrencePatternsByFoodSubstance,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations
        ) {
            if (sampleCompoundCollection == null) {
                return null;
            }
            var newCompoundResidueCollections = sampleCompoundCollection
                .SelectMany(r => compounds
                    .Select(compound => {
                        var food = r.Food;
                        return createFromSampleCompoundRecords(food, compound, r.SampleCompoundRecords);
                    })
                )
                .ToList();
            var result = new Dictionary<(Food, Compound), FoodSubstanceResidueCollection>();
            foreach (var record in newCompoundResidueCollections) {
                result.Add((record.Food, record.Compound), record);
            }
            return result;
        }

        private static FoodSubstanceResidueCollection createFromSampleCompoundRecords(
            Food food,
            Compound compound,
            ICollection<SampleCompoundRecord> sampleCompoundRecords
        ) {
            var positives = new List<double>();
            var censoredValuesCollection = new List<CensoredValue>();
            var zeroesCount = 0;

            foreach (var rec in sampleCompoundRecords) {
                if (rec.SampleCompounds.TryGetValue(compound, out var sc) && !sc.IsMissingValue) {
                    if (sc.IsPositiveResidue) {
                        positives.Add(sc.Residue);
                    } else if (sc.IsCensoredValue) {
                        censoredValuesCollection.Add(new CensoredValue() {
                            LOD = sc.Lod,
                            LOQ = sc.Loq,
                            ResType = sc.IsNonDetect ? ResType.LOD : ResType.LOQ
                        });
                    } else if (sc.IsZeroConcentration) {
                        zeroesCount++;
                    }
                }
            }

            positives.Sort();

            return new FoodSubstanceResidueCollection() {
                Compound = compound,
                Food = food,
                Positives = positives,
                ZerosCount = zeroesCount,
                CensoredValuesCollection = censoredValuesCollection,
            };
        }

        /// <summary>
        /// Resamples the set of substance residue collections.
        /// </summary>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IDictionary<(Food, Compound), FoodSubstanceResidueCollection> Resample(
            IDictionary<(Food, Compound), FoodSubstanceResidueCollection> compoundResidueCollections,
            IRandom random,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new();
            var seed = random.Next();
            var newRecords = compoundResidueCollections.Values
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(compoundResidueCollection => {
                    var randomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, compoundResidueCollection.GetHashCode()));
                    return Resample(compoundResidueCollection, randomGenerator);
                });

            var result = new Dictionary<(Food, Compound), FoodSubstanceResidueCollection>();
            foreach (var record in newRecords) {
                result.Add((record.Food, record.Compound), record);
            }
            return result;
        }

        /// <summary>
        /// Generates a bootstrap instance of the compound residue collection. I.e., resamples
        /// the positives and the censored values in order to get a bootstrap sample of substance
        /// residue concentrations.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static FoodSubstanceResidueCollection Resample(FoodSubstanceResidueCollection compoundResidueCollection, IRandom random) {
            var allSamplesTmp = new List<(double Concentration, ResType ResType)>();
            allSamplesTmp.AddRange(compoundResidueCollection.Positives.Select(c => (Concentration: c, ResType: ResType.VAL)).ToList());

            allSamplesTmp.AddRange(compoundResidueCollection.CensoredValuesCollection
                .Select(c => (
                    Concentration: !double.IsNaN(c.LOQ) ? c.LOQ : c.LOD,
                    ResType: !double.IsNaN(c.LOQ) ? ResType.LOQ : ResType.LOD
                ))
                .OrderBy(r => r.Concentration)
                .ToList());

            allSamplesTmp.AddRange(Enumerable.Repeat(0d, compoundResidueCollection.ZerosCount)
                .Select(c => (
                    Concentration: c,
                    ResType: ResType.VAL
                ))
                .OrderBy(r => r.Concentration)
                .ToList());

            allSamplesTmp = allSamplesTmp.Resample(random).ToList();

            var censoredValuesCollection = allSamplesTmp.Where(s => s.ResType != ResType.VAL)
                .Select(l => new CensoredValue() {
                    LOD = l.Concentration,
                    LOQ = l.Concentration,
                    ResType = l.ResType,
                }).ToList();

            var newCollection = new FoodSubstanceResidueCollection() {
                Compound = compoundResidueCollection.Compound,
                Food = compoundResidueCollection.Food,
                Positives = allSamplesTmp.Where(s => s.Concentration > 0 && s.ResType == ResType.VAL).Select(c => c.Concentration).ToList(),
                CensoredValuesCollection = censoredValuesCollection,
                ZerosCount = allSamplesTmp.Count(s => s.Concentration == 0 && s.ResType == ResType.VAL)
            };
            return newCollection;
        }
    }
}
