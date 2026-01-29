using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceRoutePercentilesSectionView : SectionView<ExposureBySourceRoutePercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");
            var hiddenProperties = new List<string> {
                "SubstanceCode",
                "SubstanceName"
            };
            if (Model.Records.All(c => c.Values.Count == 0)) {
                hiddenProperties.Add("Median");
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
            }

            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "BySourcePercentilesTable",
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
