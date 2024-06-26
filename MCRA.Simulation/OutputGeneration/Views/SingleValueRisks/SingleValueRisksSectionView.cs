﻿using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using System.Globalization;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksSectionView : SectionView<SingleValueRisksSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var exposurePercentage = Model.RiskMetric == RiskMetricType.HazardExposureRatio ? 100 - Model.Percentage : Model.Percentage;
            var isAdjustment = Model.IsAdjustment ? " after adjustment" : string.Empty;
            var description = $"Single value risks based on individual risk distribution. ";
            description += Model.IsInversDistribution ? $"The specified percentile is calculated using the inverse distribution." : string.Empty;
            description += $" Risk is expressed as {Model.RiskMetric.GetDisplayName()} at the {exposurePercentage.ToString("F1", CultureInfo.InvariantCulture)}% of exposure{isAdjustment}.";
            sb.AppendDescriptionParagraph(description);
            if (double.IsNaN(Model.Record.MedianRiskValue)) {
                sb.Append("<table><thead>");
                sb.AppendHeaderRow($"Percentage", Model.RiskMetric.GetDisplayName());
                sb.Append("</thead><tbody>");
                sb.AppendTableRow(
                    $"{Model.Percentage.ToString("F1", CultureInfo.InvariantCulture)}%",
                    Model.Record.RiskValue.ToString("G3", CultureInfo.InvariantCulture)
                );
                sb.Append("</tbody></table>");
            } else {
                sb.Append("<table><thead>");
                sb.AppendHeaderRow(
                    $"Percentage",
                    $"Risk ({Model.RiskMetric.GetDisplayName()})",
                    $"p{Model.UncertaintyLowerBound.ToString("F1", CultureInfo.InvariantCulture)}",
                    $"p{Model.UncertaintyUpperBound.ToString("F1", CultureInfo.InvariantCulture)}"
                );
                sb.Append("</thead><tbody>");
                sb.AppendTableRow(
                    $"{Model.Percentage.ToString("F1", CultureInfo.InvariantCulture)}%",
                    Model.Record.MedianRiskValue.ToString("G3", CultureInfo.InvariantCulture),
                    Model.Record.LowerRiskValue.ToString("G3", CultureInfo.InvariantCulture),
                    Model.Record.UpperRiskValue.ToString("G3", CultureInfo.InvariantCulture)
                );
                sb.Append("</tbody></table>");
            }
        }
    }
}
