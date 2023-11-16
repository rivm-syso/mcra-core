using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;


namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionsForIndividualsSectionView : SectionView<ContributionsForIndividualsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //sb.Append("<div class=\"figure-container\">");
            var chartCreator = new IndividualContributionsBySubstanceBoxPlotChartCreator(Model);
            sb.AppendChart(
                "IndividualContributionsBoxPlotChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true
            );

            //sb.Append("</div>");
            var hiddenProperties = new List<string>() {
                "SampleTypeCode",
                "Description",
                "LOR",
                "NumberOfMeasurements",
                "NumberOfPositives",
                "SampleTypeCode",
                "Unit",
                "MinPositives",
                "MaxPositives"
            };
            if (Model.HbmBoxPlotRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            sb.AppendTable(
                Model,
                Model.HbmBoxPlotRecords,
                    "IndividualBoxPlotTable",
                    ViewBag,
                    caption: $"Contributions to risk for individuals.",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );

            hiddenProperties = new List<string>();
            var isUncertainty = false;
            if (Model.IndividualContributionRecords.All(c => double.IsNaN(c.LowerContributionPercentage))) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("Contribution");
                isUncertainty = true;
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
                hiddenProperties.Add("ExpressionType");
            }

            sb.AppendChart(
                "IndividualContributionsPieChart",
                new IndividualContributionsPieChartCreator(Model, isUncertainty),
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: "Mean contributions to risk for individuals.",
                saveChartFile: true
            );
            sb.AppendTable(
                Model,
                Model.IndividualContributionRecords,
                    "IndividualContributionsTable",
                    ViewBag,
                    caption: "Mean contributions to risk for individuals.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
        }
    }
}

