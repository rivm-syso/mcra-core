using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownSectionView : SectionView<DietaryAcuteDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var equivalents = Model.IsCumulative ? " equivalents" : "";

            //Render HTML
            sb.Append("<h3>Summary</h3>");
            sb.Append($"<p>Drilldown of {Model.DrillDownSummaryRecords.Count} individual days " +
                      $"around {Model.VariabilityDrilldownPercentage} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                      $"of exposure distribution ({Model.ReferenceCompoundName.ToHtml()}{equivalents})</p>");
            sb.Append("<div class=\"section\">");

            renderSectionView(sb, "DietaryAcuteDrillDownIndividualsSection", Model);

            sb.Append("</div>");
            sb.Append("<div class=\"section\">");
            for (int i = 0; i < Model.DrillDownSummaryRecords.Count; i++) {
                var item = Model.DrillDownSummaryRecords[i];

                if (item.DietaryIntakePerMassUnit > 0) {
                    sb.Append($"<h3>Drilldown {i + 1}</h3>");
                    sb.Append("<div class=\"section\">");
                    sb.Append($"Individual {item.IndividualCode.ToHtml()}, day {item.Day.ToHtml()}, " +
                              $"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, " +
                              $"sampling weight: {item.SamplingWeight:F2}");

                    var recordSection = new DrillDownRecordSection<DietaryAcuteDrillDownRecord> {
                        DrillDownRecord = item,
                        ReferenceSubstanceName = Model.ReferenceCompoundName,
                        UseUnitVariability = Model.IsUnitVariability,
                        UseProcessing = Model.IsProcessing,
                        IsCumulative = Model.IsCumulative
                    };
                    if (item.DietaryIntakePerMassUnit > 0) {
                        renderSectionView(sb, "DietaryAcuteDrillDownDetailSection", recordSection);
                    }
                    renderSectionView(sb, "DietaryAcuteDrillDownCompoundSection", recordSection);
                    if (item.DietaryIntakePerMassUnit > 0) {
                        renderSectionView(sb, "DietaryAcuteDrillDownFoodAsMeasuredSection", recordSection);
                    }
                    if (item.DietaryIntakePerMassUnit > 0) {
                        renderSectionView(sb, "DietaryAcuteDrillDownFoodAsEatenSection", recordSection);
                    }
                    sb.Append("</div>");
                } else {
                    sb.AppendParagraph($"For individual: {item.IndividualCode}, day {item.Day} no exposure is available");
                }
            }
            sb.Append("</div>");
        }
    }
}
