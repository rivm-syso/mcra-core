using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.KineticModels {
    public enum KineticModelsSections {
        KineticConversionFactorSection,
        ParametersSubstanceIndependentSection, // deprecated
        ParametersSubstanceDependentSection, // deprecated
        PbkModelParametersSection,
        AbsorptionFactorsSection,
        HumanKineticModelSection,
        AnimalKineticModelSection
    }

    public sealed class KineticModelsSummarizer : ActionModuleResultsSummarizer<KineticModelsModuleConfig, IKineticModelsActionResult> {

        public KineticModelsSummarizer(KineticModelsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IKineticModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<KineticModelsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new KineticModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(KineticModelsSections.AbsorptionFactorsSection)) {
                summarizeAbsorptionFactors(
                    data.KineticAbsorptionFactors,
                    data.ActiveSubstances ?? data.AllCompounds,
                    _configuration.Aggregate,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticModelsSections.KineticConversionFactorSection)
                && (data.KineticConversionFactorModels?.Any() ?? false)) {
                summarizeKineticConversionFactorModels(
                    data.KineticConversionFactorModels,
                    subHeader,
                    order++
                );
            }
        }

        /// <summary>
        /// Summarize absorption factors
        /// </summary>
        /// <param name="absorptionFactors"></param>
        /// <param name="substances"></param>
        /// <param name="aggregate"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        public void summarizeAbsorptionFactors(
            ICollection<KineticAbsorptionFactor> absorptionFactors,
            ICollection<Compound> substances,
            bool aggregate,
            SectionHeader header,
            int order
        ) {
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.AbsorptionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Absorption factors", order);
            section.SummarizeAbsorptionFactors(
                absorptionFactors,
                substances,
                aggregate
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize conversion factor models
        /// </summary>
        /// <param name="kineticConversionFactorModels"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeKineticConversionFactorModels(
            ICollection<KineticConversionFactorModel> kineticConversionFactorModels,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.KineticConversionFactorSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors", order);
            section.Summarize(kineticConversionFactorModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
