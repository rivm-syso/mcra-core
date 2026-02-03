using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.OpexProductDefinitions.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.KineticConversionFactors {

    public enum KineticConversionFactorsSections {
        KineticConversionFactorsSection,
        KineticConversionFactorModelsSection,
    }

    public sealed class KineticConversionFactorsSummarizer : ActionModuleResultsSummarizer<KineticConversionFactorsModuleConfig, KineticConversionFactorsActionResult> {

        public KineticConversionFactorsSummarizer(KineticConversionFactorsModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig actionConfig,
            KineticConversionFactorsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<KineticConversionFactorsSections>(actionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new KineticModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(KineticConversionFactorsSections.KineticConversionFactorModelsSection)) {
                summarizeKineticConversionFactors(
                    data,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticConversionFactorsSections.KineticConversionFactorsSection)) {
                summarizeKineticConversionFactorModels(
                    data,
                    subHeader,
                    order++
                );
            }

            summarizeDerivedKineticConversionFactorModels(data, order++, subHeader);
        }

        public void SummarizeUncertain(
            ActionModuleConfig actionConfig,
            KineticConversionFactorsActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            summarizeKineticConversionFactorsUncertain(data, header);
        }

        /// <summary>
        /// Summarize conversion factors.
        /// </summary>
        private void summarizeKineticConversionFactors(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticConversionFactorsSections.KineticConversionFactorModelsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factor models", order);
            section.Summarize(data.KineticConversionFactorModels);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize conversion factor models.
        /// </summary>
        private void summarizeKineticConversionFactorModels(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var conversionFactors = data.KineticConversionFactorModels
                .Where(r => r is IKineticConversionFactorDataModel)
                .Select(r => (r as IKineticConversionFactorDataModel).ConversionRule)
                .ToList();
            if (conversionFactors.Count > 0) {
                var section = new KineticConversionFactorsDataSummarySection() {
                    SectionLabel = getSectionLabel(KineticConversionFactorsSections.KineticConversionFactorsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors data", order);
                section.Summarize(conversionFactors);
                subHeader.SaveSummarySection(section);
            }
        }

        private static void summarizeDerivedKineticConversionFactorModels(
            ActionData data,
            int order,
            SectionHeader header
        ) {
            var empiricalKineticConversionFactorModels = data.KineticConversionFactorModels
                .Where(r => r is KineticConversionFactorEmpiricalModel)
                .Cast<KineticConversionFactorEmpiricalModel>()
                .ToList();
            if (empiricalKineticConversionFactorModels?.Count > 0) {
                var section = new DerivedKineticConversionFactorModelsSummarySection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Derived kinetic conversion factor models", order);
                section.Summarize(empiricalKineticConversionFactorModels);
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Summarize kinetic conversion factors uncertainty values.
        /// </summary>
        private void summarizeKineticConversionFactorsUncertain(
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<KineticConversionFactorModelsSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as KineticConversionFactorModelsSummarySection;
                section.SummarizeUncertain(data.KineticConversionFactorModels);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
