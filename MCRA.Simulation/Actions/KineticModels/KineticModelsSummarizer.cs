using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
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
                    data.AbsorptionFactors,
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

            if (outputSettings.ShouldSummarize(KineticModelsSections.AnimalKineticModelSection)
                && (data.KineticModelInstances?.Any(r => !r.IsHumanModel) ?? false)) {
                summarizeAnimalKineticModels(
                    data.KineticModelInstances,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticModelsSections.PbkModelParametersSection)
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
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.HumanKineticModelSection)
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
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.AnimalKineticModelSection)
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
                SectionLabel = getSectionLabel(KineticModelsSections.PbkModelParametersSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "PBK model parameters", order);
            section.Summarize(kineticModelInstances);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize absorption factors
        /// </summary>
        /// <param name="absorptionFactors"></param>
        /// <param name="kineticAbsorptionFactors"></param>
        /// <param name="substances"></param>
        /// <param name="aggregate"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        public void summarizeAbsorptionFactors(
            IDictionary<(ExposurePathType Route, Compound Substance), double> absorptionFactors,
            ICollection<KineticAbsorptionFactor> kineticAbsorptionFactors,
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
                kineticAbsorptionFactors,
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
