using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
 
    public enum KineticConversionFactorsSections {
        KineticConversionFactorsSection,
        KineticConversionFactorModelsSection,
    }

    public sealed class KineticConversionFactorsSummarizer : ActionModuleResultsSummarizer<KineticConversionFactorsModuleConfig, IKineticConversionFactorsActionResult> {

        public KineticConversionFactorsSummarizer(KineticConversionFactorsModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IKineticConversionFactorsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<KineticConversionFactorsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new KineticModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(KineticConversionFactorsSections.KineticConversionFactorModelsSection)) {
                summarizeKineticConversionFactorModels(
                    data,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticConversionFactorsSections.KineticConversionFactorsSection)) {
                summarizeKineticConversionFactors(
                    data,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            ActionModuleConfig sectionConfig,
            IKineticConversionFactorsActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            summarizeKineticConversionFactorsUncertain(data, header);
        }

        /// <summary>
        /// Summarize conversion factor models.
        /// </summary>
        private void summarizeKineticConversionFactorModels(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticConversionFactorsSections.KineticConversionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factor models", order);
            section.Summarize(data.KineticConversionFactorModels);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize conversion factors.
        /// </summary>
        private void summarizeKineticConversionFactors(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorsSummarySection() {
                SectionLabel = getSectionLabel(KineticConversionFactorsSections.KineticConversionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors", order);
            section.Summarize(data.KineticConversionFactorModels);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize kinetic conversion factors uncertainty values.
        /// </summary>
        private void summarizeKineticConversionFactorsUncertain(
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<KineticConversionFactorsSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as KineticConversionFactorsSummarySection;
                section.SummarizeUncertain(data.KineticConversionFactorModels);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
