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

            if (Model.ExposureRecords.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }

            var isUncertainty = Model.ExposureRecords.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }


            //Render HTML
            if (Model.ExposureRecords.Any(r => r.MeanAll > 0)) {
                sb.AppendDescriptionParagraph($"Total {Model.ExposureRecords.Count} routes.");
                if (Model.ExposureRecords.Count > 1) {
                    var chartCreator = new ExternalExposuresByRoutePieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        $"{typeName}Chart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }

                sb.AppendTable(
                    Model,
                    Model.ExposureRecords,
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
