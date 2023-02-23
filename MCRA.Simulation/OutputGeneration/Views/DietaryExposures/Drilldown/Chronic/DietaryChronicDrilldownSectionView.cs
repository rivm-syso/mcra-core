using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrilldownSectionView : SectionView<DietaryChronicDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            bool IsProcessing = Model.IsProcessing;
            bool isOIM = Model.IsOIM;
            bool IsCumulative = Model.IsCumulative;
            string label = isOIM ? " the Observed Individual Mean " : " the model assisted ";
            List<string> column, description;
            if (isOIM) {
                column = new List<string>() {
                    $"Observed Individual Mean ({ViewBag.GetUnit("IntakeUnit")})",
                };
                description = new List<string>() {
                    "average of the daily exposures",
                };
            } else {
                column = new List<string>() {
                  "Frequency",
                  "Group mean amount",
                  "Mean transformed exposure per day",
                  "Amount shrinkage factor",
                  $"Model assisted exposure ({ViewBag.GetUnit("IntakeUnit")})",
                  $"Observed Individual mean ({ViewBag.GetUnit("IntakeUnit")})",
                };
                description = new List<string>() {
                  "model based predicted frequency based on BetaBinomial or LogisticNormal model. The predicted frequencies are shrunken. Shrinkage factor is based on the realisation (positive survey days and total number of survey days). The predicted value is on the original scale (0, 1).",
                  "predicted amount based on the Amounts model. The predicted value is on the transformed scale (or Modified BLUP on the transformed scale = (lp - (mean transformed exposure per day - lp) * sqrt(factor) and factor = VarianceBetween/(VarianceBetween + VarianceWithin/nDays) ",
                  "average of the power or logarithmic transformed daily exposures (positive days only)",
                  "sqrt(VarianceBetween/(VarianceBetween + VarianceWithin/nDays)) with nDays = the number of positive survey days",
                  "is based on the model estimated for the frequency and amounts model (= fitted frequency * BiasCorrectedBackTransformed (group mean amount + shrinkage factor * (mean transformed exposures per day - group mean amount))). For individuals without observed exposure, the model assisted exposure is simulated from the model (model-based imputation: ModelAssistedAmount = sqrt(VarianceBetween) * u ~ StandardNormal() + Prediction).",
                  "average of the daily exposures" ,
                };
            }

            //Render HTML
            sb.Append("<h4>Summary</h4>");
            sb.Append("<div class=\"section\">");
            sb.Append($"<p>Drilldown {Model.ChronicDrillDownRecords.Count} individual days around " +
                      $"{Model.PercentageForDrilldown} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                      $"of {label} exposure distribution.</p>");

            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
            renderSectionView(sb, "DietaryChronicDrillDownIndividualsSection", Model);

            sb.AppendTable(
                Model,
                Model.IndividualDrillDownRecords,
                "DietaryIndividualIntakeDrillDownTable",
                ViewBag,
                caption: "Individual drilldown.",
                header: true,
                saveCsv: true
            );
            sb.Append("</div>");

            sb.Append("<div>");
            for (int i = 0; i < Model.ChronicDrillDownRecords.Count; i++) {
                var individualDrillDown = Model.ChronicDrillDownRecords[i];
                sb.Append($"<h4>Drilldown {i + 1}</h4>");
                sb.Append("<div class=\"section\">");
                sb.Append($"<p>Individual {individualDrillDown.IndividualCode.ToHtml()}, body weight: {individualDrillDown.BodyWeight} " +
                          $"{ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, sampling weight: {individualDrillDown.SamplingWeight:F2}</p>");

                sb.AppendParagraph($"observed individual mean: {individualDrillDown.ObservedIndividualMean:G3} {ViewBag.GetUnit("IntakeUnit")}");

                var recordSection = new DrillDownRecordSection<DietaryChronicDrillDownRecord> {
                    DrillDownRecord = individualDrillDown,
                    ReferenceSubstanceName= Model.ReferenceCompoundName,
                    IsCumulative = IsCumulative,
                    UseProcessing = IsProcessing
                };

                if (!isOIM) {
                    sb.AppendParagraph($"model assisted exposure: {individualDrillDown.ModelAssistedIntake:G3} {ViewBag.GetUnit("IntakeUnit")}");
                }
                if (individualDrillDown.DietaryIntakePerMassUnit > 0) {
                    renderSectionView(sb, "DietaryChronicDrillDownDetailSection", recordSection);
                }
                renderSectionView(sb, "DietaryChronicDrillDownCompoundSection", recordSection);
                renderSectionView(sb, "DietaryChronicDrillDownFoodAsMeasuredSection", recordSection);
                renderSectionView(sb, "DietaryChronicDrillDownFoodAsEatenSection", recordSection);
                sb.Append("</div>");
            }
            sb.Append("</div>");
        }
    }
}
