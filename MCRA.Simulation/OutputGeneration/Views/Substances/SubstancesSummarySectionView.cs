using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstancesSummarySectionView : SectionView<SubstancesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            // Description
            var description = $"The scope contains {Model.Records.Count} substances.";
            sb.AppendDescriptionParagraph(description);

            // Table
            var hiddenProperties = new List<string>();
            var hasCramerClassInfo = Model.Records.All(r => string.IsNullOrEmpty(r.CramerClass));
            if (hasCramerClassInfo) {
                hiddenProperties.Add("CramerClass");
            }
            var hasMolecularWeights = Model.Records.All(r => double.IsNaN(r.MolecularWeight) || r.MolecularWeight == 0D);
            if (hasMolecularWeights) {
                hiddenProperties.Add("MolecularWeight");
            }
            sb.AppendTable(
               Model,
               Model.Records,
               "SubstancesTable",
               ViewBag,
               caption: "Substances.",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
