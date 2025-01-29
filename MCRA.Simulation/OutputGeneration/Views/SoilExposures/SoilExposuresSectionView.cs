using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SoilExposuresSectionView : SectionView<SoilExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.SoilExposuresDataRecords.Count == 0) {
                sb.AppendParagraph("No soil exposures found/available for the specified scope.", "warning");
            } else {
                var numberOfSubstances = Model.SoilExposuresDataRecords
                    .GroupBy(r => r.SubstanceCode)
                    .Count();
                var numberOfIndividuals = Model.TotalIndividuals;

                sb.AppendParagraph($"Simulated soil exposures for {numberOfSubstances} substances for {numberOfIndividuals} individuals.");
            }
        }
    }
}
