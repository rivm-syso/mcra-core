using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownDetailSectionView : SectionView<DrillDownRecordSection<DietaryAcuteDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            DietaryAcuteDrillDownRecord item = Model.DrillDownRecord;
            var isUnitVariability = Model.UseUnitVariability;
            var isProcessing = Model.UseProcessing;
            var showRpf = item.AcuteIntakePerFoodRecords.Any(r => r.AcuteIntakePerCompoundRecords.Any(ipc => !double.IsNaN(ipc.Rpf) && ipc.Rpf != 1D));
            var referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            var showZeroConcentrations = false;
            var column = new List<string>();
            var description = new List<string>();
            var processingCalculation = string.Empty;
            var processingCorrectionFactor = string.Empty;
            var total = item.DietaryIntakePerMassUnit;
            if (isProcessing) {
                processingCalculation = "* Processing factor / Processing correction factor";
                processingCorrectionFactor = " .The processing correction factor e.g for a dried food with a consumption of 100 gram which is translated to 300 gram raw agricultural commodity, is 3.";
            }
            if (isUnitVariability) {
                column = [
                    $"Amount ({ViewBag.GetUnit("ConsumptionUnit")})",
                    "Conversion factor",
                    $"Portion amount ({ViewBag.GetUnit("ConsumptionUnit")})",
                    $"Concentration in sample ({ViewBag.GetUnit("ConcentrationUnit")})",
                    "Stochastic vf",
                    $"Concentration in portion ({ViewBag.GetUnit("ConcentrationUnit")})",
                    $"Exposure ({ViewBag.GetUnit("IntakeUnit")})",
                    $"Unit weight ({ViewBag.GetUnit("ConsumptionUnit")})",
                    "Units in composite sample",
                ];
                description = [
                    "Consumed amount of food as eaten",
                    "Translation percentage from food as eaten to modelled food",
                    "Consumed amount of modelled food (= Amount * Conversion factor / 100)",
                    "Monitoring residue (or drawn residue based on specified distribution)",
                    "Stochastic variability factor (= Concentration in portion/Concentration in sample)",
                    "Drawn residue based on specified unit variability distribution and monitoring residue",
                    $"Exposure (= Portion amount * Concentration in portion {processingCalculation} / body weight){processingCorrectionFactor}" ,
                    "Unit weights as specified in database",
                    "Units in composite sample as specified in database",
                ];
            } else {
                column = [
                    $"Amount ({ViewBag.GetUnit("ConsumptionUnit")})",
                    "Conversion factor",
                    $"Portion amount ({ViewBag.GetUnit("ConsumptionUnit")})",
                    $"Concentration in portion ({ViewBag.GetUnit("ConcentrationUnit")})",
                    $"Exposure ({ViewBag.GetUnit("IntakeUnit")})",
                ];
                description = [
                    "Consumed amount of food as eaten",
                    "Translation percentage from food as eaten to modelled food",
                    "Consumed amount of modelled food (= Amount * Conversion factor / 100)",
                    "Monitoring residue (or drawn residue based on specified distribution)",
                    $"Exposure (= Portion amount * Concentration in portion {processingCalculation} / body weight)",
                ];
            }
            if (showRpf) {
                description.Add("Relative potency factor");
                column.Add("Rpf");
                description.Add($"{referenceCompoundName} equivalents exposure (= RPF * exposure)");
                column.Add($"{referenceCompoundName} equivalents exposure ({ViewBag.GetUnit("IntakeUnit")})");
                description.Add("Percentage of total dietary exposure");
                column.Add("Percentage of total");
            }

            //Render HTML
            if (item.OthersIntakePerMassUnit > 0) {
                var riskDriverIntake = item.DietaryIntakePerMassUnit - item.OthersIntakePerMassUnit;
                var othersPercentage = item.OthersIntakePerMassUnit / item.DietaryIntakePerMassUnit * 100;
                sb.Append($"<p>dietary exposure due to selected risk driver components (details below) = " +
                          $"{riskDriverIntake:G3} ({ViewBag.GetUnit("IntakeUnit").ToHtml()}) <br /> remaining dietary exposure " +
                          $"(not further specified) = {item.OthersIntakePerMassUnit:G3} " +
                          $"({ViewBag.GetUnit("IntakeUnit").ToHtml()}) (= {othersPercentage:F1}%)</p>");
            }
            sb.Append("<h4>Per consumed portion per substance</h4>");
            sb.Append("<div class=\"section\">");
            if (isProcessing) {
                sb.AppendParagraph($"Exposure = portion amount * concentration in portion * processing factor / processing correction factor / {item.BodyWeight} (= body weight)");
            } else {
                sb.AppendParagraph($"Exposure = portion amount * concentration in portion  / {item.BodyWeight} (= body weight)");
            }
            if (showRpf) {
                sb.AppendParagraph($"{referenceCompoundName} equivalents exposure = RPF * exposure");
            }
            if (item.OthersIntakePerMassUnit > 0) {
                sb.AppendParagraph("The summary below refers to the exposure due to riskdrivers");
            }
            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));

            sb.Append("<table class=\"sortable\"><thead>");
            //build header row
            var row = new ArrayList {
                $"Food as eaten",
                $"Amount ({ViewBag.GetUnit("ConsumptionUnit")})",
                $"Modelled food",
                $"Conversion factor",
                $"Portion amount ({ViewBag.GetUnit("ConsumptionUnit")})"
            };
            if (isUnitVariability) {
                row.Add($"Unit weight ({ViewBag.GetUnit("ConsumptionUnit")})");
                row.Add($"Units in composite sample");
            }
            row.Add($"Compound");
            if (isUnitVariability) {
                row.Add($"Concentration in sample ({ViewBag.GetUnit("ConcentrationUnit")})");
                row.Add($"Variability factor");
                row.Add($"Stochastic vf");
            }
            row.Add($"Concentration in portion ({ViewBag.GetUnit("ConcentrationUnit")})");
            if (isProcessing) {
                row.Add($"Processing factor");
                row.Add($"Processing correction factor");
            }
            if (isProcessing) {
                row.Add($"Processing type description");
            } else if (isUnitVariability) {
                row.Add($"Batch processing");
            } else if (isProcessing && isUnitVariability) {
                row.Add($"Processing type description");
            }
            row.Add($"Exposure ({ViewBag.GetUnit("IntakeUnit")})");
            if (showRpf) {
                row.Add($"RPF");
                row.Add($"{referenceCompoundName} equivalents exposure ({ViewBag.GetUnit("IntakeUnit")})");
                row.Add($"Percentage of total");
            }
            sb.AppendHeaderRow(row.ToArray());

            sb.Append("</thead><tbody>");


            foreach (var ipf in item.AcuteIntakePerFoodRecords) {
                var intakesPerCompounds = showZeroConcentrations
                    ? ipf.AcuteIntakePerCompoundRecords
                    : ipf.AcuteIntakePerCompoundRecords.Where(i => double.IsNaN(i.Concentration) || i.Concentration > 0);
                foreach (var ipc in intakesPerCompounds) {
                    foreach (var portion in ipc.UnitVariabilityPortions) {
                        row = [
                            ipf.FoodAsEatenName,
                            double.IsNaN(ipf.FoodAsEatenAmount) ? "-" : ipf.FoodAsEatenAmount.ToString("G3"),
                            ipf.FoodAsMeasuredName,
                            double.IsNaN(ipf.Translation) ? "-" : ipf.Translation.ToString("G3"),
                            double.IsNaN(portion.Amount) ? "-" : portion.Amount.ToString("G3")
                        ];
                        if (isUnitVariability) {
                            row.Add(ipc.UnitWeight);
                            row.Add(ipc.UnitsInCompositeSample);
                        }
                        row.Add(ipc.CompoundName);
                        if (isUnitVariability) {
                            row.Add(double.IsNaN(ipc.Concentration) ? "-" : ipc.Concentration.ToString("G3"));
                            row.Add(ipc.UnitVariabilityFactor);
                            var stochVF = portion.Concentration / ipc.Concentration;
                            row.Add(double.IsNaN(stochVF) ? "-" : stochVF.ToString("G3"));
                        }
                        row.Add(double.IsNaN(portion.Concentration) ? "-" : portion.Concentration.ToString("G3"));
                        if (isProcessing) {
                            row.Add(double.IsNaN(ipc.ProcessingFactor) ? "-" : ipc.ProcessingFactor.ToString("G3"));
                            row.Add(double.IsNaN(ipc.ProportionProcessing) ? "-" : ipc.ProportionProcessing.ToString("G3"));
                        }
                        if (isProcessing || isUnitVariability) {
                            row.Add(ipc.ProcessingTypeDescription);
                        }
                        if (double.IsNaN(portion.Amount)) {
                            row.Add(ipc.Intake.ToString("G3"));
                        } else {
                            row.Add((portion.Amount * portion.Concentration * ipc.ProcessingFactor / item.BodyWeight / ipc.ProportionProcessing).ToString("G3"));
                        }
                        if (showRpf && !double.IsNaN(portion.Amount)) {
                            var exposure = ipc.Rpf * portion.Amount * portion.Concentration * ipc.ProcessingFactor / item.BodyWeight / ipc.ProportionProcessing;
                            row.Add(ipc.Rpf.ToString("G3"));
                            row.Add(exposure.ToString("G3"));
                            row.Add((exposure / total).ToString("P2"));
                        } else if (showRpf) {
                            row.Add("-");
                            row.Add(ipc.Intake.ToString("G3"));
                            row.Add((ipc.Intake / total).ToString("P2"));
                        }
                        sb.AppendTableRow(row.ToArray());
                    }
                }
            }
            sb.Append("</tbody></table>");
            sb.Append("</div>");
        }
    }
}
