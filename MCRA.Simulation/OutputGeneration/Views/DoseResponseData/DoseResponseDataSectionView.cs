using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseDataSectionView : SectionView<DoseResponseDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ResponseUnit))) {
                hiddenProperties.Add("ResponseUnit");
            }

            //Render HTML
            sb.AppendTable(
                Model,
                Model.Records,
                "DoseResponseExperimentsTable",
                ViewBag,
                caption: "Dose response experiments.",
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
