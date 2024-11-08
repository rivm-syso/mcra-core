using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticConversionFactorsSummarySection : SummarySection {

        public List<KineticConversionFactorSummaryRecord> KineticConversionFactorRecords { get; set; } = [];

        /// <summary>
        /// Summarize kinetic model conversion factors
        /// </summary>
        /// <param name="conversionFactors"></param>
        public void Summarize(ICollection<IKineticConversionFactorModel> conversionFactorModels) {
            KineticConversionFactorRecords = [];
            conversionFactorModels = conversionFactorModels.Where(c => c.ConversionRule.SubstanceFrom != null).ToList();
            if (conversionFactorModels.Any()) {
                foreach (var record in conversionFactorModels) {
                    var conversionRule = record.ConversionRule;
                    var isAgeLower = conversionRule.KCFSubgroups.Any(c => c.AgeLower != null);
                    var isGender = conversionRule.KCFSubgroups.Any(c => c.Gender != GenderType.Undefined);
                    var result = new KineticConversionFactorSummaryRecord() {
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
                        UncertaintyUpper = conversionRule.UncertaintyUpper.HasValue
                            ? conversionRule.UncertaintyUpper.Value : double.NaN,
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