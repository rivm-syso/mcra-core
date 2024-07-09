using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryDrillDownSectionView : SectionView<NonDietaryDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var referenceCompoundName = Model.ReferenceCompoundName.ToHtml();
            var isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";

            //Render HTML
            sb.Append("<h3>Summary</h3>");
            sb.Append($"<p>Drilldown of {Model.DrillDownSummaryRecords.Count} individual days " +
                      $"around {Model.VariabilityDrilldownPercentage} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                      $"of exposure distribution ({referenceCompoundName}{equivalents})</p>");

            sb.Append("<div class=\"section\">");
            renderSectionView(sb, "NonDietaryDrillDownIndividualsSection", Model);
            sb.Append("</div>");

            sb.Append("<div class=\"section\">");
            for (int i = 0; i < Model.DrillDownSummaryRecords.Count; i++) {
                var item = Model.DrillDownSummaryRecords[i];

                if (item.NonDietaryIntakePerBodyWeight > 0) {
                    sb.Append($"<h3>Drilldown {i + 1}</h3>");
                    sb.Append("<div class=\"section\">");
                    sb.Append($"Individual {item.IndividualCode.ToHtml()}, day {item.Day.ToHtml()}, " +
                              $"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, " +
                              $"sampling weight: {item.SamplingWeight:F2}");

                    var recordSection = new DrillDownRecordSection<NonDietaryDrillDownRecord> {
                        DrillDownRecord = item,
                        ReferenceSubstanceName = Model.ReferenceCompoundName,
                        IsCumulative = Model.IsCumulative
                    };
                    renderSectionView(sb, "NonDietaryDrillDownCompoundSection", recordSection);
                    sb.Append("</div>");
                } else {
                    sb.AppendParagraph($"For individual: {item.IndividualCode}, day {item.Day} no exposure is available");
                }
            }
            sb.Append("</div>");
        }
    }
}
