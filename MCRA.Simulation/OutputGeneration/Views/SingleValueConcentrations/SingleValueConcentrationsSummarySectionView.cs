using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueConcentrationsSummarySectionView : SectionView<SingleValueConcentrationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records?.Count > 0) {
                var hiddenProperties = new List<string>();
                var foodsCount = Model.Records.Select(r => r.FoodCode).Distinct().Count();
                var substancesCount = Model.Records.Select(r => r.SubstanceCode).Distinct().Count();

                var isActiveConversion = Model.Records.Any(r => !string.IsNullOrEmpty(r.MeasuredSubstanceCode) && !double.IsNaN(r.ConversionFactor));
                if (!isActiveConversion) {
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.MeasuredSubstanceCode));
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.MeasuredSubstanceName));
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.ConversionFactor));
                }
                if (Model.Records.All(r => double.IsNaN(r.MeanConcentration))) {
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.MeanConcentration));
                }
                if (Model.Records.All(r => double.IsNaN(r.MedianConcentration))) {
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.MedianConcentration));
                }
                if (Model.Records.All(r => double.IsNaN(r.HighestConcentration))) {
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.HighestConcentration));
                }
                if (Model.Records.All(r => double.IsNaN(r.Loq))) {
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.Loq));
                }
                if (Model.Records.All(r => double.IsNaN(r.Mrl))) {
                    hiddenProperties.Add(nameof(SingleValueConcentrationSummaryRecord.Mrl));
                }
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} concentration single values for {foodsCount} foods and {substancesCount} substances.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    isActiveConversion ? "SingleValueConcentrationsTable" : "ActiveSubstanceSingleValueConcentrationsTable",
                    ViewBag,
                    header: true,
                    caption: isActiveConversion
                        ? "Single value concentrations converted to active substance."
                        : "Single value concentrations.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph($"No concentration single values.");
            }
        }
    }
}
