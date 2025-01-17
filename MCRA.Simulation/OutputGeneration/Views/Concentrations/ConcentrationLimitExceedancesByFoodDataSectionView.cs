using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationLimitExceedancesByFoodDataSectionView : SectionView<ConcentrationLimitExceedancesByFoodDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Count > 0) {
                var limitExceedingSamplesCount = Model.Records.Sum(r => r.NumberOfSamplesExceedingLimit);
                var totalSamples = Model.Records.Sum(c => c.TotalNumberOfSamples);
                sb.AppendDescriptionParagraph(
                    $"Total {limitExceedingSamplesCount} samples out of {totalSamples} samples (of {Model.Records.Count} distinct foods) were omitted. " +
                    $"These were the samples with at least one substance concentration higher than {Model.ExceedanceFactionThreshold:0%} of the concentration limit value.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConcentrationLimitExceedancesByFoodDataTable",
                    ViewBag,
                    caption: "Concentration limit exceedances by food.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph(
                    $"No exceedances of more than {Model.ExceedanceFactionThreshold:0%} of the concentration limit value were detected."
                );
            }
        }
    }
}
