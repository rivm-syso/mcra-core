using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposuresSectionView : SectionView<ConsumerProductExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.ConsumerProductExposuresDataRecords.Count == 0) {
                sb.AppendParagraph("No consumer product exposures found/available for the specified scope.", "warning");
            } else {
                var numberOfSubstances = Model.ConsumerProductExposuresDataRecords
                    .GroupBy(r => r.SubstanceCode)
                    .Count();
                var numberOfIndividuals = Model.TotalIndividuals;

                sb.AppendParagraph($"Simulated consumer product exposures for {numberOfSubstances} substances for {numberOfIndividuals} individuals.");
            }
        }
    }
}
