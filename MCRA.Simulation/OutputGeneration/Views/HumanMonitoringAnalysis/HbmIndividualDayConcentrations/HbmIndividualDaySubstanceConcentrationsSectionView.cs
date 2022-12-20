using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDaySubstanceConcentrationsSectionView : SectionView<HbmIndividualDaySubstanceConcentrationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<p>Press the download link to download the individual day monitoring concentrations per substance. " +
                      "Note that only concentrations > 0 are shown in this table.</p>");

            if (Model.TruncatedIndividualDaysCount > 0) {
                sb.AppendParagraph($"Note: this table is truncated and limited to show the exposures for the first {Model.TruncatedIndividualDaysCount} individual days only.", "note");
            }
            sb.Append(TableHelpers.CsvExportLink("HbmIndividualDaySubstanceConcentrationTable", Model, Model.Records, ViewBag, true, true));
        }
    }
}
