using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MolecularDockingModelsBindingEnergiesSectionView : SectionView<MolecularDockingModelsBindingEnergiesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendBase64Image(new MolecularDockingModelsBindingEnergiesChartCreator(Model));
        }
    }
}
