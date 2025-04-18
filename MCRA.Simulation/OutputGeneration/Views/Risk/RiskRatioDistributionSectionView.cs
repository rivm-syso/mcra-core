﻿using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskRatioDistributionSectionView : SectionView<RiskRatioDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Threshold == 1) {
                sb.AppendParagraph($"Nominal Probability of Critical Exposure (POCE) {Model.ProbabilityOfCriticalEffect:G2} %");
            } else {
                sb.AppendParagraph($"Nominal Probability of Critical Exposure (POCE) relative to threshold {Model.Threshold:G2} = {Model.ProbabilityOfCriticalEffect:G2} %");
            }

            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new RiskRatioChartCreator(Model);
            sb.AppendChart(
                "RiskDistributionChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );

            var chartCreator1 = new RiskRatioCumulativeChartCreator(Model);
            sb.AppendChart(
                "RiskCumulativeChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );
            sb.Append("</div>");
        }
    }
}
