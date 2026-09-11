using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposurePercentilesByRouteSectionView : SectionView<ExposurePercentilesByRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                nameof(ExposurePercentileRecord.SubstanceCode),
                nameof(ExposurePercentileRecord.SubstanceName)
            };
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");
            if (Model.Records.All(c => String.IsNullOrEmpty(c.Route))) {
                hiddenProperties.Add(nameof(ExposurePercentileRecord.Route));
            }
            if (Model.Records.All(c => c.Values.Count == 0)) {
                hiddenProperties.Add(nameof(ExposurePercentileRecord.Median));
                hiddenProperties.Add(nameof(ExposurePercentileRecord.LowerBound));
                hiddenProperties.Add(nameof(ExposurePercentileRecord.UpperBound));
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
