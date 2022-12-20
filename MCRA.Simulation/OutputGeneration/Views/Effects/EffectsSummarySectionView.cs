using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EffectsSummarySectionView : SectionView<EffectsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => !r.IsMainEffect)) {
                hiddenProperties.Add("IsMainEffect");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalOrganisation))) {
                hiddenProperties.Add("BiologicalOrganisation");
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
