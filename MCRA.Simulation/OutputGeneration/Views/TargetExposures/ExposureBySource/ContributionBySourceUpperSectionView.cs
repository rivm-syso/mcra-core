﻿using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionBySourceUpperSectionView : SectionView<ContributionBySourceUpperSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NumberOfIntakes} records), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

            if (Model.Records.Count > 0) {
                var chartCreator = new ContributionBySourceUpperPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "UpperDistributionSourceChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ExposureBySourceUpperTable",
                    ViewBag,
                    caption: $"Contributions by source for the upper distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No upper distribution available for specified percentage");
            }
        }
    }
}
