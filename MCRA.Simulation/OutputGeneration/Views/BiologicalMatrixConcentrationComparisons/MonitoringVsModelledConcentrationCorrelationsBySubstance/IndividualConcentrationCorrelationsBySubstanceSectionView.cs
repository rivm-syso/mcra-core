﻿using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualConcentrationCorrelationsBySubstanceSectionView : SectionView<IndividualConcentrationCorrelationsBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            int take = 3;
            int loopCount = (int)Math.Ceiling(1.0 * Model.Records.Count / take);

            if (Model.Records.Any(r => r.MonitoringVersusModelExposureRecords.Any())) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "MonitoringVersusModelIndividualConcentrationsBySubstanceTable",
                   ViewBag,
                   caption: "Monitoring versus modelled individual exposures by substance.",
                   saveCsv: true,
                   header: true
                );
                sb.Append("<table><tbody>");
                for (int i = 0; i < loopCount; i++) {
                    sb.Append("<tr>");
                    foreach (var record in Model.Records.Skip(i * take).Take(take)) {
                        sb.Append("<td>");
                        if (record.MonitoringVersusModelExposureRecords.Any(r => r.BothPositive)) {
                            var chartCreator = new IndividualConcentrationCorrelationChartCreator(
                                    Model,
                                    record.SubstanceCode,
                                    ViewBag.GetUnit("ModelledExposureUnit"),
                                    Model.ExposureTarget,
                                    Model.LowerPercentage,
                                    Model.UpperPercentage,
                                    375, 300
                            );
                            sb.AppendChart(
                                "MonitoringVersusModelIndividualConcentrationsBySubstanceChart",
                                chartCreator,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                chartCreator.Title,
                                saveChartFile: true
                            );
                        } else {
                            sb.Append("<div class='no_measurements'>No matches between monitoring and modelled exposures</div>");
                        }
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
            } else {
                sb.AppendNotification("No matches at individual-level between individuals from monitoring and individuals from modelled exposures.");
            }
        }
    }
}
