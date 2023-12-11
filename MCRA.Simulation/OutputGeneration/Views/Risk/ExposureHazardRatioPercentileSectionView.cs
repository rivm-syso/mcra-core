using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureHazardRatioPercentileSectionView : SectionView<ExposureHazardRatioPercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var riskPercentileRecords = Model.GetRiskPercentileRecords();
            var showUncertainty = riskPercentileRecords.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);

            var isHazardCharacterisationUncertainty = showUncertainty
                && Model.MeanHazardCharacterisation?.UncertainValues != null
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
            if (Model.Reference == null || Model.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
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
                    ? $" [{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                    : string.Empty;
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    descriptionTable.Add((
                        $"Mean exposure ({Model.TargetUnit.GetShortDisplayName()})",
                        $"{Model.MeanExposure.ReferenceValue:G3}{uncertaintyMeanOfExposure}")
                    );
                }
            }

            var uncertaintyMeanOfRisk = showUncertainty
                ? $" [{Model.MeanRisk.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanRisk.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                : string.Empty;

            descriptionTable.Add((
                $"Mean risk characterisation ratio ({RiskMetricType.ExposureHazardRatio.GetDisplayName()})",
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

            // Percentiles table
            sb.AppendTable(
                Model,
                riskPercentileRecords,
                "HIPercentileTable",
                viewBag,
                caption: $"Risk characterisation ratio distribution percentiles.",
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

                var chartCreator = new ExposureHazardRatioPercentileChartCreator(Model);
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
