using System.Text;
using MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposuresByRouteSectionView<Tsection> : SectionView<Tsection>
        where Tsection : ExternalExposuresByRouteSection {
        public override void RenderSectionHtml(StringBuilder sb) {

            var typeName = typeof(Tsection).Name;
            const string suffix = "Section";
            if (typeName.EndsWith(suffix, StringComparison.Ordinal)) {
                typeName = typeName[..^suffix.Length];
            }

            var hiddenProperties = new List<string>();

            if (Model.TableRecords.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }

            //Render HTML
            if (Model.TableRecords.Any(r => r.MeanAll > 0)) {
                var chartCreator = new ExternalExposuresByRouteBoxPlotChartCreator(
                    Model,
                    Model.BoxPlotRecords,
                    Model.ExposureUnit
                );
                var warning = Model.BoxPlotRecords.Any(c => c.P95 == 0)
                       ? "The asterisk indicates substances with positive measurements above an upper whisker of zero."
                       : string.Empty;
                var figCaption = $"Exposures by route. " + chartCreator.Title + $" {warning}";
                sb.AppendChart(
                    $"{typeName}Chart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    figCaption,
                    true
                );

                sb.AppendTable(
                    Model,
                    Model.TableRecords,
                    $"{typeName}Table",
                    ViewBag,
                    caption: "Exposure statistics by route (total distribution).",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendParagraph("No positive exposures found", "warning");
            }
        }
    }
}
