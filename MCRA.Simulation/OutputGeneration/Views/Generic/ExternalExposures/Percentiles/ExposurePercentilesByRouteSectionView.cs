using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposurePercentilesByRouteSectionView : SectionView<ExposurePercentilesByRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("SubstanceCode");
            hiddenProperties.Add("SubstanceName");
            hiddenProperties.Add("Source");
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");
            if (Model.Records.All(c => String.IsNullOrEmpty(c.Route))) {
                hiddenProperties.Add("Route");
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
                    "PercentileByRouteTable",
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
