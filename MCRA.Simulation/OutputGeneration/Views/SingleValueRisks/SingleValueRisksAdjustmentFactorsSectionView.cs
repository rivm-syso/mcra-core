using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksAdjustmentFactorsSectionView : SectionView<SingleValueRisksAdjustmentFactorsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            var hiddenPropertiesAF = new List<string>();
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedMarginOfExposure");
            }

            var description = $"Single value risks based on individual risk distribution. ";
            description += Model.IsInversDistribution ? $"The specified percentile is calculated using the inverse distribution." : string.Empty;
            sb.AppendDescriptionParagraph(description);

            //Render further HTML only if adjustment factor is used
            if (!Model.UseAdjustmentFactor) {
                return;
            }

            sb.Append("<div class=\"figure-container\">");
            if (Model.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None
                && Model.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed
            ) {
                var chartCreatorExposure = new AFDensityChartCreator(Model, true);
                sb.AppendChart(
                    "AdjustmentFactorsDensityExposureChart",
                    chartCreatorExposure,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreatorExposure.Title,
                    true
                );
            }
            if (Model.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None
                && Model.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed
            ) {
                var chartCreatorHazard = new AFDensityChartCreator(Model, false);
                sb.AppendChart(
                    "AdjustmentFactorsDensityHazardChart",
                    chartCreatorHazard,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreatorHazard.Title,
                    true
                );
            }
            sb.Append("</div>");

            if (Model.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None
                || Model.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None
            ) {
                var caption = $"Adjustment factors: AFoverall = foreground contribution/100 + background contribution/100 * AFexposure * AFhazard.";
                if (Model.AdjustmentFactorRecords.All(c => c.AdjustmentFactorExposure == 1)) {
                    hiddenPropertiesAF.Add("AdjustmentFactorExposure");
                    hiddenPropertiesAF.Add("AdjustmentFactorExposureHazard");
                    caption = $"Adjustment factors: AFoverall = foreground contribution/100 + background contribution/100 * AFhazard.";
                }
                if (Model.AdjustmentFactorRecords.All(c => c.AdjustmentFactorHazard == 1)) {
                    hiddenPropertiesAF.Add("AdjustmentFactorHazard");
                    hiddenPropertiesAF.Add("AdjustmentFactorExposureHazard");
                    caption = $"Adjustment factors: AFoverall = foreground contribution/100 + background contribution/100 * AFexposure.";
                }
                if (Model.UseAdjustmentFactorBackground && Model.AdjustmentFactorRecords.All(c => c.BackgroundContribution == 1)) {
                    hiddenPropertiesAF.Add("BackgroundContributionPercentage");
                    caption = "Adjustment factors: AFoverall = AFexposure * AFhazard  (no background contribution estimated).";
                }
                if (!Model.UseAdjustmentFactorBackground) {
                    hiddenPropertiesAF.Add("BackgroundContributionPercentage");
                    caption = "Adjustment factors: AFoverall = AFexposure * AFhazard.";
                }
                sb.AppendDescriptionParagraph("Adjustment factors for selected percentile");
                sb.AppendDescriptionParagraph(caption);
                sb.AppendTable(
                    Model,
                    Model.AdjustmentFactorRecords,
                    "SingleValueRisksAdjustmentFactorTable",
                    ViewBag,
                    header: true,
                    saveCsv: true,
                    rotate: true,
                    hiddenProperties: hiddenPropertiesAF
                );
            }
        }
    }
}
