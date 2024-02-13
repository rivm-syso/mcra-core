using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticConversionFactorsSummarySection : SummarySection {

        public List<KineticConversionFactorSummaryRecord> KineticConversionFactorRecords { get; set; }

        /// <summary>
        /// Summarize kinetic model conversion factors
        /// </summary>
        /// <param name="conversionFactors"></param>
        public void Summarize(ICollection<KineticConversionFactor> conversionFactors) {
            KineticConversionFactorRecords = new List<KineticConversionFactorSummaryRecord>();
            if (conversionFactors.Any()) {
                foreach (var record in conversionFactors) {
                    var isAgeLower = record.KCFSubgroups.Any(c => c.AgeLower != null);
                    var isGender = record.KCFSubgroups.Any(c => c.Gender != GenderType.Undefined);
                    var result = new KineticConversionFactorSummaryRecord() {
                        SubstanceCodeFrom = record.SubstanceFrom.Code,
                        SubstanceNameFrom = record.SubstanceFrom.Name,
                        BiologicalMatrixFrom = record.BiologicalMatrixFrom != BiologicalMatrix.Undefined
                            ? record.BiologicalMatrixFrom.GetDisplayName() : null,
                        ExposureRouteFrom = record.ExposureRouteFrom != ExposureRoute.Undefined
                            ? record.ExposureRouteFrom.GetDisplayName() : null,
                        DoseUnitFrom = record.DoseUnitFrom.GetShortDisplayName(),
                        ExpressionTypeFrom = record.ExpressionTypeFrom != ExpressionType.None
                            ? record.ExpressionTypeFrom.GetDisplayName() : null,
                        SubstanceCodeTo = record.SubstanceTo.Code,
                        SubstanceNameTo = record.SubstanceTo.Name,
                        BiologicalMatrixTo = record.BiologicalMatrixTo != BiologicalMatrix.Undefined
                            ? record.BiologicalMatrixTo.GetDisplayName() : null,
                        ExposureRouteTo = record.ExposureRouteTo != ExposureRoute.Undefined
                            ? record.ExposureRouteTo.GetDisplayName() : null,
                        DoseUnitTo = record.DoseUnitTo.GetShortDisplayName(),
                        ExpressionTypeTo = record.ExpressionTypeTo != ExpressionType.None
                            ? record.ExpressionTypeTo.GetDisplayName() : null,
                        DistributionType = record.Distribution != BiomarkerConversionDistribution.Unspecified
                            ? record.Distribution.GetDisplayName() : null,
                        ConversionFactor = record.ConversionFactor,
                        UncertaintyUpper = record.UncertaintyUpper.HasValue
                            ? record.UncertaintyUpper.Value : double.NaN,
                        IsAgeLower = isAgeLower,
                        IsGender = isGender,
                        Both = isAgeLower && isGender
                    };
                    KineticConversionFactorRecords.Add(result);
                }
            }
        }
    }
}