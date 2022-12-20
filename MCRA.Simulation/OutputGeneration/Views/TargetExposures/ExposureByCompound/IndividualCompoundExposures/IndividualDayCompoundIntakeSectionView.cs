using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualDayCompoundIntakeSectionView : SectionView<IndividualDayCompoundIntakeSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendParagraph("Press the download link to download the individual day intakes per substance, " +
                      "expressed in equivalents of the reference substance. " +
                      "Note that only intakes > 0 are shown in this table.");

            if (Model.TruncatedIndividualDaysCount > 0) {
                sb.AppendParagraph($"Note: this table is truncated and limited to show the exposures for the first {Model.TruncatedIndividualDaysCount} individual days only.", "note");
            }
            sb.Append(TableHelpers.CsvExportLink("IndividualDayIntakesPerSubstanceTable", Model, Model.IndividualCompoundIntakeRecords, ViewBag, true, true));
        }
    }
}
