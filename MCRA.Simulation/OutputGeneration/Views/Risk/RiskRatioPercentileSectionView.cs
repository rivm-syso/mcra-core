using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskRatioPercentileSectionView : SectionView<RiskRatioPercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var riskPercentileRecords = Model.GetRiskPercentileRecords();
            var showUncertainty = riskPercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

            var isHazardCharacterisationUncertainty = showUncertainty
                && Model.MeanHazardCharacterisation?.UncertainValues != null
                && Model.MeanHazardCharacterisation.UncertainValues.Distinct().Count() > 1;

            if (Model.SkippedPercentages?.Count > 0) {
                var skippedPercentilesString = Model.RiskMetricType == RiskMetricType.HazardExposureRatio
                    ? string.Join(", ", Model.SkippedPercentages.Select(r => 100 - r).Order().Select(c => c.ToString("F3")))
                    : string.Join(", ", Model.SkippedPercentages.Select(r => r.ToString("F3")));
                sb.AppendWarning($"In accordance with privacy guidelines the following percentiles were excluded due to an insufficient sample size: {skippedPercentilesString}.");
            }

            var descriptionTable = new List<(string, string)>();
            if (Model.Reference != null) {
                descriptionTable.Add(($"Reference substance", $"{Model.Reference.Name} ({Model.Reference.Code})"));

                var uncertaintyMeanOfHazardCharacterisation = isHazardCharacterisationUncertainty
                    ? $" [{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                    : string.Empty;

                var hcSubgroupDependent = Model.HCSubgroupDependent ? "age dependent" : string.Empty;
                var nominalHazardCharacterisationType = Model.IsHazardCharacterisationDistribution
                    ? $"Mean hazard characterisation {hcSubgroupDependent}"
                    : $"Hazard characterisation {hcSubgroupDependent}";
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    descriptionTable.Add((
                        $"{nominalHazardCharacterisationType} ({Model.TargetUnit.GetShortDisplayName()})",
                        $"{Model.MeanHazardCharacterisation.ReferenceValue:G3}{uncertaintyMeanOfHazardCharacterisation}")
                    );
                }

                var uncertaintyMeanOfExposure = showUncertainty
                    ? $" ({Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                    : string.Empty;
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    descriptionTable.Add((
                        $"Mean exposure ({Model.TargetUnit.GetShortDisplayName()})",
                        $"{Model.MeanExposure.ReferenceValue:G3}{uncertaintyMeanOfExposure}")
                    );
                }
            }

            var uncertaintyMeanOfRisk = showUncertainty
                ? $" ({Model.MeanRisk.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanRisk.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4})"
                : string.Empty;

            descriptionTable.Add((
                $"Mean risk characterisation ratio ({Model.RiskMetricType.GetDisplayName()})",
                $"{Model.MeanRisk.ReferenceValue:G3}{uncertaintyMeanOfRisk}"
            ));
            sb.AppendDescriptionTable(descriptionTable);

            // Notification
            if (Model.IsInverseDistribution) {
                sb.AppendNotification("The specified percentiles are calculated using the inverse distribution.");
            }

            // Create copy of viewbag and fill with (local) target unit
            var viewBag = ViewBag.Clone();
            viewBag.UnitsDictionary.Add("IntakeUnit", Model.TargetUnit?.GetShortDisplayName() ?? string.Empty);

            if (!showUncertainty) {
                hiddenProperties.Add("Median");
                hiddenProperties.Add("LowerBound");
                hiddenProperties.Add("UpperBound");
                hiddenProperties.Add("MedianExposure");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
            } else {
                hiddenProperties.Add("ReferenceValue");
                hiddenProperties.Add("ReferenceValueExposure");
            }
            if (riskPercentileRecords.All(r => r.RisksPercentage == r.ExposurePercentage)) {
                hiddenProperties.Add("RisksPercentage");
            }
            if (Model.Reference == null || Model.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                hiddenProperties.Add("ReferenceValueExposure");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
            }
            if (Model.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                hiddenProperties.Add("ReferenceValueExposure");
            }

            // Percentiles table
            sb.AppendTable(
                Model,
                riskPercentileRecords,
                Model.RiskMetricType == RiskMetricType.HazardExposureRatio ? "MOEPercentileTable" : "HIPercentileTable",
                viewBag,
                caption: $"Risk characterisation ratio distribution percentiles.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );
            var description = string.Empty;
            // Percentiles boxplot
            if (showUncertainty) {
                if (Model.RiskMetricType == RiskMetricType.HazardExposureRatio) {
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
                    description = $"The boxplots for uncertainty show the p{lowerBoxDefault} and p{upperBoxDefault} as edges of the box, " +
                        $"and p{lowerBound} and p{upperBound} as edges of the whiskers. The reference value is indicated with the dashed black line, the median " +
                        $"with the solid black line within the box. Outliers are displayed as dots outside the wiskers.";
                }
                //create chart data section to include in the AppendChart call
                var bootstrapResultsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "MOEPercentilesBootstrapTable", Model, Model.GetPercentileBootstrapRecords(false),
                    ViewBag, true, hiddenProperties
                );

                var chartCreator = new RiskRatioPercentileChartCreator(Model);
                sb.AppendChart(
                    "RiskPercentileChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true,
                    chartData: bootstrapResultsDataSection
                );
                if (Model.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                    sb.AppendDescriptionParagraph(description);
                }
            }
        }
    }
}
