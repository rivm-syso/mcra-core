using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleThresholdExposureRatioSectionView : SectionView<SingleThresholdExposureRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
            var isUncertainty = Model.RiskRecords
                .Any(c => !double.IsNaN(c.PLowerRiskUncLower) && c.PLowerRiskUncLower > 0);

            // Section description
            var substancesString = Model.OnlyCumulativeOutput ? $" for cumulative substance" : string.Empty;
            var effectString = !string.IsNullOrEmpty(Model.EffectName) ? Model.EffectName : "based on multiple effects";
            var descriptionString = $"threshold value/exposure{substancesString} for {effectString}.";

            // Table
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("PLowerRisk_UncLower");
                hiddenProperties.Add("PUpperRisk_UncUpper");
                hiddenProperties.Add("PLowerRiskUncP50");
                hiddenProperties.Add("RiskP50UncP50");
                hiddenProperties.Add("PUpperRiskUncP50");
                hiddenProperties.Add("MedianProbabilityOfCriticalEffect");
                hiddenProperties.Add("LowerProbabilityOfCriticalEffect");
                hiddenProperties.Add("UpperProbabilityOfCriticalEffect");
            } else {
                hiddenProperties.Add("PLowerRiskNom");
                hiddenProperties.Add("RiskP50Nom");
                hiddenProperties.Add("PUpperRiskNom");
                hiddenProperties.Add("ProbabilityOfCriticalEffect");
            }

            var records = (Model.RiskRecords.Any(c => c.RiskP50UncP50 > 0))
                ? Model.RiskRecords.OrderBy(c => c.PLowerRiskUncLower).ToList()
                : Model.RiskRecords.OrderBy(c => c.PLowerRiskNom).ToList();
            sb.AppendTable(
                Model,
                records,
                "MOEBySubstanceTable",
                ViewBag,
                caption: descriptionString,
                saveCsv: true,
                sortable: false,
                rotate: true,
                hiddenProperties: hiddenProperties
            );

            // Figure
            var caption = $"Safety chart: bars show MOE with variability ({pLower} - {pUpper}) in the population.";
            if (isUncertainty) {
                caption = caption
                    + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                    + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                    + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
            }

            sb.AppendChart(
                "MarginOfExposureBySubstance",
                chartCreator: new SingleThresholdExposureRatioHeatMapCreator(Model, isUncertainty, ViewBag.GetUnit("IntakeUnit")),
                ChartFileType.Png,
                Model,
                ViewBag,
                caption: caption,
                true
            );
        }
    }
}
