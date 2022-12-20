using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownIndividualsSectionView : SectionView<DietaryChronicDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool cofactor = Model.ChronicDrillDownRecords.First().Cofactor != string.Empty;
            bool covariable = Model.ChronicDrillDownRecords.First().Covariable != string.Empty;
            bool isOIM = Model.IsOIM;

            var row = new ArrayList();
            row.Add("Individual ID");
            row.Add($"Body weight ({ViewBag.GetUnit("BodyWeightUnit")})");
            if (cofactor) {
                row.Add(Model.CofactorName);
            }
            if (covariable) {
                row.Add(Model.CovariableName);
            }
            row.Add("Sampling weight");
            row.Add($"Sum total exposure ({ViewBag.GetUnit("IntakeUnit")})");
            row.Add("Number of survey days");
            row.Add("Number of positive survey days");
            if (!isOIM) {
                row.Add("Frequency");
                row.Add("Group mean amount");
                row.Add("Mean transformed exposure per day");
                row.Add("Amount shrinkage factor");
                row.Add($"Model assisted exposure ({ViewBag.GetUnit("IntakeUnit")})");
            }
            row.Add($"Observed Individual Mean ({ViewBag.GetUnit("IntakeUnit")})");

            sb.Append("<table class='sortable'><thead>");
            sb.AppendHeaderRow(row.ToArray());
            sb.Append("</thead><tbody>");

            foreach (var r in Model.ChronicDrillDownRecords) {
                row = new ArrayList();
                row.Add(r.IndividualCode);
                row.Add(r.BodyWeight);
                if (cofactor) {
                    row.Add(r.Cofactor);
                }
                if (covariable) {
                    row.Add(r.Covariable);
                }
                row.Add(r.SamplingWeight.ToString("F2"));
                row.Add(r.DietaryIntakePerMassUnit.ToString("G3"));
                row.Add(r.DayDrillDownRecords.Count().ToString("N0"));
                row.Add(r.PositiveSurveyDays.ToString("N0"));
                if (!isOIM) {
                    row.Add(r.ModelAssistedFrequency.ToString("G3"));
                    row.Add(r.AmountPrediction.ToString("G3"));
                    row.Add(double.IsNaN(r.TransformedOIM) ? "-" : r.TransformedOIM.ToString("G3"));
                    row.Add(r.ShrinkageFactor.ToString("G3"));
                    row.Add(r.ModelAssistedIntake.ToString("G3"));
                }
                row.Add(r.ObservedIndividualMean.ToString("G3"));

                sb.AppendTableRow(row.ToArray());
            }
            sb.Append("</tbody></table>");
        }
    }
}
