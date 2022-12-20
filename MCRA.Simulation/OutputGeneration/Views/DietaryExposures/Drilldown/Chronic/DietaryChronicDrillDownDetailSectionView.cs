using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownDetailSectionView : SectionView<DrillDownRecordSection<DietaryChronicDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            DietaryChronicDrillDownRecord item = Model.DrillDownRecord;
            var isProcessing = Model.UseProcessing;
            var showRpf = Model.DrillDownRecord.DayDrillDownRecords.Any(r => r.ChronicIntakePerFoodRecords
                .Any(ipf => ipf.ChronicIntakePerCompoundRecords
                    .Any(ipc => !double.IsNaN(ipc.Rpf) && ipc.Rpf != 1D)));
            var processingCalculation = " ";
            var column = new List<string>();
            var description = new List<string>();
            var referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            if (isProcessing) {
                processingCalculation = "* Processing factor / Processing correction factor";
            }
            column = new List<string>() {
                "Day",
                "Amount (" + ViewBag.GetUnit("ConsumptionUnit") + ")",
                "Conversion factor",
                "Portion amount (" + ViewBag.GetUnit("ConsumptionUnit") + ")",
                "Concentration (" + ViewBag.GetUnit("ConcentrationUnit") + ")",
                "Exposure (" + ViewBag.GetUnit("IntakeUnit") + ")",
            };
            description = new List<string>() {
                "day in survey",
                "consumed amount of food as eaten",
                "translation percentage from food as eaten to modelled food",
                "consumed amount of modelled food (= Amount * Conversion factor / 100)",
                "monitoring residue (or drawn residue based on specified distribution)",
                $"exposure (= Portion amount * Concentration in portion {processingCalculation} / body weight)",
            };
            if (showRpf) {
                description.Add("Relative potency factor");
                column.Add("Rpf");
                description.Add($"{referenceCompoundName} equivalents exposure (= RPF * exposure)");
                column.Add($"{referenceCompoundName} equivalents exposure ({ViewBag.GetUnit("IntakeUnit")})");
                description.Add("Percentage of average of the daily exposures (OIM)");
                column.Add("Percentage of OIM");
            }

            //Render HTML
            sb.Append("<h4>Per consumed portion per substance</h4>");
            sb.Append("<div class=\"section\">");
            sb.AppendParagraph($"Exposure per day = portion amount * concentration {processingCalculation.ToLower()} / {item.BodyWeight} (= body weight)");
            if (showRpf) {
                sb.AppendParagraph($"{referenceCompoundName} equivalents exposure = RPF * exposure");
            }
            if (item.OthersDietaryIntakePerMassUnit > 0) {
                sb.AppendParagraph("The summary below refers to the exposure due to riskdrivers");
            }
            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
            var row = new ArrayList();
            row.Add("Day");
            row.Add("Food as eaten");
            row.Add($"Amount ({ViewBag.GetUnit("ConsumptionUnit")})");
            row.Add("Modelled food");
            row.Add("Conversion factor");
            row.Add($"Portion amount ({ViewBag.GetUnit("ConsumptionUnit")})");
            row.Add("Compound");
            row.Add($"Concentration ({ViewBag.GetUnit("ConcentrationUnit")})");
            if (isProcessing) {
                row.Add("Processing factor");
                row.Add("Processing correction factor");
            }
            row.Add($"Exposure ({ViewBag.GetUnit("IntakeUnit")})");
            if (showRpf) {
                row.Add("RPF");
                row.Add($"{referenceCompoundName} equivalents exposure ({ViewBag.GetUnit("IntakeUnit")})");
                row.Add("Percentage of OIM");
            }
            sb.Append("<table class='sortable'><thead>");
            sb.AppendHeaderRow(row.ToArray());
            sb.Append("</thead><tbody>");
            var counter = 0;
            foreach (var dayDrillDown in item.DayDrillDownRecords) {
                var numberOfDays = item.DayDrillDownRecords.Count;
                var detailedIntakePerFoodRecord = dayDrillDown.ChronicIntakePerFoodRecords;
                counter = 0;
                foreach (var ipf in detailedIntakePerFoodRecord) {
                    foreach (var ipc in ipf.ChronicIntakePerCompoundRecords) {
                        if (counter > Model.DisplayNumber) {
                            break;
                        }
                        if (ipc.Concentration > 0 || double.IsNaN(ipc.Concentration)) {
                            row = new ArrayList();
                            row.Add(dayDrillDown.Day);
                            row.Add(ipf.FoodAsEatenName);
                            row.Add(double.IsNaN(ipf.FoodAsEatenAmount) ? "-" : ipf.FoodAsEatenAmount.ToString("G3"));
                            row.Add(ipf.FoodAsMeasuredName);
                            row.Add(double.IsNaN(ipf.Translation) ? "-" : ipf.Translation.ToString("P2"));
                            row.Add(double.IsNaN(ipf.FoodAsMeasuredAmount) ? "-" : ipf.FoodAsMeasuredAmount.ToString("G3"));
                            row.Add(ipc.CompoundName);
                            row.Add(double.IsNaN(ipc.Concentration) ? "-" : ipc.Concentration.ToString("G3"));
                            if (isProcessing) {
                                row.Add(double.IsNaN(ipc.ProcessingFactor) ? "-" : ipc.ProcessingFactor.ToString("G3"));
                                row.Add(double.IsNaN(ipc.ProportionProcessing) ? "-" : ipc.ProportionProcessing.ToString("G3"));
                            }
                            if (double.IsNaN(ipf.FoodAsMeasuredAmount)) {
                                row.Add(ipc.Intake.ToString("G4"));
                            } else {
                                row.Add((ipf.FoodAsMeasuredAmount * ipc.Concentration * ipc.ProcessingFactor / item.BodyWeight / ipc.ProportionProcessing).ToString("G4"));
                            }
                            if (showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)) {
                                var exposure = ipc.Rpf * ipf.FoodAsMeasuredAmount * ipc.Concentration * ipc.ProcessingFactor / item.BodyWeight / ipc.ProportionProcessing;
                                row.Add(ipc.Rpf.ToString("G3"));
                                row.Add(exposure.ToString("G4"));
                                row.Add((exposure / Model.DrillDownRecord.ObservedIndividualMean / numberOfDays).ToString("P2"));
                            } else if (showRpf) {
                                row.Add("-");
                                row.Add(ipc.Intake.ToString("G3"));
                                row.Add((ipc.Intake / Model.DrillDownRecord.ObservedIndividualMean / numberOfDays).ToString("P2"));
                            }
                            sb.AppendTableRow(row.ToArray());
                            counter++;
                        }

                    }
                }
            }
            sb.Append("</tbody></table>");
            if (counter > Model.DisplayNumber) {
                sb.Append($"<p>Only the first {Model.DisplayNumber} records are displayed</p>");
            }
            sb.Append("</div>");
        }
    }
}
