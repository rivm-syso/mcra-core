using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardIndexPercentileSectionView : SectionView<HazardIndexPercentileSection> {
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
                    ? $"[{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                        + $"{Model.MeanHazardCharacterisation.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                    : string.Empty;
                var nominalHazardCharacterisationType = Model.IsHazardCharacterisationDistribution
                    ? "Mean hazard characterisation"
                    : "Hazard characterisation";
                descriptionTable.Add(($"{nominalHazardCharacterisationType} ({ViewBag.GetUnit("TargetDoseUnit")})", $"{Model.MeanHazardCharacterisation.ReferenceValue:G3}{uncertaintyMeanOfHazardCharacterisation}"));
            }

            var uncertaintyMeanOfExposure = showUncertainty
                ? $" [{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanExposure.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                : string.Empty;
            descriptionTable.Add(($"Mean exposure ({ViewBag.GetUnit("IntakeUnit")})", $"{Model.MeanExposure.ReferenceValue:G3}{uncertaintyMeanOfExposure}"));

            var uncertaintyMeanOfHazardIndex = showUncertainty
                ? $" [{Model.MeanOfHazardIndex.UncertainValues.Percentile(Model.UncertaintyLowerLimit):G4}, "
                    + $"{Model.MeanOfHazardIndex.UncertainValues.Percentile(Model.UncertaintyUpperLimit):G4}]"
                : string.Empty;
            descriptionTable.Add(($"Mean hazard index", $"{Model.MeanOfHazardIndex.ReferenceValue:G3}{uncertaintyMeanOfHazardIndex}"));
            sb.AppendDescriptionTable(descriptionTable);

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
                caption: "Percentiles hazard index.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );

            // Percentiles boxplot
            if (showUncertainty) {
                var chartCreator = new HazardIndexPercentileChartCreator(Model);
                sb.AppendChart(
                    "HazardIndexPercentileChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true
                );
            }
        }
    }
}
