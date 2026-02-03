using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticConversionFactorsDataSummarySection : SummarySection {

        public List<KineticConversionFactorsDataSummaryRecord> Records { get; set; } = [];

        /// <summary>
        /// Summarize kinetic conversion factors.
        /// </summary>
        public void Summarize(ICollection<KineticConversionFactor> conversionFactors) {
            Records = getSummaryRecords(conversionFactors);
        }

        private static List<KineticConversionFactorsDataSummaryRecord> getSummaryRecords(
            ICollection<KineticConversionFactor> conversionFactors
        ) {
            var result = new List<KineticConversionFactorsDataSummaryRecord>();
            foreach (var conversionRule in conversionFactors) {
                var record = new KineticConversionFactorsDataSummaryRecord() {
                    KcfModelCode = conversionRule.IdKineticConversionFactor,
                    SubstanceCodeFrom = conversionRule.SubstanceFrom.Code,
                    SubstanceNameFrom = conversionRule.SubstanceFrom.Name,
                    BiologicalMatrixFrom = conversionRule.BiologicalMatrixFrom != BiologicalMatrix.Undefined
                            ? conversionRule.BiologicalMatrixFrom.GetDisplayName() : null,
                    ExposureRouteFrom = conversionRule.ExposureRouteFrom != ExposureRoute.Undefined
                            ? conversionRule.ExposureRouteFrom.GetDisplayName() : null,
                    DoseUnitFrom = conversionRule.DoseUnitFrom.GetShortDisplayName(),
                    ExpressionTypeFrom = conversionRule.ExpressionTypeFrom != ExpressionType.None
                            ? conversionRule.ExpressionTypeFrom.GetDisplayName() : null,
                    SubstanceCodeTo = conversionRule.SubstanceTo.Code,
                    SubstanceNameTo = conversionRule.SubstanceTo.Name,
                    BiologicalMatrixTo = conversionRule.BiologicalMatrixTo != BiologicalMatrix.Undefined
                            ? conversionRule.BiologicalMatrixTo.GetDisplayName() : null,
                    ExposureRouteTo = conversionRule.ExposureRouteTo != ExposureRoute.Undefined
                            ? conversionRule.ExposureRouteTo.GetDisplayName() : null,
                    DoseUnitTo = conversionRule.DoseUnitTo.GetShortDisplayName(),
                    ExpressionTypeTo = conversionRule.ExpressionTypeTo != ExpressionType.None
                            ? conversionRule.ExpressionTypeTo.GetDisplayName() : null,
                    DistributionType = conversionRule.Distribution != KineticConversionFactorDistributionType.Unspecified
                        ? conversionRule.Distribution.GetDisplayName() : null,
                    ConversionFactor = conversionRule.ConversionFactor,
                    UncertaintyUpper = conversionRule.UncertaintyUpper ?? double.NaN,
                    HasCovariateAge = conversionRule.KCFSubgroups.Any(r => r.AgeLower != null),
                    HasCovariateSex = conversionRule.KCFSubgroups.Any(r => r.Gender != GenderType.Undefined)
                };
                result.Add(record);
            }
            return result;
        }
    }
}