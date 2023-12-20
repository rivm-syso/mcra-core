using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualContributionsSectionView : SectionView<HbmIndividualContributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var target = Model.Target;

            sb.Append("<div class=\"figure-container\">");
            
            var chartCreator = new HbmIndividualContributionsBoxPlotChartCreator(Model);
            sb.AppendChart(
                name: $"IndividualContributionsChart{target}",
                section: Model,
                viewBag: ViewBag,
                chartCreator: chartCreator,
                fileType: ChartFileType.Svg,
                saveChartFile: true,
                caption: chartCreator.Title
            );

            var chartCreatorPie = new HbmIndividualContributionsPieChartCreator(Model, false);
            sb.AppendChart(
                name: $"MeanContributionsChart{target}",
                section: Model,
                chartCreator: chartCreatorPie,
                viewBag: ViewBag,
                fileType: ChartFileType.Svg,
                saveChartFile: true,
                caption: chartCreatorPie.Title
            );

            sb.Append("</div>");

            var hiddenProperties = new List<string>();
            var allBoxplotRecords = Model.HbmBoxPlotRecords.ToList();
            if (Model.HbmBoxPlotRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.HbmBoxPlotRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.HbmBoxPlotRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add("ExpressionType");
            }
            sb.AppendTable(
                Model,
                allBoxplotRecords,
                "IndividualHbmConcentrationsBoxPlotTable",
                ViewBag,
                caption: "Mean contributions to HBM concentrations for individuals.",
                saveCsv: true,
                displayLimit: 20,
                hiddenProperties: hiddenProperties
            );

            hiddenProperties = new List<string>();
            if (Model.IndividualContributionRecords.All(c => double.IsNaN(c.LowerContributionPercentage))) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("Contribution");
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
                "IndividualHbmConcentrationsContributionsTable",
                ViewBag,
                caption: "Mean contributions to HBM concentrations for individuals.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}

