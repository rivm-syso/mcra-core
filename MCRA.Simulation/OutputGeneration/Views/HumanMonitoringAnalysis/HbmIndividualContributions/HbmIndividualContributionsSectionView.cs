using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualContributionsSectionView
        : SectionView<HbmIndividualContributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var target = Model.Target;
            sb.Append(TableHelpers.CsvExportLink("IndividualHBMContributionsBoxPlotTable", Model, Model.HbmBoxPlotRecords, ViewBag, true, true));

            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new HbmIndividualContributionsBoxPlotChartCreator(Model, Model.ShowOutliers);
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

            if (Model.IndividualContributionRecords.All(c => double.IsNaN(c.LowerContributionPercentage))) {
                hiddenProperties.Add(nameof(IndividualContributionsRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(IndividualContributionsRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(IndividualContributionsRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(IndividualContributionsRecord.Contribution));
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add(nameof(IndividualContributionsRecord.ExposureRoute));
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add(nameof(IndividualContributionsRecord.BiologicalMatrix));
            }
            if (Model.IndividualContributionRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add(nameof(IndividualContributionsRecord.ExpressionType));
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

