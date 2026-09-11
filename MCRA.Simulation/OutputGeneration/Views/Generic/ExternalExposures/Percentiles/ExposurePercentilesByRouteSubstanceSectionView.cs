using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposurePercentilesByRouteSubstanceSectionView : SectionView<ExposurePercentilesByRouteSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => String.IsNullOrEmpty(c.Route))) {
                hiddenProperties.Add(nameof(ExposurePercentileRecord.Route));
            }
            if (Model.Records.All(c => String.IsNullOrEmpty(c.SubstanceCode))) {
                hiddenProperties.Add(nameof(ExposurePercentileRecord.SubstanceCode));
                hiddenProperties.Add(nameof(ExposurePercentileRecord.SubstanceName));
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
