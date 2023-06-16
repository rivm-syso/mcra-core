using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ThresholdExposureRatioPercentileSectionView : SectionView<ThresholdExposureRatioPercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var riskPercentileRecords = Model.GetRiskPercentileRecords();
            var showUncertainty = riskPercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

            var isHazardCharacterisationUncertainty = showUncertainty
                && Model.MeanHazardCharacterisation.UncertainValues != null
                && Model.MeanHazardCharacterisation.UncertainValues.Distinct().Count() > 1;

            if (!showUncertainty) {
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
                hiddenProperties.Add("Median");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
            } else {
                hiddenProperties.Add("ReferenceValueExposure");
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
                hiddenProperties.Add("ExposurePercentage");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
            }
            var riskMetricCalculationType = "(RPF weighted)";
            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                riskMetricCalculationType = "(sum of risk ratios)";
            }
            if (Model.Reference != null) {
                sb.AppendParagraph($"Reference: {Model.Reference.Name}.");
                var uncertaintyMeanOfHazardCharacterisation = isHazardCharacterisationUncertainty
                    ? $"({Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                    : string.Empty;
                var nominalHazardCharacterisationType = Model.IsHazardCharacterisationDistribution
                    ? $"Mean hazard characterisation {riskMetricCalculationType}"
                    : $"Hazard characterisation {riskMetricCalculationType}";
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    sb.AppendParagraph($"{nominalHazardCharacterisationType}: {Model.MeanHazardCharacterisation.ReferenceValue:G3}{uncertaintyMeanOfHazardCharacterisation} ({ViewBag.GetUnit("TargetDoseUnit")}).");
                }
            }

            var uncertaintyMeanOfExposure = showUncertainty
                ? $" ({Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                : string.Empty;
            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                sb.AppendParagraph($"Mean exposure: {Model.MeanExposure.ReferenceValue:G3}{uncertaintyMeanOfExposure} ({ViewBag.GetUnit("IntakeUnit")}).");
            }
            var uncertaintyMeanOfRisk = showUncertainty
                ? $" ({Model.MeanOfThresholdExposureRatio.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanOfThresholdExposureRatio.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                : string.Empty;
            sb.AppendParagraph($"Mean risk (threshold value/exposure): {Model.MeanOfThresholdExposureRatio.ReferenceValue:G3}{uncertaintyMeanOfRisk}.");

            if (Model.IsInverseDistribution) {
                sb.AppendDescriptionParagraph("The specified percentiles are calculated using the inverse distribution.");
            }

            sb.AppendTable(
                Model,
                riskPercentileRecords,
                "MOEPercentileTable",
                ViewBag,
                caption: "Percentiles risk.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );

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

                var chartCreator = new ThresholdExposureRatioPercentileChartCreator(Model);
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
