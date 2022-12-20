using System.Linq;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelTimeCourseSectionView : SectionView<KineticModelTimeCourseSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var externalExposuresUnit = ViewBag.UnitsDictionary.ContainsKey("ExternalExposureUnit") ? ViewBag.GetUnit("ExternalExposureUnit") : ViewBag.GetUnit("IntakeUnit");
            var targetAmountUnit = ViewBag.UnitsDictionary.ContainsKey("TargetAmountUnit") ? ViewBag.GetUnit("TargetAmountUnit") : ViewBag.GetUnit("KineticUnit");
            var targetConcentrationUnit = ViewBag.UnitsDictionary.ContainsKey("TargetConcentrationUnit") ? ViewBag.GetUnit("TargetConcentrationUnit") : ViewBag.GetUnit("KineticPerBWUnit");

            //Render HTML
            sb.Append("<table>");
            sb.AppendTableRow("PBPK model:", $"{Model.ModelName} ({Model.ModelCode})");
            sb.AppendTableRow("Description:", Model.ModelDescription);
            sb.AppendTableRow("Substance", $"{Model.SubstanceName} ({Model.SubstanceCode})");
            sb.AppendTableRow("Exposure route(s):", string.Join(", ", Model.ExposureRoutes));
            sb.AppendTableRow("Output:", $"{Model.OutputDescription} ({Model.OutputCode})");
            sb.AppendTableRow("Output unit:", $"{Model.DoseUnit}");
            sb.AppendTableRow("Time unit:", Model.TimeUnit);
            sb.AppendTableRow("Number of doses per day:", Model.NumberOfDosesPerDay);
            sb.AppendTableRow("Number of days skipped:", Model.NumberOfDaysSkipped);
            sb.AppendTableRow("Number of exposure days:", Model.NumberOfDays);
            sb.Append("</table>");

            //no items in InternalTargetSystemExposures collection, return
            if ((Model.InternalTargetSystemExposures?.Count ?? 0) == 0) {
                return;
            }

            sb.Append("<table>");

            //loop over each item using a value tuple to select the item and the item's index
            foreach (var item in Model.InternalTargetSystemExposures) {
                sb.Append("<tr>");
                if (item.MaximumTargetExposure > 0) {
                    sb.Append("<td style=\"vertical-align: top; \">");
                    sb.Append("<table>");
                    if (!string.IsNullOrEmpty(item.Code) && !item.Code.Equals("-1")) {
                        sb.AppendTableRow("Individual id", item.Code);
                    }
                    sb.AppendTableRow($"Body weight ({ViewBag.GetUnit("BodyWeightUnit")})", item.Weight);
                    sb.AppendTableRow("Relative compartment weight", $"{item.RelativeCompartmentWeight:G3}");
                    sb.AppendTableRow($"External daily substance exposure amount ({targetAmountUnit})", $"{item.ExternalExposure:G3}");
                    foreach (var route in Model.ExposureRoutes) {
                        sb.AppendTableRow($" - {route} ({targetAmountUnit})", $"{item.ExposurePerRoute[route]:G3}");
                    }
                    sb.AppendTableRow($"External exposure ({externalExposuresUnit})", $"{item.ExternalExposure / item.Weight:G3}");
                    if (Model.IsAcute) {
                        sb.AppendTableRow($"Peak internal substance amount ({targetAmountUnit})", $"{item.PeakTargetExposure:G3}");
                        sb.AppendTableRow($"Peak internal exposure ({targetConcentrationUnit})", $"{item.InternalPeakTargetConcentration:G3}");
                        sb.AppendTableRow("Ratio peak internal / external", $"{item.PeakAbsorptionFactor:G3}");
                    } else {
                        sb.AppendTableRow($"Long term internal substance amount ({targetAmountUnit})", $"{item.SteadyStateTargetExposure:G3}");
                        sb.AppendTableRow($"Long term internal exposure ({targetConcentrationUnit})", $"{item.InternalLongTermTargetConcentration:G3}");
                        sb.AppendTableRow("Ratio long term internal / external", $"{item.LongTermAbsorptionFactor:G3}");
                    }
                    sb.Append("</table>");

                    sb.Append("</td>");
                    sb.Append("<td>");

                    var chartCreator = new PBPKChartCreator(item, Model, targetConcentrationUnit);
                    sb.AppendChart(
                        "PBPKChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                    sb.Append("</td>");
                } else {
                    sb.Append("<table>");
                    sb.Append("<tr>");
                    sb.Append("<td colspan=\"2\">");
                    sb.Append($"<div class='no_measurements'>No exposure available for individual id = {item.Code}</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
                sb.Append("</tr>");
            }

            //close last row element and table
            sb.Append("</table>");
        }
    }
}
