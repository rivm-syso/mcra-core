using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionFoodAsEatenSectionView : SectionView<TotalDistributionFoodAsEatenSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }
            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Sum(r => r.Total) > 0) {
                if (Model.HasOthers) {
                    sb.AppendParagraph("In this table, each row only summarizes risk driver components selected in the screening", "note");
                }
                if (Model.Records.Count() > 1) {
                    var chartCreator = new TotalDistributionFoodAsEatenPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "TotalDistributionFoodAsEatenChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            } else {
                sb.AppendDescriptionParagraph("No positive exposures found");
            }
            sb.AppendDescriptionParagraph($"Number of foods as eaten: {Model.Records.Count}");
            sb.AppendTable(
                Model,
                Model.Records,
                "TotalDistributionFoodAsEatenTable",
                ViewBag,
                caption: "Exposure statistics by food as eaten (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
