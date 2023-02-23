using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstanceConversionsDataSectionView : SectionView<SubstanceConversionsDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.TotalConversionRulesCount} substance conversion rules for {Model.TotalMeasuredSubstanceDefinitionsCount}. There are {Model.TotalComplexSubstanceConversionsCount} complex conversion rules and {Model.TotalIdentityTranslationsCount} identity conversion rules.");
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "SubstanceConversionRulesTable",
                   ViewBag,
                   caption: "Substance conversion rules.",
                   saveCsv: true,
                   header: true
                );
            } else {
                sb.AppendDescriptionParagraph("No substance conversion rules available.");
            }
        }
    }
}
