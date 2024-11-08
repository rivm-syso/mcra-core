using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
    public enum KineticConversionFactorsSections {
        KineticConversionFactorSection
    }

    public sealed class KineticConversionFactorsSummarizer : ActionModuleResultsSummarizer<KineticConversionFactorsModuleConfig, IKineticConversionFactorsActionResult> {

        public KineticConversionFactorsSummarizer(KineticConversionFactorsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IKineticConversionFactorsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<KineticConversionFactorsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new KineticModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(KineticConversionFactorsSections.KineticConversionFactorSection)
                && (data.KineticConversionFactorModels?.Count > 0)) {
                summarizeKineticConversionFactorModels(
                    data.KineticConversionFactorModels,
                    subHeader,
                    order++
                );
            }
        }

        /// <summary>
        /// Summarize conversion factor models
        /// </summary>
        /// <param name="kineticConversionFactorModels"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeKineticConversionFactorModels(
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorsSummarySection() {
                SectionLabel = getSectionLabel(KineticConversionFactorsSections.KineticConversionFactorSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors", order);
            section.Summarize(kineticConversionFactorModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
