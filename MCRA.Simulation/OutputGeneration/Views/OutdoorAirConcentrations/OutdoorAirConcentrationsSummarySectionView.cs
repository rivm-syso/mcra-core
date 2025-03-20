using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OutdoorAirConcentrationsSummarySectionView : SectionView<OutdoorAirConcentrationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfSubstances = Model.Records.Select(r => r.CompoundName).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentration distributions for {numberOfSubstances} substances.");

                var hiddenProperties = new List<string>();

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OutdoorAirConcentrationsDataTable",
                    ViewBag,
                    caption: "Outdoor air concentrations.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No outdoor air concentration available for the selected substances.");
            }
        }
    }
}
