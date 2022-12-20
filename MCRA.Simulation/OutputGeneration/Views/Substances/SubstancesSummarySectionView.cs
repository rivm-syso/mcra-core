using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstancesSummarySectionView : SectionView<SubstancesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.CramerClass))) {
                hiddenProperties.Add("CramerClass");
            }
            hiddenProperties.Add("IsReference");
            var reference = Model.Records.Where(c => c.IsReference).FirstOrDefault();
            if (Model.Records.All(r => double.IsNaN(r.MolecularWeight) || r.MolecularWeight == 0D)) {
                hiddenProperties.Add("MolecularWeight");
            }

            // Description
            var description = $"The scope contains {Model.Records.Count} substances.";
            if (Model.Records.Any(r => r.IsReference)) {
                description += $" The index substance is {reference.CompoundName} ({reference.CompoundCode}).";
            }
            sb.AppendDescriptionParagraph(description);

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "SubstancesTable",
               ViewBag,
               caption: "Substance names, codes (and if present, molecular weights and Cramer classes).",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
