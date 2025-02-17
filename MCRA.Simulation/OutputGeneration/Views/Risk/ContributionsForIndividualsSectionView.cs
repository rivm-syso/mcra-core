using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;


namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionsForIndividualsSectionView : SectionView<ContributionsForIndividualsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var isUncertainty = false;
            if (Model.IndividualContributionRecords.All(c => c.Contributions.Any())) {
                isUncertainty = true;
            }

            sb.Append(TableHelpers.CsvExportLink(
                "IndividualContributionsBoxPlotTable", Model, Model.BoxPlotRecords, ViewBag, true, true)
            );

            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new IndividualContributionsBySubstanceBoxPlotChartCreator(
                Model,
                Model.ShowOutliers
            );
            sb.AppendChart(
                "SubstanceContributionsIndividualRisksBoxPlot",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true
            );
            var chartCreatorPie = new IndividualContributionsPieChartCreator(Model, isUncertainty);
            sb.AppendChart(
                "SubstanceContributionsIndividualRisksPieChart",
                chartCreatorPie,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreatorPie.Title,
                saveChartFile: true
            );
            sb.Append("</div>");

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

            hiddenProperties = [];

            if (Model.IndividualContributionRecords.All(c => double.IsNaN(c.LowerContributionPercentage))) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("Contribution");
                isUncertainty = true;
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add("ExpressionType");
            }

            sb.AppendTable(
                Model,
                Model.IndividualContributionRecords,
                "SubstanceContributionsIndividualRisksTable",
                ViewBag,
                displayLimit: 10,
                caption: "Mean contributions to risk for individuals.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}

