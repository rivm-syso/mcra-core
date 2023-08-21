using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardExposureRatioPercentileSectionView : SectionView<HazardExposureRatioPercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var riskPercentileRecords = Model.GetRiskPercentileRecords();
            var showUncertainty = riskPercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

            var isHazardCharacterisationUncertainty = showUncertainty
                && Model.MeanHazardCharacterisation.UncertainValues != null
                && Model.MeanHazardCharacterisation.UncertainValues.Distinct().Count() > 1;

            if (!showUncertainty) {
                hiddenProperties.Add("Median");
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
                hiddenProperties.Add("MedianExposure");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
            } else {
                hiddenProperties.Add("ReferenceValueExposure");
            }
            if (riskPercentileRecords.All(r => r.RisksPercentage == r.ExposurePercentage)) {
                hiddenProperties.Add("RisksPercentage");
            }

            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("ReferenceValueExposure");
                hiddenProperties.Add("ReferenceValueExposure");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
            }

            if (Model.IsHazardCharacterisationDistribution) {
                hiddenProperties.Add("ReferenceValueExposure");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
            }

            // Description table
            var descriptionTable = new List<(string, string)>();
            if (Model.Reference != null) {
                descriptionTable.Add(($"Reference substance", $"{Model.Reference.Name} ({Model.Reference.Code})"));
                var uncertaintyMeanOfHazardCharacterisation = isHazardCharacterisationUncertainty
                    ? $"({Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                    : string.Empty;
                var nominalHazardCharacterisationType = Model.IsHazardCharacterisationDistribution
                    ? $"Mean hazard characterisation"
                    : $"Hazard characterisation";
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    descriptionTable.Add(($"{nominalHazardCharacterisationType} ({ViewBag.GetUnit("TargetDoseUnit")})", $"{Model.MeanHazardCharacterisation.ReferenceValue:G3}{uncertaintyMeanOfHazardCharacterisation}"));
                }
            }

            var uncertaintyMeanOfExposure = showUncertainty
                ? $" ({Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                : string.Empty;
            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                descriptionTable.Add(($"Mean exposure ({ViewBag.GetUnit("IntakeUnit")})", $"{Model.MeanExposure.ReferenceValue:G3}{uncertaintyMeanOfExposure}"));
            }
            var uncertaintyMeanOfRisk = showUncertainty
                ? $" ({Model.MeanRisk.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanRisk.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                : string.Empty;

            var riskMetricString = ViewBag.GetUnit("RiskMetric");
            descriptionTable.Add(($"Mean risk ({riskMetricString})", $"{Model.MeanRisk.ReferenceValue:G3}{uncertaintyMeanOfRisk}"));
            sb.AppendDescriptionTable(descriptionTable);

            // Notification
            if (Model.IsInverseDistribution) {
                sb.AppendNotification("The specified percentiles are calculated using the inverse distribution.");
            }

            // Percentiles table
            sb.AppendTable(
                Model,
                riskPercentileRecords,
                "MOEPercentileTable",
                ViewBag,
                caption: $"Risk distribution percentiles.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );

            // Percentiles boxplot
            if (showUncertainty) {
                var lowerBound = Model.UncertaintyLowerLimit;
                var upperBound = Model.UncertaintyUpperLimit;
                if (upperBound < lowerBound) {
                    var tmp = upperBound;
                    upperBound = lowerBound;
                    lowerBound = tmp;
                    Model.UncertaintyLowerLimit = lowerBound;
                    Model.UncertaintyUpperLimit = upperBound;
                }
                var upperBoxDefault = 75D;
                var lowerBoxDefault = 25D;
                if (upperBound < upperBoxDefault) {
                    upperBoxDefault = upperBound;
                }
                if (lowerBound > lowerBoxDefault) {
                    lowerBoxDefault = lowerBound;
                }
                //create chart data section to include in the AppendChart call
                var bootstrapResultsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "MOEPercentilesBootstrapTable", Model, Model.GetPercentileBootstrapRecords(false),
                    ViewBag, true, hiddenProperties
                );

                var chartCreator = new HazardExposureRatioPercentileChartCreator(Model);
                sb.AppendChart(
                    "MarginOfExposurePercentileChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true,
                    chartData: bootstrapResultsDataSection
                );
                sb.AppendDescriptionParagraph($"The boxplots for uncertainty show the p{lowerBoxDefault} and p{upperBoxDefault} as edges of the box, " +
                    $"and p{lowerBound} and p{upperBound} as edges of the whiskers. The reference value is indicated with the dashed black line, the median " +
                    $"with the solid black line within the box. Outliers are displayed as dots outside the wiskers.");
            }
        }
    }
}
