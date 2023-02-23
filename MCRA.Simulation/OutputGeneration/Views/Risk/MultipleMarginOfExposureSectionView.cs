using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MultipleMarginOfExposureSectionView : SectionView<MultipleMarginOfExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
            var isUncertainty = Model.MOERecords
                .Any(c => !double.IsNaN(c.PLowerMOE_UncLower) && c.PLowerMOE_UncLower > 0);

            // Section description
            sb.Append("<p class=\"description\">");

            var numberOfSubstancesZero = Model.MOERecords.Count(c => c.PercentagePositives == 0);
            var substancesString = (Model.NumberOfSubstances - numberOfSubstancesZero) > 1
                ? $" for {Model.NumberOfSubstances - numberOfSubstancesZero} substances"
                : string.Empty;
            if (!string.IsNullOrEmpty(Model.EffectName)) {
                sb.Append($"Margin of exposure{substancesString} for {Model.EffectName}.");
            } else {
                sb.Append($"Margin of exposure{substancesString} based on multiple effects.");
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
            var caption = $"Safety chart: bars show MOE with variability ({pLower} - {pUpper}) in the population.";
            if (isUncertainty) {
                caption = caption
                    + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                    + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                    + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
            }
            sb.AppendChart(
                name: "MarginOfExposureBySubstanceChart",
                chartCreator: new MultipleMarginOfExposureHeatMapCreator(Model, isUncertainty, ViewBag.GetUnit("IntakeUnit")),
                fileType: ChartFileType.Png,
                section: Model,
                viewBag: ViewBag,
                caption: caption,
                true
            );

            // Table
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("PLowerMOE_UncLower");
                hiddenProperties.Add("PUpperMOE_UncUpper");
                hiddenProperties.Add("PLowerMOEUncP50");
                hiddenProperties.Add("MOEP50UncP50");
                hiddenProperties.Add("PUpperMOEUncP50");
                hiddenProperties.Add("MedianProbabilityOfCriticalEffect");
                hiddenProperties.Add("LowerProbabilityOfCriticalEffect");
                hiddenProperties.Add("UpperProbabilityOfCriticalEffect");
            } else {
                hiddenProperties.Add("PLowerMOENom");
                hiddenProperties.Add("MOEP50Nom");
                hiddenProperties.Add("PUpperMOENom");
                hiddenProperties.Add("ProbabilityOfCriticalEffect");
            }
            var tableCaption = "Margins of exposure statistics by substance.";

            var records = (Model.MOERecords.Any(c => c.MOEP50UncP50 > 0))
                ? Model.MOERecords.OrderBy(c => c.PLowerMOE_UncLower).ToList()
                : Model.MOERecords.OrderBy(c => c.PLowerMOENom).ToList();
            sb.AppendTable(
                Model,
                records.Where(c => c.PercentagePositives > 0).ToList(),
                "MOEBySubstanceTable",
                ViewBag,
                caption: tableCaption,
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
