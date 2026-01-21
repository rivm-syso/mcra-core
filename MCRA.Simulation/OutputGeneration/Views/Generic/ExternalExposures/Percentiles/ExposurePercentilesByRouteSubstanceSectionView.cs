using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposurePercentilesByRouteSubstanceSectionView : SectionView<ExposurePercentilesByRouteSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => String.IsNullOrEmpty(c.Route))) {
                hiddenProperties.Add("Route");
            }
            if (Model.Records.All(c => String.IsNullOrEmpty(c.SubstanceCode))) {
                hiddenProperties.Add("SubstanceCode");
                hiddenProperties.Add("SubstanceName");
            }
            if (Model.Records.All(c => c.Values.Count == 0)) {
                hiddenProperties.Add("Median");
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
            }

            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "PercentileByRouteSubstanceTable",
                    ViewBag,
                    header: true,
                    caption: "Percentiles",
                    saveCsv: true,
                    sortable: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
