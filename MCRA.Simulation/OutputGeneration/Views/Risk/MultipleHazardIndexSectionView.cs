using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MultipleHazardIndexSectionView : SectionView<MultipleHazardIndexSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
            var isUncertainty = Model.HazardIndexRecords
                .Any(c => c.HazardIndexPercentiles[0].UncertainValues?.Any() ?? false);

            // Section description
            sb.Append("<p class=\"description\">");

            var numberOfSubstancesZero = Model.HazardIndexRecords.Count(c => c.PercentagePositives == 0);
            var substancesString = (Model.NumberOfSubstances - numberOfSubstancesZero) > 1
                ? $" for {Model.NumberOfSubstances - numberOfSubstancesZero} substances"
                : string.Empty;
            if (!string.IsNullOrEmpty(Model.EffectName)) {
                sb.Append($"Hazard index{substancesString} for {Model.EffectName}.");
            } else {
                sb.Append($"Hazard index{substancesString} based on multiple effects.");
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


            var caption = $"Safety chart: bar shows variability of HI (range {pLower} - {pUpper}) in the population.";
            if (isUncertainty) {
                caption = caption
                    + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                    + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                    + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
            }
            sb.AppendChart(
                name: "HazardIndexBySubstanceChart",
                chartCreator: new MultipleHazardIndexHeatMapCreator(Model, isUncertainty, ViewBag.GetUnit("IntakeUnit")),
                fileType: ChartFileType.Png,
                section: Model,
                viewBag: ViewBag,
                caption: caption,
                saveChartFile: true
            );



            // Table
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("PLowerHI_UncLower");
                hiddenProperties.Add("PUpperHI_UncUpper");
                hiddenProperties.Add("PLowerHIUncP50");
                hiddenProperties.Add("HIP50UncP50");
                hiddenProperties.Add("PUpperHIUncP50");
                hiddenProperties.Add("MedianProbabilityOfCriticalEffect");
                hiddenProperties.Add("LowerProbabilityOfCriticalEffect");
                hiddenProperties.Add("UpperProbabilityOfCriticalEffect");
            } else {
                hiddenProperties.Add("PLowerHINom");
                hiddenProperties.Add("HIP50Nom");
                hiddenProperties.Add("PUpperHINom");
                hiddenProperties.Add("ProbabilityOfCriticalEffect");
            }

            //Render HTML

            var records = (Model.HazardIndexRecords.Any(c => c.HIP50UncP50 > 0))
              ? Model.HazardIndexRecords.OrderByDescending(c => c.PUpperHI_UncUpper).ToList()
              : Model.HazardIndexRecords.OrderByDescending(c => c.PUpperHINom).ToList();
            sb.AppendTable(
                Model,
                records.Where(c => c.PercentagePositives > 0).ToList(),
                "HazardIndexBySubstanceTable",
                ViewBag,
                caption: "Hazard index and percentiles by substance.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );

            sb.Append("<div class=\"figure-container\">");
            caption = $"Cumulative Hazard indices (median) in the population.";
            sb.AppendChart(
               name: "CumulativeHazardIndexBySubstanceMedianChart",
               chartCreator: new CumulativeHazardIndexMedianChartCreator(Model, isUncertainty),
               fileType: ChartFileType.Svg,
               section: Model,
               viewBag: ViewBag,
               caption: caption,
               saveChartFile: true
           );

            caption = $"Cumulative Hazard indices (upper) in the population.";
            sb.AppendChart(
               name: "CumulativeHazardIndexBySubstanceUpperChart",
               chartCreator: new CumulativeHazardIndexUpperChartCreator(Model, isUncertainty),
               fileType: ChartFileType.Svg,
               section: Model,
               viewBag: ViewBag,
               caption: caption,
               saveChartFile: true
           );
            sb.Append("</div>");
        }
    }
}
