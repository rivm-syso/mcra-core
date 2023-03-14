using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections {
    public class HumanMonitoringSampleSubstanceCollectionsBuilder {

        /// <summary>
        /// Get all sample compound records for the current project.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="humanMonitoringSamples"></param>
        /// <param name="targetUnit"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public static List<HumanMonitoringSampleSubstanceCollection> Create(
            ICollection<Compound> substances,
            ICollection<HumanMonitoringSample> humanMonitoringSamples,
            ConcentrationUnit targetUnit,
            ICollection<HumanMonitoringSurvey> surveys,
            List<string> selectedSurveyCodes,
            CompositeProgressState progressState = null
        ) {
            var survey = surveys.Single(c => c.Code == selectedSurveyCodes.First());  
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = humanMonitoringSamples
                .GroupBy(r => r.SamplingMethod)
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(r => new HumanMonitoringSampleSubstanceCollection(
                    hbmSamplingMethod: r.Key,
                    hbmSampleSubstanceRecords: r
                        .Select(s => createFromSamples(s, substances, targetUnit))
                        .ToList(),
                    triglycConcentrationUnit: survey.TriglycConcentrationUnit,
                    lipidConcentrationUnit: survey.LipidConcentrationUnit,
                    creatConcentrationUnit: survey.CreatConcentrationUnit,
                    cholestConcentrationUnit: survey.CholestConcentrationUnit
                ))
                .ToList();
            return result;
        }

        /// <summary>
        /// Resamples the sample compound collection for a bootstrap run.
        /// </summary>
        /// <param name="humanMonitoringSampleSubstancesCollections"></param>
        /// <param name="seed"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public static List<HumanMonitoringSampleSubstanceCollection> ResampleSampleSubstancesCollections(
            ICollection<HumanMonitoringSampleSubstanceCollection> humanMonitoringSampleSubstancesCollections,
            int seed,
            CompositeProgressState progressState = null
        ) {
            var newSampleCompoundCollections = new ConcurrentBag<HumanMonitoringSampleSubstanceCollection>();
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            return humanMonitoringSampleSubstancesCollections
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(hmSampleCompoundCollection => {
                    var randomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, hmSampleCompoundCollection.GetHashCode()), true);
                    var newSampleSubstanceRecords = hmSampleCompoundCollection.HumanMonitoringSampleSubstanceRecords
                        .Resample(randomGenerator)
                        .Select(scr => new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = scr.HumanMonitoringSampleSubstances.Values
                                .ToDictionary(sc => sc.ActiveSubstance, sc => sc.Clone())
                        })
                        .ToList();

                    return new HumanMonitoringSampleSubstanceCollection(
                        hmSampleCompoundCollection.SamplingMethod, 
                        newSampleSubstanceRecords,
                        hmSampleCompoundCollection.TriglycConcentrationUnit,
                        hmSampleCompoundCollection.LipidConcentrationUnit,
                        hmSampleCompoundCollection.CreatConcentrationUnit,
                        hmSampleCompoundCollection.CholestConcentrationUnit
                    );
                })
                .ToList();
        }

        private static HumanMonitoringSampleSubstanceRecord createFromSamples(
            HumanMonitoringSample sample,
            ICollection<Compound> substances,
            ConcentrationUnit targetUnit
        ) {
            var hmSampleSubstanceRecord = new HumanMonitoringSampleSubstanceRecord() {
                HumanMonitoringSample = sample,
                HumanMonitoringSampleSubstances = substances.Select(substance => {
                    var substanceAnalyses = sample.SampleAnalyses
                        .Where(c => c.AnalyticalMethod.AnalyticalMethodCompounds.ContainsKey(substance))
                        .ToList();
                    var sampleSubstances = substanceAnalyses
                        .Select(c => {
                            var analyticalMethodCompound = c.AnalyticalMethod.AnalyticalMethodCompounds[substance];
                            c.Concentrations.TryGetValue(substance, out var sampleCompoundConcentration);
                            var alignmentFactor = analyticalMethodCompound
                                .GetConcentrationUnit()
                                .GetConcentrationAlignmentFactor(targetUnit, substance.MolecularMass);

                            var residue = sampleCompoundConcentration != null && sampleCompoundConcentration.ResType == ResType.VAL
                                ? sampleCompoundConcentration.Concentration.Value 
                                : double.NaN;
                            var lod = analyticalMethodCompound.LOD;
                            var loq = analyticalMethodCompound.LOQ;
                            var resType = ResType.VAL;
                            if (sampleCompoundConcentration == null) {
                                resType = !double.IsNaN(loq) ? ResType.LOQ : ResType.LOD;
                            } else {
                                resType = sampleCompoundConcentration.ResType;
                            }

                            return new SampleCompound() {
                                MeasuredSubstance = substance,
                                ActiveSubstance = substance,
                                ResType = resType,
                                Lod = alignmentFactor * lod,
                                Loq = alignmentFactor * loq,
                                Residue = alignmentFactor * residue,
                            };
                        })
                        .ToList();

                    var result = sampleSubstances.Any()
                        ? aggregateMeasurements(sampleSubstances)
                        : missingMeasurements(substance);
                    return result;
                }).ToDictionary(sc => sc.ActiveSubstance)
            };
            return hmSampleSubstanceRecord;
        }

        private static SampleCompound aggregateMeasurements(List<SampleCompound> sampleSubstances) {
            var residue = double.NaN;
            if (sampleSubstances.Any(r => !double.IsNaN(r.Residue))) {
                residue = sampleSubstances.Where(r => !double.IsNaN(r.Residue)).Average(r => r.Residue);
            }
            
            var resType = ResType.VAL;
            if (sampleSubstances.All(r => r.IsMissingValue)) {
                resType = ResType.MV;
            } else if (sampleSubstances.All(r => r.ResType == ResType.LOQ)) {
                resType = ResType.LOQ;
            } else if (sampleSubstances.All(r => r.ResType == ResType.LOD)) {
                resType = ResType.LOD;
            } else {
                resType = ResType.VAL;
            };

            var result = new SampleCompound() {
                MeasuredSubstance = sampleSubstances.First().MeasuredSubstance,
                ActiveSubstance = sampleSubstances.First().ActiveSubstance,
                Residue = residue,
                ResType = resType,
                Loq = sampleSubstances.Min(r => r.Loq),
                Lod = sampleSubstances.Min(r => r.Lod),
            };
            return result;
        }
        private static SampleCompound missingMeasurements(Compound substance) {
            var result = new SampleCompound() {
                MeasuredSubstance = substance,
                ActiveSubstance = substance,
                Residue = double.NaN,
                ResType = ResType.MV,
                Loq = double.NaN,
                Lod = double.NaN
            };
            return result;
        }
    }
}
