using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustExposuresSectionView : SectionView<DustExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.DustExposuresDataRecords.Count == 0) {
                sb.AppendParagraph("No dust exposures found/available for the specified scope.", "warning");
            } else {
                var numberOfSubstances = Model.DustExposuresDataRecords
                    .GroupBy(r => r.SubstanceCode)
                    .Count();
                var numberOfIndividuals = Model.TotalIndividuals;

                sb.AppendParagraph($"Simulated dust exposures for {numberOfSubstances} substances for {numberOfIndividuals} individuals.");
            }
        }
    }
}
