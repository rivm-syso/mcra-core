using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySubstancePercentilesSectionView : SectionView<ExposureBySubstancePercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");

            var hiddenProperties = new List<string> {
                nameof(TargetExposurePercentileRecord.Route),
                nameof(TargetExposurePercentileRecord.Source)
            };
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Stratification))) {
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.Stratification));
            }
            if (Model.Records.All(c => c.Values.Count == 0)) {
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.Median));
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.LowerBound));
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.UpperBound));
            }

            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "BySubstancePercentilesTable",
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
