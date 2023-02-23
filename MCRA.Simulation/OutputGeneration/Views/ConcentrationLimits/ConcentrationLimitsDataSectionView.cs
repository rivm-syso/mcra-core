using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationLimitsDataSectionView : SectionView<ConcentrationLimitsDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any(c => c.MaximumConcentrationLimit != null)) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
                var numberOfSubstances = Model.Records.Select(r => r.CompoundCode).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentration limits for {numberOfFoods} foods and {numberOfSubstances} substances.");

                // Table
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.StartDate))) {
                    hiddenProperties.Add("StartDate");
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.EndDate))) {
                    hiddenProperties.Add("EndDate");
                }
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConcentrationLimitsDataTable",
                    ViewBag,
                    caption: "Concentration limits.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No concentration limits available for the selected foods and/or substances.");
            }
        }
    }
}
