using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskSummarySectionView : SectionView<RiskSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hazardCharacterisationNature = Model.IsHazardCharacterisationDistribution ? "probabilistic" : "single value";
            var exposureSection = Model.ExposureModel == ActionType.DietaryExposures
                ? SectionReference.FromHeader(Toc.GetSubSectionHeader<DietaryExposuresSummarySection>(), "exposure")
                : SectionReference.FromHeader(Toc.GetSubSectionHeader<TargetExposuresSummarySection>(), "exposure");

            var hazCharSection = SectionReference.FromHeader(Toc.GetSubSectionHeader<HazardCharacterisationsSummarySection>(), $"{hazardCharacterisationNature} hazard characterisation");
            var descriptions = new List<string>();

            var riskMetricCalculationType = string.Empty;
            if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                riskMetricCalculationType = "(RPF weighted)";
            } else if (Model.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                riskMetricCalculationType = "(sum of risk ratios)";
            }
            var description = $"Risk computed from {{0}} and {{1}} {riskMetricCalculationType} as {Model.RiskMetricType.GetDisplayName()}.";
            descriptions.AddDescriptionItem(description, exposureSection, hazCharSection);

            description = $"{{0}} and {{1}} were calculated for {Model.ExposureType.GetDisplayName().ToLower()} {Model.TargetDoseLevel.GetDisplayName().ToLower()} doses.";
            exposureSection.Title = "Exposures";
            hazCharSection.Title = "hazard characterisations";
            descriptions.AddDescriptionItem(description, exposureSection, hazCharSection);

            sb.AppendDescriptionList(descriptions);
            if (Model.NumberOfMissingSubstances > 0) {
                var message = Model.ExposureType == ExposureType.Acute
                    ? $"Note, exposure estimates indicate zero exposure for all individual days to {Model.NumberOfMissingSubstances} of the {Model.NumberOfSubstances} substances."
                    : $"Note, exposure estimates indicate zero exposure for all individuals to {Model.NumberOfMissingSubstances} of the {Model.NumberOfSubstances} substances.";
                sb.AppendParagraph(message);
            }
            if (Model.NumberOfSubstances > 0 && Model.NumberOfMissingSubstances == 0) {
                sb.AppendParagraph($"For {Model.NumberOfSubstances} substances exposure data is available.");
            }
        }
    }
}
