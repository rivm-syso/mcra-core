using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SoilConcentrationsSummarySectionView : SectionView<SoilConcentrationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfSubstances = Model.Records.Select(r => r.SubstanceName).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentrations for {numberOfSubstances} substances.");

                // Download table
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                   name: "SoilPercentiles",
                   section: Model,
                   items: Model.PercentileRecords,
                   viewBag: ViewBag
               );

                // Chart
                var chartCreator = new SubstanceConcentrationsBoxPlotChartCreator(Model, "substance concentrations in soil");
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
                var hiddenProperties = new List<string>();
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SoilConcentrationsDataTable",
                    ViewBag,
                    caption: "Soil concentrations.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No soil concentrations available for the selected substances.");
            }
        }
    }
}
