using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.SampleCompoundCollections {

    /// <summary>
    /// Builder class for sample substance collections.
    /// </summary>
    public class SampleCompoundCollectionsBuilder {

        /// <summary>
        /// Get all sample compound records for the current project
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Food, SampleCompoundCollection> Create(
            ICollection<Food> foods,
            ICollection<Compound> compounds,
            ICollection<FoodSample> foodSamples,
            ConcentrationUnit concentrationUnit,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new();
            var samplesPerFoodAsMeasured = foods
               .GroupJoin(foodSamples,
                   f => f,
                   s => s.Food,
                   (f, s) => (
                       Food: f,
                       FoodSamples: s
                   ))
                .ToList();
            var result = samplesPerFoodAsMeasured
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(r => create(
                    r.Food,
                    r.FoodSamples.OrderBy(s => s.Food.Code, StringComparer.OrdinalIgnoreCase).ToList(),
                    compounds,
                    concentrationUnit)
                )
                .OrderBy(f => f.Food.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(r => r.Food);

            if (substanceAuthorisations != null) {
                foreach (var scc in result.Values) {
                    foreach (var scr in scc.SampleCompoundRecords) {
                        var positives = scr.SampleCompounds.Values
                            .Where(r => r.IsPositiveResidue);
                        scr.AuthorisedUse = positives
                            .All(r => substanceAuthorisations.ContainsKey((scc.Food, r.ActiveSubstance))
                                || (scc.Food.BaseFood != null && substanceAuthorisations.ContainsKey((scc.Food.BaseFood, r.ActiveSubstance)))
                        );
                    }
                }
            } else {
                foreach (var scc in result.Values) {
                    foreach (var scr in scc.SampleCompoundRecords) {
                        scr.AuthorisedUse = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Resamples the sample compound collection for a bootstrap run.
        /// </summary>
        /// <param name="sampleCompoundCollections"></param>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public static List<SampleCompoundCollection> ResampleSampleCompoundCollections(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            int seed,
            CompositeProgressState progressState = null
        ) {
            var newSampleCompoundCollections = new ConcurrentBag<SampleCompoundCollection>();
            var cancelToken = progressState?.CancellationToken ?? new();
            return sampleCompoundCollections
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(sampleCompoundCollection => {
                    var randomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, sampleCompoundCollection.GetHashCode()));
                    var newSampleCompoundRecords = sampleCompoundCollection.SampleCompoundRecords
                        .Resample(randomGenerator)
                        .Select(scr => new SampleCompoundRecord() {
                            FoodSample = scr.FoodSample,
                            AuthorisedUse = scr.AuthorisedUse,
                            SampleCompounds = scr.SampleCompounds.Values
                                .ToDictionary(sc => sc.ActiveSubstance, sc => sc.Clone())
                        })
                        .ToList();
                    return new SampleCompoundCollection(sampleCompoundCollection.Food, newSampleCompoundRecords);
                })
                .OrderBy(c => c.Food.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static SampleCompoundCollection create(
            Food food,
            ICollection<FoodSample> foodSamples,
            ICollection<Compound> compounds,
            ConcentrationUnit referenceUnit
        ) {
            var sampleCompoundRecords = new List<SampleCompoundRecord>();
            foreach (var sample in foodSamples) {
                // Normal construction of sample compound records
                sampleCompoundRecords.Add(createFromSamples(sample, compounds, referenceUnit));
            }
            return new SampleCompoundCollection(food, sampleCompoundRecords);
        }

        private static SampleCompoundRecord createFromSamples(
            FoodSample foodSample,
            ICollection<Compound> compounds,
            ConcentrationUnit referenceUnit
        ) {
            var sampleCompoundRecord = new SampleCompoundRecord() {
                FoodSample = foodSample,
                SampleCompounds = compounds.Select(compound => {
                    var substanceAnalyses = foodSample.SampleAnalyses
                        .Where(c => c.AnalyticalMethod.AnalyticalMethodCompounds.ContainsKey(compound))
                        .ToList();
                    var sampleCompounds = substanceAnalyses
                        .Select(c => {
                            var analyticalMethodCompound = c.AnalyticalMethod.AnalyticalMethodCompounds[compound];
                            c.Concentrations.TryGetValue(compound, out var sampleCompoundConcentration);
                            var concentrationCorrection = analyticalMethodCompound != null ? analyticalMethodCompound.ConcentrationUnit.GetConcentrationUnitMultiplier(referenceUnit) : 1D;
                            var lod = concentrationCorrection * analyticalMethodCompound?.LOD ?? double.NaN;
                            var loq = concentrationCorrection * analyticalMethodCompound?.LOQ ?? double.NaN;

                            var residue = (sampleCompoundConcentration != null && sampleCompoundConcentration.ResType == ResType.VAL)
                                ? concentrationCorrection * sampleCompoundConcentration.Concentration.Value : double.NaN;
                            var resType = ResType.VAL;
                            if (sampleCompoundConcentration == null) {
                                resType = !double.IsNaN(loq) ? ResType.LOQ : ResType.LOD;
                            } else {
                                resType = sampleCompoundConcentration.ResType;
                            }

                            return new SampleCompound() {
                                MeasuredSubstance = compound,
                                ActiveSubstance = compound,
                                Residue = residue,
                                ResType = resType,
                                Lod = lod,
                                Loq = loq
                            };
                        })
                        .ToList();

                    var result = sampleCompounds.Any()
                        ? aggregateMeasurements(sampleCompounds)
                        : missingMeasurements(compound);
                    return result;
                }).ToDictionary(sc => sc.ActiveSubstance)
            };
            return sampleCompoundRecord;
        }

        private static SampleCompound aggregateMeasurements(List<SampleCompound> sampleCompounds) {
            var residue = double.NaN;
            if (sampleCompounds.Any(r => !double.IsNaN(r.Residue))) {
                residue = sampleCompounds.Where(r => !double.IsNaN(r.Residue)).Average(r => r.Residue);
            }

            var resType = ResType.VAL;
            if (sampleCompounds.Any(r => r.IsPositiveResidue)) {
                resType = ResType.VAL;
            } else if (sampleCompounds.Any(r => r.ResType == ResType.LOQ)) {
                resType = ResType.LOQ;
            } else if (sampleCompounds.Any(r => r.ResType == ResType.LOD)) {
                resType = ResType.LOD;
            } else {
                resType = ResType.MV;
            };

            var result = new SampleCompound() {
                MeasuredSubstance = sampleCompounds.First().MeasuredSubstance,
                ActiveSubstance = sampleCompounds.First().ActiveSubstance,
                Residue = residue,
                ResType = resType,
                Loq = sampleCompounds.Max(r => r.Loq),
                Lod = sampleCompounds.Max(r => r.Lod),
            };
            return result;
        }

        private static SampleCompound missingMeasurements(Compound compound) {
            var result = new SampleCompound() {
                MeasuredSubstance = compound,
                ActiveSubstance = compound,
                Residue = double.NaN,
                ResType = ResType.MV,
                Loq = double.NaN,
                Lod = double.NaN,
            };
            return result;
        }
    }
}
