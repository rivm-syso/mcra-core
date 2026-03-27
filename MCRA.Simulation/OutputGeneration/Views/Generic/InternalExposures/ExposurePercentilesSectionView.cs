using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {

    public class ExposurePercentilesSectionView<S, T> : SectionView<InternalExposurePercentileSectionBase<S, T>>
        where S : IExposureContributorKey, new()
        where T : InternalExposurePercentileRecordBase<S>, new()
    {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Stratification))) {
                hiddenProperties.Add("Stratification");
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
                    $"By{Model.DescriptorKey}PercentilesTable",
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
