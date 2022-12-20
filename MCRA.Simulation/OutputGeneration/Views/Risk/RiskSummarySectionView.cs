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
            var description = Model.RiskMetricType == RiskMetricType.HazardIndex
                ? "Risk is summarised by Hazard Index. Hazard Index is {0} divided by {1}."
                : "Risk is summarised by Margin of Exposure. Margin of Exposure is {1} divided by {0}.";
            descriptions.AddDescriptionItem(description, exposureSection, hazCharSection);

            description = $"{{0}} and {{1}} were calculated for {Model.ExposureType.GetDisplayName().ToLower()} {Model.TargetDoseLevel.GetDisplayName().ToLower()} doses.";
            exposureSection.Title = "Exposures";
            hazCharSection.Title = "hazard characterizations";
            descriptions.AddDescriptionItem(description, exposureSection, hazCharSection);

            sb.AppendDescriptionList(descriptions);
        }
    }
}
