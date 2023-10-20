using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmCumulativeIndividualDistributionsSectionView : SectionView<HbmCumulativeIndividualDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.Any()) {
                hiddenProperties.Add("SubstanceCode");
                if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                    hiddenProperties.Add("BiologicalMatrix");
                }

                var percentileDataSection = DataSectionHelper
                    .CreateCsvDataSection("CumulativeConcentrationsPercentiles", Model, Model.HbmBoxPlotRecords, ViewBag);
                var chartCreator = new HbmCumulativeIndividualDistributionsBoxPlotChartCreator(Model);

                sb.AppendChart(
                    "CumulativeConcentrationsBoxPlotChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true,
                    chartData: percentileDataSection
                );

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "CumulativeConcentrationsTable",
                    ViewBag,
                    caption: "Human monitoring individual measurement distribution endpoint cumulative substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph($"After removing individual days with missing values, no data are left.");
            }
        }
    }
}
