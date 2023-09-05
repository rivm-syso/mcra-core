using DocumentFormat.OpenXml.Wordprocessing;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections {
    public class HumanMonitoringSampleSubstanceCollectionsBuilder {

        /// <summary>
        /// Get all sample compound records for the current project.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="humanMonitoringSamples"></param>
        /// <param name="survey"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public static List<HumanMonitoringSampleSubstanceCollection> Create(
            ICollection<Compound> substances,
            ICollection<HumanMonitoringSample> humanMonitoringSamples,
            HumanMonitoringSurvey survey,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new CancellationToken();
            var result = humanMonitoringSamples
                .GroupBy(r => r.SamplingMethod)
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(r => {
                    var targetConcentrationUnit = r.Key.BiologicalMatrix.GetTargetConcentrationUnit();
                    return new HumanMonitoringSampleSubstanceCollection(
                        hbmSamplingMethod: r.Key,
                        hbmSampleSubstanceRecords: r
                            .Select(s => createFromSamples(s, substances, targetConcentrationUnit))
                            .ToList(),
                        targetConcentrationUnit: targetConcentrationUnit,
                        expressionType: ExpressionType.None,
                        triglycConcentrationUnit: survey.TriglycConcentrationUnit,
                        lipidConcentrationUnit: survey.LipidConcentrationUnit,
                        creatConcentrationUnit: survey.CreatConcentrationUnit,
                        cholestConcentrationUnit: survey.CholestConcentrationUnit
                    );
                })
                .OrderBy(c => c.SamplingMethod.Code)
                .ToList();

            return result;
        }

        private static HumanMonitoringSampleSubstanceRecord createFromSamples(
            HumanMonitoringSample sample,
            ICollection<Compound> substances,
            ConcentrationUnit targetConcentrationUnit
        ) {
            var hmSampleSubstanceRecord = new HumanMonitoringSampleSubstanceRecord() {
                HumanMonitoringSample = sample,
                HumanMonitoringSampleSubstances = substances
                    .Select(substance => {
                        var substanceAnalyses = sample.SampleAnalyses
                            .Where(c => c.AnalyticalMethod.AnalyticalMethodCompounds.ContainsKey(substance))
                            .ToList();
                        var sampleSubstances = substanceAnalyses
                            .Select(c => {
                                var analyticalMethodCompound = c.AnalyticalMethod.AnalyticalMethodCompounds[substance];
                                c.Concentrations.TryGetValue(substance, out var sampleCompoundConcentration);
                                var alignmentFactor = analyticalMethodCompound
                                    .GetConcentrationUnit()
                                    .GetConcentrationAlignmentFactor(targetConcentrationUnit, substance.MolecularMass);

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
                    })
                    .ToDictionary(sc => sc.ActiveSubstance)
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
