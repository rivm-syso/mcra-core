using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualSubstanceExposureSectionView : SectionView<IndividualSubstanceExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendParagraph(
                "Press the download link to download the individual intakes (OIM) per substance, " +
                "expressed in equivalents of the reference substance. " +
                "Note that only intakes > 0 are shown in this table."
            );

            if (Model.TruncatedIndividualsCount > 0) {
                sb.AppendParagraph($"Note: this table is truncated and limited to show the exposures for the first {Model.TruncatedIndividualsCount} individuals only.", "note");
            }
            sb.Append(TableHelpers.CsvExportLink("IndividualIntakesPerSubstanceTable", Model, Model.Records, ViewBag, true, true));
        }
    }
}
