using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleExposureHazardRatioSectionView : SectionView<SingleExposureHazardRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
            var isUncertainty = Model.RiskRecords
                .Any(c => c.RiskPercentiles[0].UncertainValues?.Any() ?? false);

            // Section description
            var substancesString = Model.OnlyCumulativeOutput 
                ? $" for cumulative substance" : string.Empty;
            var effectString = !string.IsNullOrEmpty(Model.EffectName) 
                ? $" for {Model.EffectName}" : " based on multiple effects";
            var riskMetricString = ViewBag.GetUnit("RiskMetricShort");
            var descriptionString = $"Risk ({riskMetricString}){substancesString}{effectString}.";

            // Table
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("PLowerRiskUncLower");
                hiddenProperties.Add("PUpperRiskUncUpper");
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
             ? Model.RiskRecords.OrderByDescending(c => c.PUpperRiskUncUpper).ToList()
             : Model.RiskRecords.OrderByDescending(c => c.PUpperRiskNom).ToList();
            sb.AppendTable(
               Model,
               records,
               "SingleHazardIndexTable",
               ViewBag,
               caption: descriptionString,
               saveCsv: true,
               sortable: false,
               rotate: true,
               hiddenProperties: hiddenProperties
           );

            // Figure
            var caption = $"Safety chart: bar shows variability of risk (range {pLower} - {pUpper}) in the population.";
            if (isUncertainty) {
                caption = caption
                    + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                    + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                    + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
            }
            sb.AppendChart(
                name: "HazardIndexBySubstanceChart",
                chartCreator: new SingleExposureHazardRatioHeatMapCreator(Model, isUncertainty, ViewBag.GetUnit("IntakeUnit")),
                fileType: ChartFileType.Png,
                section: Model,
                viewBag: ViewBag,
                caption: caption,
                saveChartFile: true
            );
        }
    }
}
