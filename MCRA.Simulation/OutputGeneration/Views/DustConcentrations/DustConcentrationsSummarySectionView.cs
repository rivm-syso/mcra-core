using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustConcentrationsSummarySectionView : SectionView<DustConcentrationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfSubstances = Model.Records.Select(r => r.SubstanceName).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentrations  for {numberOfSubstances} substances.");

                // Download table
                if (Model.PercentileRecords?.Count > 0) {
                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: "DustPercentiles",
                        section: Model,
                        items: Model.PercentileRecords,
                        viewBag: ViewBag
                    );

                    // Chart
                    var chartCreator = new SubstanceConcentrationsBoxPlotChartCreator(Model, "substance concentrations in dust");
                    sb.AppendChart(
                        "DustBoxPlotChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true,
                        chartData: percentileDataSection
                    );
                }

                // Table
                var hiddenProperties = new List<string>();
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "DustConcentrationsDataTable",
                    ViewBag,
                    caption: "Dust concentrations.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No dust concentrations available for the selected substances.");
            }
        }
    }
}
