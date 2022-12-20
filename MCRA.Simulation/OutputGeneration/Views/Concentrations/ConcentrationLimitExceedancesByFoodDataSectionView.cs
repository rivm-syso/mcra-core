using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationLimitExceedancesByFoodDataSectionView : SectionView<ConcentrationLimitExceedancesByFoodDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records?.Any() ?? false) {
                var limitExceedingSamplesCount = Model.Records.Sum(r => r.NumberOfSamplesExceedingLimit);
                sb.AppendDescriptionParagraph($"Total {limitExceedingSamplesCount} samples out of {Model.Records.Sum(c => c.TotalNumberOfSamples)} samples (of {Model.Records.Count} distinct foods) were omitted. These were the samples with at least one substance concentration higher than {Model.ExceedanceFactionThreshold:P1} of the limit value as provided in the concentration limit data file.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConcentrationLimitExceedancesByFoodDataTable",
                    ViewBag,
                    caption: "Concentration limits exceedances by food.",
                    header: true,
                    saveCsv: true
                ); 
            } else {
                sb.AppendDescriptionParagraph($"No concentration limits exceedances of more than {Model.ExceedanceFactionThreshold:P1} of the limit value were detected.");
            }
        }
    }
}
