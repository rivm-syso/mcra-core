﻿using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureThresholdRatioPercentileSectionView : SectionView<ExposureThresholdRatioPercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var hiPercentileRecords = Model.GetHIPercentileRecords();
            bool showUncertainty = hiPercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

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
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
            }
            var riskMetricCalculationType = "(RPF weighted)";
            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                riskMetricCalculationType = "(sum of risk ratios)";
            }
            // Description table
            var descriptionTable = new List<(string, string)>();
            if (Model.Reference != null) {

                descriptionTable.Add(($"Reference substance", $"{Model.Reference.Name} ({Model.Reference.Code})"));
                var uncertaintyMeanOfHazardCharacterisation = isHazardCharacterisationUncertainty
                    ? $"[{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                    : string.Empty;
                var nominalHazardCharacterisationType = Model.IsHazardCharacterisationDistribution
                    ? $"Mean hazard characterisation {riskMetricCalculationType}"
                    : $"Hazard characterisation {riskMetricCalculationType}";
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    descriptionTable.Add(($"{nominalHazardCharacterisationType} ({ViewBag.GetUnit("TargetDoseUnit")})", $"{Model.MeanHazardCharacterisation.ReferenceValue:G3}{uncertaintyMeanOfHazardCharacterisation}"));
                }
            }

            var uncertaintyMeanOfExposure = showUncertainty
                ? $" [{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                : string.Empty;
            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                descriptionTable.Add(($"Mean exposure ({ViewBag.GetUnit("IntakeUnit")})", $"{Model.MeanExposure.ReferenceValue:G3}{uncertaintyMeanOfExposure}"));
            }
            var uncertaintyMeanOfRisk = showUncertainty
                ? $" [{Model.MeanOfExposureThresholdRatio.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanOfExposureThresholdRatio.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                : string.Empty;
            sb.AppendParagraph($"Mean risk (exposure/threshold value):", $"{Model.MeanOfExposureThresholdRatio.ReferenceValue:G3}{uncertaintyMeanOfRisk}");

            // Notification
            if (Model.IsInverseDistribution) {
                sb.AppendNotification("The specified percentiles are calculated using the inverse distribution.");
            }

            // Percentiles table
            sb.AppendTable(
                Model,
                hiPercentileRecords,
                "HIPercentileTable",
                ViewBag,
                caption: "Percentiles risk.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );

            // Percentiles boxplot
            if (showUncertainty) {
                //create chart data section to include in the AppendChart call
                var bootstrapResultsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "HIPercentilesBootstrapTable", Model, Model.GetPercentileBootstrapRecords(false),
                    ViewBag, true, hiddenProperties
                );

                var chartCreator = new ExposureThresholdRatioPercentileChartCreator(Model);
                sb.AppendChart(
                    "HazardIndexPercentileChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true,
                    chartData: bootstrapResultsDataSection
                );
            }
        }
    }
}