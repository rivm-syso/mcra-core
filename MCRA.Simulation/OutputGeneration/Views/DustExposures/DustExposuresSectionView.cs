using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustExposuresSectionView : SectionView<DustExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var numSubstances = Model.TotalSubstances;
            var numIndividuals = Model.TotalIndividuals;
            sb.AppendParagraph($"Simulated dust exposures (n = {numIndividuals} individuals) and for {numSubstances} substances.");
        }
    }
}
