using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseModelsDetailsSectionView : SectionView<DoseResponseModelsDetailsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendTable(
                Model,
                Model.DoseResponseModels,
                "DoseResponseModelsDetailsOverviewTable",
                ViewBag,
                caption: "Dose response models details overview.",
                header: true,
                saveCsv: true
            );
        }
    }
}
