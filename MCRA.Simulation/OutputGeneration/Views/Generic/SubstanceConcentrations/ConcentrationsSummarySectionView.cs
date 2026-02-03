using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstanceConcentrationsSummarySectionView<Tsection> : SectionView<Tsection>
        where Tsection : SubstanceConcentrationsSummarySection {
        public override void RenderSectionHtml(StringBuilder sb) {

            var typeName = typeof(Tsection).Name;
            const string suffix = "Section";
            if (typeName.EndsWith(suffix, StringComparison.Ordinal)) {
                typeName = typeName[..^suffix.Length];
            }

            //Render HTML
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfSubstances = Model.Records.Select(r => r.SubstanceName).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentration records for {numberOfSubstances} substances.");

                // Download table
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                   name: $"{typeName}Percentiles",
                   section: Model,
                   items: Model.PercentileRecords,
                   viewBag: ViewBag
               );

                // Chart
                var chartCreator = new SubstanceConcentrationsBoxPlotChartCreator(Model, "substance concentrations in soil");
                sb.AppendChart(
                    $"{typeName}BoxPlotChart",
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
                    $"{typeName}DataTable",
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
