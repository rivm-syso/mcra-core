using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposuresSectionView : SectionView<ConsumerProductExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var numSubstances = Model.TotalSubstances;
            var numIndividuals = Model.TotalIndividuals;
            sb.AppendParagraph($"Simulated consumer product exposures (n = {numIndividuals} individuals) and for {numSubstances} substances.");
        }
    }
}
