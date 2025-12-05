using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ErfDataSummarySectionView : SectionView<ErfDataSummarySection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.ExposureRoute));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.BiologicalMatrix));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.ExpressionType));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PopulationCharacteristic))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.PopulationCharacteristic));
            }
            if (Model.Records.All(r => r.EffectThresholdLower == null)) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.EffectThresholdLower));
            }
            if (Model.Records.All(r => r.EffectThresholdUpper == null)) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.EffectThresholdUpper));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ErSpecificationUncertaintyType))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.ErSpecificationUncertaintyType));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ErSpecificationUncLower))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.ErSpecificationUncLower));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ErSpecificationUncUpper))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.ErSpecificationUncUpper));
            }
            if (Model.Records.All(r => !r.CfvUncertaintyLower.HasValue || double.IsNaN(r.CfvUncertaintyLower.Value))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.CfvUncertaintyLower));
            }
            if (Model.Records.All(r => !r.CfvUncertaintyUpper.HasValue || double.IsNaN(r.CfvUncertaintyUpper.Value))) {
                hiddenProperties.Add(nameof(ErfDataSummaryRecord.CfvUncertaintyUpper));
            }

            sb.AppendTable(
                Model,
                Model.Records,
                "ErfDataSummaryTable",
                ViewBag,
                header: true,
                caption: "Exposure response functions information.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
