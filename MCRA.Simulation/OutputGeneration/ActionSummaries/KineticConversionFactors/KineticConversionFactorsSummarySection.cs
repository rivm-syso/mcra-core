using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticConversionFactorsSummarySection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<KineticConversionFactorSummaryRecord> Records { get; set; } = [];

        /// <summary>
        /// Summarize kinetic model conversion factors.
        /// </summary>
        public void Summarize(ICollection<IKineticConversionFactorModel> conversionFactorModels) {
            Records = [];
            var records = getSummaryRecords(conversionFactorModels);
            Records = records;
        }

        /// <summary>
        /// Summarize drawn kinetic model conversion factors of uncertainty runs.
        /// </summary>
        public void SummarizeUncertain(ICollection<IKineticConversionFactorModel> conversionFactorModels) {
            var recordsLookup = Records
                .ToDictionary(r => (r.KcfModelCode, r.AgeLower, r.Sex));
            var records = getSummaryRecords(conversionFactorModels);
            foreach (var record in records) {
                if (recordsLookup.TryGetValue((record.KcfModelCode, record.AgeLower, record.Sex), out var sectionRecord)) {
                    sectionRecord.UncertaintyValues ??= [];
                    sectionRecord.UncertaintyValues.Add(record.NominalFactor);
                }
            }
        }

        private static List<KineticConversionFactorSummaryRecord> getSummaryRecords(ICollection<IKineticConversionFactorModel> conversionFactorModels) {
            var records = new List<KineticConversionFactorSummaryRecord>();
            foreach (var model in conversionFactorModels) {
                foreach (var factor in model.GetParametrisations()) {
                    var conversionRule = model.ConversionRule;
                    var isAgeLower = conversionRule.KCFSubgroups.Any(c => c.AgeLower != null);
                    var isGender = conversionRule.KCFSubgroups.Any(c => c.Gender != GenderType.Undefined);
                    var result = new KineticConversionFactorSummaryRecord() {
                        KcfModelCode = model.ConversionRule.IdKineticConversionFactor,
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
                        NominalFactor = factor.Factor,
                        AgeLower = factor.Age,
                        Sex = factor.Gender != GenderType.Undefined ? factor.Gender.GetDisplayName() : null
                    };
                    records.Add(result);
                }
            }
            return records;
        }
    }
}