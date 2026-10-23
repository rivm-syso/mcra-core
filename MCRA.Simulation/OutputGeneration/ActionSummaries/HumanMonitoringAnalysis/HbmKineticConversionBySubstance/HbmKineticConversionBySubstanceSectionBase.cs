using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmKineticConversionBySubstanceSectionBase : SummarySection {
        public Dictionary<(string, string), KineticConversionModelType> KineticModelTypes { get; set; }
        public List<DerivedKineticConversionFactorModelSummaryRecord> Records { get; set; }

        protected static List<DerivedKineticConversionFactorModelSummaryRecord> Summarize(
            ICollection<HbmConcentrationBySubstanceCollection> convertedExposures,
            ICollection<HbmConcentrationBySubstanceCollection> measuredExposures,
            ICollection<Compound> substances,
            Dictionary<(string, string), KineticConversionModelType> kineticModelTypes
        ) {
            var measuredTargets = measuredExposures.Select(c => c.ExposureTarget).ToHashSet();
            var results = new List<(ExposureTarget MeasuredTarget, Compound Substance, TargetUnit TargetUnitTo, int simulatedId, KineticConversionInfo ConversionInfo)>();

            foreach (var measuredTarget in measuredTargets) {
                foreach (var substance in substances) {
                    var result = convertedExposures
                        .Select(converted => {
                            var measuredConcentration = measuredExposures
                                .Where(measured => measured.Id == converted.Id && measured.ExposureTarget == measuredTarget)
                                .SelectMany(m => m.ConcentrationsBySubstance.Where(q => q.Substance == substance).Select(m => m.Concentration))
                                .FirstOrDefault();
                            var (convertedConcentration, convertedTarget, targetUnitTo) = converted.ConcentrationsBySubstance
                                .Where(r => r.Substance == substance)
                                .Select(r => (r.Concentration, converted.ExposureTarget, converted.TargetUnit))
                                .FirstOrDefault();
                            var conversionInfo = new KineticConversionInfo() {
                                TargetFrom = measuredTarget,
                                TargetTo = convertedTarget,
                                ExposureFrom = measuredConcentration,
                                ExposureTo = convertedConcentration
                            };
                            return (measuredTarget, substance, targetUnitTo, converted.Id, conversionInfo);
                        }
                    ).ToList();

                    result = [.. result.Where(c => c.conversionInfo.TargetTo != null).Select(c => c)];
                    results.AddRange(result);
                }
            }
            var records = results.GroupBy(c => (c.Substance, c.MeasuredTarget))
                .Select(c => new DerivedKineticConversionFactorModelSummaryRecord() {
                    SubstanceNameFrom = c.Key.Substance.Name,
                    SubstanceCodeFrom = c.Key.Substance.Code,
                    SubstanceCodeTo = c.Key.Substance.Code,
                    SubstanceNameTo = c.Key.Substance.Name,
                    ExposurePairs = [.. c.Select(r => new ExposurePair(r.ConversionInfo.ExposureFrom, r.ConversionInfo.ExposureTo))],
                    ExposureRouteTo = c.Select(r => r.ConversionInfo.TargetTo?.ExposureRoute.GetShortDisplayName()).FirstOrDefault(),
                    BiologicalMatrixFrom = c.Key.MeasuredTarget.BiologicalMatrix.GetShortDisplayName(),
                    BiologicalMatrixTo = c.Select(r => r.ConversionInfo.TargetTo?.BiologicalMatrix.GetShortDisplayName()).FirstOrDefault(),
                    UnitFrom = c.Key.MeasuredTarget.BiologicalMatrix.GetTargetConcentrationUnit().GetShortDisplayName(),
                    UnitTo = c.Select(r => r.TargetUnitTo.GetShortDisplayName()).FirstOrDefault(),
                    NominalFactor = c.Average(r => r.ConversionInfo.ExposureTo / r.ConversionInfo.ExposureFrom),
                    KineticModelType = kineticModelTypes.TryGetValue((c.Key.Substance.Code, c.Key.MeasuredTarget.BiologicalMatrix.GetShortDisplayName()), out var kmType)
                        ? kmType.GetShortDisplayName()
                        : string.Empty
                }).ToList();
            return records;
        }

        protected static Dictionary<(string, string), KineticConversionModelType> GetKineticModelType(
           ICollection<Compound> substances,
           ICollection<BiologicalMatrix> matrices,
           ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
           ICollection<KineticModelInstance> kineticModelInstances
       ) {
            var kineticConversionModel = new Dictionary<(string, string),KineticConversionModelType>();
            var kmInstances = kineticModelInstances?.Select(c => (c.InputSubstance, Matrices: c.KineticModelDefinition.GetOutputMatrices())).ToList() ?? [];
            var kcfModels = kineticConversionFactorModels?.Select(c => (c.SubstanceFrom, Matrix: c.TargetFrom.BiologicalMatrix)).ToList() ?? [];
            foreach (var substance in substances) {
                foreach (var matrix in matrices) {
                    if (kmInstances.Where(c => c.InputSubstance == substance).SelectMany(c => c.Matrices).Contains(matrix)) {
                        kineticConversionModel[(substance.Code, matrix.GetShortDisplayName())] = KineticConversionModelType.PbkModel;
                    } else if (kcfModels.Where(c => c.SubstanceFrom == substance).FirstOrDefault().Matrix == matrix) {
                        kineticConversionModel[(substance.Code, matrix.GetShortDisplayName())] = KineticConversionModelType.ConversionFactorModel;
                    }
                }
            }
            return kineticConversionModel;
        }
    }
}
