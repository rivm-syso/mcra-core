using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UnitVariabilityFactorsSummarySectionView : SectionView<UnitVariabilityFactorsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.CompoundCode))) {
                hiddenProperties.Add("CompoundCode");
                hiddenProperties.Add("CompoundName");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ProcessingTypeDescription))) {
                hiddenProperties.Add("ProcessingTypeCode");
                hiddenProperties.Add("ProcessingTypeDescription");
            }
            if (Model.Records.All(r => double.IsNaN(r.CoefficientOfVariation))) {
                hiddenProperties.Add("CoefficientOfVariation");
            }
            if (Model.Records.All(r => double.IsNaN(r.UnitWeightRac))) {
                hiddenProperties.Add("UnitWeightRac");
            }

            //Render HTML
            if (Model.Records.Count == 0) {
                sb.AppendDescriptionParagraph("No unit variability records available");
            } else {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} unit variability factors.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "UnitVariabilityFactorsTable",
                    ViewBag,
                    caption: "Unit variability information per food (and substance).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
