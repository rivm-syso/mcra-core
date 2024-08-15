using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PbkModels {
    public enum PbkModelsSections {
        ParametersSubstanceIndependentSection, // deprecated
        ParametersSubstanceDependentSection, // deprecated
        PbkModelParametersSection,
        HumanKineticModelSection,
        AnimalKineticModelSection
    }

    public sealed class PbkModelsSummarizer : ActionModuleResultsSummarizer<PbkModelsModuleConfig, IPbkModelsActionResult> {

        public PbkModelsSummarizer(PbkModelsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IPbkModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<PbkModelsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new PbkModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(PbkModelsSections.AnimalKineticModelSection)
                && (data.KineticModelInstances?.Any(r => !r.IsHumanModel) ?? false)) {
                summarizeAnimalKineticModels(
                    data.KineticModelInstances,
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
        /// Summarize human kinetic models
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeHumanKineticModels(
            ICollection<KineticModelInstance> kineticModelInstances,
            SectionHeader header,
            int order
        ) {
            var section = new PbkModelsSummarySection() {
                SectionLabel = getSectionLabel(PbkModelsSections.HumanKineticModelSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "PBK models (human)", order);
            section.SummarizeHumanKineticModels(kineticModelInstances);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize animal kinetic models
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        public void summarizeAnimalKineticModels(
            ICollection<KineticModelInstance> kineticModelInstances,
            SectionHeader header,
            int order
        ) {
            var section = new PbkModelsSummarySection() {
                SectionLabel = getSectionLabel(PbkModelsSections.AnimalKineticModelSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "PBK models (animal)", order);
            section.SummarizeAnimalKineticModels(kineticModelInstances);
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
