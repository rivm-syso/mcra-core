using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HClustSectionView : SectionView<HClustSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.AutomaticallyDetermineNumberOfClusters) {
                sb.AppendDescriptionParagraph($"Individuals are clustered using hierarchical clustering with Ward's criterion. The number of clusters is {Model.Clusters.Count}.");
                sb.AppendDescriptionParagraph($"The number of clusters is automatically determined.");
            } else {
                sb.AppendDescriptionParagraph($"Individuals are clustered using hierarchical clustering with Ward's criterion. The specified number of clusters is {Model.Clusters.Count}.");
            }
            sb.AppendDescriptionParagraph($"Largest cluster: {Model.LargestCluster}, contains {Model.MaximumSize} individuals.");
            sb.AppendDescriptionParagraph($"Smallest cluster: {Model.SmallestCluster}, contains {Model.MinimumSize} individuals.");

            var chartCreator = new HClustChartCreator(Model);
            sb.AppendChart(
                    "HClustChart",
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