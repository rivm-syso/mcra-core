using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.KineticModels {
    public enum AbsorptionFactorsSections {
        AbsorptionFactorsSection,
    }

    public sealed class KineticModelsSummarizer : ActionModuleResultsSummarizer<KineticModelsModuleConfig, IKineticModelsActionResult> {

        public KineticModelsSummarizer(KineticModelsModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IKineticModelsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<AbsorptionFactorsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new KineticModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(AbsorptionFactorsSections.AbsorptionFactorsSection)) {
                summarizeAbsorptionFactors(
                    data,
                    _configuration,
                    subHeader,
                    order++
                );
            }
        }

        /// <summary>
        /// Summarize absorption factors
        /// </summary>
        public void summarizeAbsorptionFactors(
            ActionData data,
            KineticModelsModuleConfig config,
            SectionHeader header,
            int order
        ) {
            var absorptionFactors = data.AbsorptionFactors;
            var substances = data.ActiveSubstances ?? data.AllCompounds;
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(AbsorptionFactorsSections.AbsorptionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Absorption factors", order);
            section.SummarizeAbsorptionFactors(
                absorptionFactors,
                substances,
                config.ExposureRoutes
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
