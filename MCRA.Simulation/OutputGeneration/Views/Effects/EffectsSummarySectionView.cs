using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EffectsSummarySectionView : SectionView<EffectsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => !r.IsMainEffect)) {
                hiddenProperties.Add(nameof(EffectsSummaryRecord.IsMainEffect));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalOrganisation))) {
                hiddenProperties.Add(nameof(EffectsSummaryRecord.BiologicalOrganisation));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                hiddenProperties.Add(nameof(EffectsSummaryRecord.Description));
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "EffectsTable",
                ViewBag,
                header: true,
                caption: "Effects information.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties);
        }
    }
}
