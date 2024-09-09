using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PbkModels {
    public enum PbkModelsSections {
        PbkModelInstancesSection,
        PbkModelParametersSection
    }

    public sealed class PbkModelsSummarizer : ActionModuleResultsSummarizer<PbkModelsModuleConfig, IPbkModelsActionResult> {

        public PbkModelsSummarizer(PbkModelsModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IPbkModelsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<PbkModelsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new PbkModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(PbkModelsSections.PbkModelInstancesSection)
                && (data.KineticModelInstances?.Any() ?? false)
            ) {
                summarizePbkModelInstances(
                    data,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(PbkModelsSections.PbkModelParametersSection)
                && (data.KineticModelInstances?.Any() ?? false)
            ) {
                summarizePbkModelParameters(
                    data.KineticModelInstances,
                    subHeader,
                    order++
                );
            }
        }

        /// <summary>
        /// Summarize the kinetic model instances.
        /// </summary>
        public void summarizePbkModelInstances(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new PbkModelsSummarySection() {
                SectionLabel = getSectionLabel(PbkModelsSections.PbkModelInstancesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "PBK model instances", order);
            section.Summarize(data.KineticModelInstances);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Substance independent parameters
        /// </summary>
        private void summarizePbkModelParameters(
            ICollection<KineticModelInstance> kineticModelInstances,
            SectionHeader header,
            int order
        ) {
            var section = new PbkModelParametersSummarySection() {
                SectionLabel = getSectionLabel(PbkModelsSections.PbkModelParametersSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "PBK model parameters", order);
            section.Summarize(kineticModelInstances);
            subHeader.SaveSummarySection(section);
        }
    }
}
