using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AirExposuresSectionView : SectionView<AirExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.AirExposuresDataRecords.Count == 0) {
                sb.AppendParagraph("No air exposures found/available for the specified scope.", "warning");
            } else {
                var numberOfSubstances = Model.AirExposuresDataRecords
                    .GroupBy(r => r.SubstanceCode)
                    .Count();
                var numberOfIndividuals = Model.TotalIndividuals;

                sb.AppendParagraph($"Simulated air exposures for {numberOfSubstances} substances for {numberOfIndividuals} individuals.");
            }
        }
    }
}
