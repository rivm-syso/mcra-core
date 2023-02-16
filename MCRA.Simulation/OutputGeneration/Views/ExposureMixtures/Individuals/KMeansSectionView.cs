using MCRA.Simulation.OutputGeneration.Helpers;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KMeansSectionView : SectionView<KMeansSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Individuals are clustered using k-means clustering. The number of clusters is {Model.Clusters.Count}.");
            sb.AppendDescriptionParagraph($"Largest cluster: {Model.LargestCluster}, contains {Model.MaximumSize} individuals.");
            sb.AppendDescriptionParagraph($"Smallest cluster: {Model.SmallestCluster}, contains {Model.MinimumSize} individuals.");
            var chartCreator = new KMeansChartCreator(Model);
            sb.AppendChart(
                    "KMeansChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            sb.AppendTable(
               Model,
               Model.Records,
               $"Properties",
               ViewBag,
               caption: $"Property characteristics.",
               saveCsv: true,
               header: true
            );
            var propertyDataSection = DataSectionHelper.CreateCsvDataSection(
                "Properties",
                Model,
                ViewBag,
                (r) => Model.WritePropertiesCsv(r)
            );
            sb.AppendDescriptionParagraph($"Download link for summary of properties for population and subgroups (reformatted).");
            sb.Append(TableHelpers.CsvExportLink(propertyDataSection));
            var vMatrixDataSection = DataSectionHelper.CreateCsvDataSection(
                "VMatrix",
                Model,
                ViewBag,
                (r) => Model.WriteVMatrixCsv(r)
            );
            sb.AppendDescriptionParagraph($"Download link for V-matrix (components x individuals).");
            sb.Append(TableHelpers.CsvExportLink(vMatrixDataSection));
        }
    }
}