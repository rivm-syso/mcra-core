using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CPIndividualStatisticsSummarySectionView : SectionView<CPIndividualStatisticsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            {
                sb.AppendDescriptionTable(
                    "ConsumerProductSurveySummaryTable",
                    Model.SectionId,
                    Model.IndividualsSummaryRecord,
                    ViewBag,
                    header: false
                );
            }
            if (Model.SelectedPropertyRecords?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.SelectedPropertyRecords,
                    "ConsumerProductSelectedPropertiesTable",
                    ViewBag,
                    caption: "Selected population properties and levels.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No population properties selected (full population)");
            }

            if (Model.PopulationRecords?.Count > 0) {
                var hiddenProperties = new List<string>();
                if (Model.PopulationRecords.All(r => r.Min == null)) {
                    hiddenProperties.Add("Min");
                }
                if (Model.PopulationRecords.All(r => r.Max == null)) {
                    hiddenProperties.Add("Max");
                }
                sb.AppendTable(
                    Model,
                    Model.PopulationRecords,
                    "ConsumerProductPopulationCharacteristicsDataTable",
                    ViewBag,
                    caption: "Consumer product individuals statistics.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
