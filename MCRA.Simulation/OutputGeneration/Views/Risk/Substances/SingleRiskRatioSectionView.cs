using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleRiskRatioSectionView : SectionView<SingleRiskRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                var lower = 100 - Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values between p{lower:#0.##} and p{upper:#0.##} can be reported.");
            } else {
                var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
                var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
                var riskRecord = Model.SingleRiskRecord;
                var isUncertainty = !double.IsNaN(riskRecord.PLowerRiskUncLower);

                // Section description
                var effectString = !string.IsNullOrEmpty(Model.EffectName)
                    ? $" for {Model.EffectName}" : " based on multiple effects";
                var riskMetricString = ViewBag.GetUnit("RiskMetricShort");
                var descriptionString = $"Risk characterisation ratio ({riskMetricString}){effectString}.";

                // Table
                var hiddenProperties = new List<string>();
                if (string.IsNullOrEmpty(riskRecord.ExpressionType)) {
                    hiddenProperties.Add("ExpressionType");
                }
                if (string.IsNullOrEmpty(riskRecord.BiologicalMatrix)) {
                    hiddenProperties.Add("BiologicalMatrix");
                    hiddenProperties.Add("ExpressionType");
                }
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

                var panelBuilder = new HtmlTabPanelBuilder();
                // Figure
                var caption = $"Safety chart: the bar shows the variability ({pLower} - {pUpper}) of the risk characterisation ratio ({Model.RiskMetric.GetDisplayName()}) in the population.";
                if (isUncertainty) {
                    caption = caption
                        + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                        + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                        + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
                }

                {
                    var unstratifiedHiddenProperties = new List<string>(hiddenProperties) {
                        "Stratification"
                    };
                    var panelSb = new StringBuilder();

                    panelSb.AppendTable(
                        Model,
                        [riskRecord],
                        "RiskDistributionTable",
                        ViewBag,
                        caption: descriptionString,
                        saveCsv: true,
                        sortable: false,
                        rotate: true,
                        hiddenProperties: unstratifiedHiddenProperties
                    );

                    var chartCreator = new SingleRiskRatioHeatMapCreator(Model, isUncertainty);
                    panelSb.AppendChart(
                        name: "RiskDistributionSafetyChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: caption
                    );

                    panelBuilder.AddPanel(
                        id: $"PanelRiskDistribution",
                        title: "Risk distribution",
                        hoverText: "Risk distribution",
                        content: new HtmlString(panelSb.ToString())
                    );
                }

                var stratifiedRecords = Model.StratifiedSingleRiskRecords
                    .Where(c => !string.IsNullOrEmpty(c.Stratification)).ToList();
                if (stratifiedRecords.Count != 0) {
                    var chartCreator = new SingleStratifiedRiskRatioHeatMapCreator(
                        Model, null, isUncertainty
                    );

                    var panelSb = new StringBuilder();
                    panelSb.AppendChart(
                        name: $"RiskDistributionStratifiedSafetyChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: caption
                    );

                    panelSb.AppendTable(
                        Model,
                        Model.StratifiedSingleRiskRecords,
                        "RiskDistributionStratifiedTable",
                        ViewBag,
                        caption: descriptionString,
                        saveCsv: true,
                        sortable: true,
                        rotate: false,
                        hiddenProperties: hiddenProperties
                    );

                    panelBuilder.AddPanel(
                        id: $"PanelRiskDistributionStratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: new HtmlString(panelSb.ToString())
                    );
                }
                panelBuilder.RenderPanel(sb, collapseSingleTab: true);
            }
        }
    }
}
