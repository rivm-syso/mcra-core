using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ErfDataSummarySectionView : SectionView<ErfDataSummarySection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add("ExpressionType");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PopulationCharacteristic))) {
                hiddenProperties.Add("PopulationCharacteristic");
            }
            if (Model.Records.All(r => r.EffectThresholdLower == null)) {
                hiddenProperties.Add("EffectThresholdLower");
            }
            if (Model.Records.All(r => r.EffectThresholdUpper == null)) {
                hiddenProperties.Add("EffectThresholdUpper");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureResponseSpecificationLower))) {
                hiddenProperties.Add("ExposureResponseSpecificationLower");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureResponseSpecificationUpper))) {
                hiddenProperties.Add("ExposureResponseSpecificationUpper");
            }
            if (Model.Records.All(r => !r.CfvUncertaintyLower.HasValue || double.IsNaN(r.CfvUncertaintyLower.Value))) {
                hiddenProperties.Add("CfvUncertaintyLower");
            }
            if (Model.Records.All(r => !r.CfvUncertaintyUpper.HasValue || double.IsNaN(r.CfvUncertaintyUpper.Value))) {
                hiddenProperties.Add("CfvUncertaintyUpper");
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
