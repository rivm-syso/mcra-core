using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SoilConcentrationDistributionsSummarySectionView : SectionView<SoilConcentrationDistributionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfSubstances = Model.Records.Select(r => r.SubstanceName).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentration distributions for {numberOfSubstances} substances.");

                var hiddenProperties = new List<string>();

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                   name: "SoilPercentiles",
                   section: Model,
                   items: Model.PercentileRecords,
                   viewBag: ViewBag
               );
                var chartCreator = new SoilDataBoxPlotChartCreator(Model);
                sb.AppendChart(
                    "SoilBoxPlotChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true,
                    chartData: percentileDataSection
                );

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SoilConcentrationDistributionsDataTable",
                    ViewBag,
                    caption: "Soil concentrations.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No soil concentration distributions available for the selected substances.");
            }
        }
    }
}
