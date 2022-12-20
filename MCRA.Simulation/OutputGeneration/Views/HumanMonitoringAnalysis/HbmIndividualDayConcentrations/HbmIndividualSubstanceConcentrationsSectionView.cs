using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualSubstanceConcentrationsSectionView : SectionView<HbmIndividualSubstanceConcentrationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<p>Press the download link to download the individual monitoring concentrations per substance. " +
                      "Note that only concentrations > 0 are shown in this table.</p>");

            if (Model.TruncatedIndividualDaysCount > 0) {
                sb.AppendParagraph($"Note: this table is truncated and limited to show the exposures for the first {Model.TruncatedIndividualDaysCount} individual only.", "note");
            }
            sb.Append(TableHelpers.CsvExportLink("HbmIndividualSubstanceConcentrationTable", Model, Model.Records, ViewBag, true, true));
        }
    }
}
