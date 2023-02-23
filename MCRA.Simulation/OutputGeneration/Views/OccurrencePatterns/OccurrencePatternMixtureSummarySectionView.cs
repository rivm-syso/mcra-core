using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccurrencePatternMixtureSummarySectionView : SectionView<OccurrencePatternMixtureSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => r.FromAuthorisedUse == null)) {
                hiddenProperties.Add("FromAuthorisedUse");
            }
            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "AgriculturalUseMixtureTable",
               ViewBag,
               caption: "Occurrence patterns/mixtures.",
               saveCsv: true,
               header: true,
               displayLimit: 20,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
