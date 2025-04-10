using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsInfinity(c.StandardizedTotalAttributableBod))) {
                hiddenProperties.Add("StandardizedTotalAttributableBod");
            }

            var isUncertainty = Model.Records.FirstOrDefault()?.TotalAttributableBods.Any() ?? false;
            if (isUncertainty) {
                hiddenProperties.Add("TotalAttributableBod");
                hiddenProperties.Add("StandardizedTotalAttributableBod");
            } else {
                hiddenProperties.Add("MedianTotalAttributableBod");
                hiddenProperties.Add("LowerTotalAttributableBod");
                hiddenProperties.Add("UpperTotalAttributableBod");
                hiddenProperties.Add("MedianStandardizedTotalAttributableBod");
                hiddenProperties.Add("LowerStandardizedTotalAttributableBod");
                hiddenProperties.Add("UpperStandardizedTotalAttributableBod");
            }

            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add("StandardizedTotalAttributableBod");
                hiddenProperties.Add("MedianStandardizedTotalAttributableBod");
                hiddenProperties.Add("LowerStandardizedTotalAttributableBod");
                hiddenProperties.Add("UpperStandardizedTotalAttributableBod");
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
