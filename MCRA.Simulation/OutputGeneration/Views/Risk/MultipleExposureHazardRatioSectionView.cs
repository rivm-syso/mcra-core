﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MultipleExposureHazardRatioSectionView : SectionView<MultipleExposureHazardRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
            var isUncertainty = Model.RiskRecords
                .Any(c => c.RiskPercentiles[0].UncertainValues?.Any() ?? false);

            // Section description
            sb.Append("<p class=\"description\">");

            var numberOfSubstancesZero = Model.RiskRecords.Count(c => c.PercentagePositives == 0);
            var substancesString = (Model.NumberOfSubstances - numberOfSubstancesZero) > 1
                ? $" for {Model.NumberOfSubstances - numberOfSubstancesZero} substances"
                : string.Empty;
            if (!string.IsNullOrEmpty(Model.EffectName)) {
                sb.Append($"Risk {substancesString} for {Model.EffectName}.");
            } else {
                sb.Append($"Risk {substancesString} based on multiple effects.");
            }

            if (numberOfSubstancesZero > 0) {
                sb.Append($" For {numberOfSubstancesZero} substances no positive exposure is found.");
                var activeSubstancesSectionReference = Toc?.GetSubSectionHeader<ActiveSubstancesSummarySection>();
                if (activeSubstancesSectionReference != null) {
                    var activeSubstanceSection = SectionReference.FromHeader(activeSubstancesSectionReference, ActionType.ActiveSubstances.GetDisplayName(true));
                    sb.Append(SectionHelper.FormatWithSectionLinks($" See the {{0}} section for an overview of all active substances.", activeSubstanceSection));
                }
            }

            sb.Append("</p>");

            // Figure
            var caption = $"Safety chart: bars show risks with variability ({pLower} - {pUpper}) in the population.";
            if (isUncertainty) {
                caption = caption
                    + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                    + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                    + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
            }
            sb.AppendChart(
                name: "HazardIndexBySubstanceChart",
                chartCreator: new MultipleExposureHazardRatioHeatMapCreator(Model, isUncertainty, ViewBag.GetUnit("IntakeUnit")),
                fileType: ChartFileType.Png,
                section: Model,
                viewBag: ViewBag,
                caption: caption,
                saveChartFile: true
            );

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
                records.Where(c => c.PercentagePositives > 0).ToList(),
                "HazardIndexBySubstanceTable",
                ViewBag,
                caption: $"Risk statistics by substance.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}