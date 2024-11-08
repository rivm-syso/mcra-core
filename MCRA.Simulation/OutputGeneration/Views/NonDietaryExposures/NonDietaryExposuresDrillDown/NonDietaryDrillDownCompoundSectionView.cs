using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryDrillDownCompoundSectionView : SectionView<DrillDownRecordSection<NonDietaryDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var item = Model.DrillDownRecord;
            var referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            var isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";

            var uncorrectedExposure = 0d;
            var record = item.NonDietaryIntakeSummaryPerCompoundRecords.First();

            //Render HTML
            sb.Append("<h4>Compound aggregate</h4>");
            sb.Append("<div class=\"section\">");

            if (item.NonDietaryIntakePerBodyWeight > 0) {
                var chartCreator = new NonDietaryCompoundPieChartCreator(item);
                sb.AppendChart(
                    "NonDietaryCompoundPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

            } else {
                sb.AppendParagraph("No positive exposure");
            }
            sb.Append($"<p>Intake {referenceCompoundName} equivalents = relative potency factor * " +
                      $"(sum of route * absorptionfactor) / {item.BodyWeight} (= body weight)</p>");

            sb.Append("<table class=\"sortable\"><thead>");
            var row = new ArrayList {
                 "Substance name",
                 "Substance code",
                 "Relative potency factor"
            };
            foreach (var r in record.UncorrectedRouteIntakeRecords) {
                row.Add($"Sum {r.Route}({ViewBag.GetUnit("IntakeUnit")})");
                row.Add($"Absorption factor {r.Route}");
            }
            row.Add($"Sum non dietary exposure ({ViewBag.GetUnit("IntakeUnit")})");
            row.Add("Number of non dietary contributions");
            if (isCumulative) {
                row.Add($"Sum non dietary exposure {referenceCompoundName}{equivalents}({ViewBag.GetUnit("IntakeUnit")})");
            }
            sb.AppendHeaderRow(row.ToArray());

            sb.Append("</thead><tbody>");

            foreach (var ipc in item.NonDietaryIntakeSummaryPerCompoundRecords) {
                row = [
                    ipc.SubstanceName,
                    ipc.SubstanceCode,
                    ipc.RelativePotencyFactor.ToString("G3")
                ];

                foreach (var uc in ipc.UncorrectedRouteIntakeRecords) {
                    row.Add((uc.Exposure * item.BodyWeight).ToString("G3"));
                    row.Add(uc.AbsorptionFactor);
                    uncorrectedExposure += uc.Exposure * uc.AbsorptionFactor;
                }
                row.Add((uncorrectedExposure * item.BodyWeight).ToString("G3"));
                row.Add(ipc.NumberOfNondietaryContributions.ToString("N0"));
                if (isCumulative) {
                    row.Add((ipc.NonDietaryIntakeAmountPerBodyWeight * item.BodyWeight).ToString("G3"));
                }
                sb.AppendTableRow(row.ToArray());
            }
            sb.Append("</tbody></table>");
            sb.Append("</div>");
        }
    }
}
