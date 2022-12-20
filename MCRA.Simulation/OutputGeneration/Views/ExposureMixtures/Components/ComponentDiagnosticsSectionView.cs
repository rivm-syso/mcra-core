using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ComponentDiagnosticsSectionView : SectionView<ComponentDiagnosticsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            //Render HTML
            if (Model.Records != null) {
                sb.AppendDescriptionParagraph($"Exposures are {Model.ExposureApproachType.GetDisplayName().ToLower()}; {Model.RRMSEdifference.Count + 1} components are estimated.");
                sb.AppendDescriptionParagraph($"The optimal number of components lies between {Model.Optimum1} (first optimum) and {Model.Optimum2} (second optimum). ");
                var chartCreatorRMSE = new RMSEChartCreator(Model);
                sb.AppendChart(
                        "RMSEChart",
                        chartCreatorRMSE,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreatorRMSE.Title,
                        true
                    );

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "MixturesInformationTableFull",
                    ViewBag,
                    caption: $"Characteristics of {Model.Records.Count} components.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No diagnostics available when estimated number of components 1 or 2.");

            }

            var uMatrixDataSection = DataSectionHelper.CreateCsvDataSection(
                "UMatrix",
                Model,
                ViewBag,
                (r) => Model.WriteUMatrixCsv(r)
            );
            sb.AppendDescriptionParagraph($"Download link for U-matrix (substances x components).");
            sb.Append(TableHelpers.CsvExportLink(uMatrixDataSection));
        }
    }
}
