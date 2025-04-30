using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsInfinity(c.StandardisedTotalAttributableBod))) {
                hiddenProperties.Add("StandardisedTotalAttributableBod");
            }

            var isUncertainty = Model.Records.FirstOrDefault()?.TotalAttributableBods.Any() ?? false;
            if (isUncertainty) {
                hiddenProperties.Add("TotalAttributableBod");
                hiddenProperties.Add("StandardisedTotalAttributableBod");
            } else {
                hiddenProperties.Add("MedianTotalAttributableBod");
                hiddenProperties.Add("LowerTotalAttributableBod");
                hiddenProperties.Add("UpperTotalAttributableBod");
                hiddenProperties.Add("MedianStandardisedTotalAttributableBod");
                hiddenProperties.Add("LowerStandardisedTotalAttributableBod");
                hiddenProperties.Add("UpperStandardisedTotalAttributableBod");
            }

            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add("StandardisedTotalAttributableBod");
                hiddenProperties.Add("MedianStandardisedTotalAttributableBod");
                hiddenProperties.Add("LowerStandardisedTotalAttributableBod");
                hiddenProperties.Add("UpperStandardisedTotalAttributableBod");
            }

            sb.AppendTable(
            Model,
            Model.Records,
            "EnvironmentalBurdenOfDiseaseSummaryTable",
            ViewBag,
            caption: "Environmental burden of disease summary table.",
            saveCsv: true,
            sortable: false,
            hiddenProperties: hiddenProperties
        );
        }
    }
}
