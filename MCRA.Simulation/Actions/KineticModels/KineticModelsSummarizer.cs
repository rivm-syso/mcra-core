using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.KineticModels {
    public enum KineticModelsSections {
        KineticConversionFactorSection,
        ParametersSubstanceIndependentSection,
        ParametersSubstanceDependentSection,
        AbsorptionFactorsSection,
        HumanKineticModelSection,
        AnimalKineticModelSection
    }

    public sealed class KineticModelsSummarizer : ActionResultsSummarizerBase<IKineticModelsActionResult> {

        public override ActionType ActionType => ActionType.KineticModels;

        public override void Summarize(ProjectDto project, IKineticModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<KineticModelsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new KineticModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);

            if (outputSettings.ShouldSummarize(KineticModelsSections.HumanKineticModelSection)
                && (data.KineticModelInstances?.Any(r => r.IsHumanModel) ?? false)) {
                summarizeHumanKineticModels(
                    data.KineticModelInstances,
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

            if (outputSettings.ShouldSummarize(KineticModelsSections.AbsorptionFactorsSection)) {
                summarizeAbsorptionFactors(
                    data.AbsorptionFactors,
                    data.KineticAbsorptionFactors,
                    data.ActiveSubstances ?? data.AllCompounds,
                    project.AssessmentSettings.Aggregate,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticModelsSections.ParametersSubstanceIndependentSection)
                && (data.KineticModelInstances?.Any(r => r.IsHumanModel) ?? false)) {
                summarizeParametersSubstanceIndependent(
                    data.KineticModelInstances,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticModelsSections.ParametersSubstanceDependentSection)
                && (data.KineticModelInstances?.Any(r => r.IsHumanModel) ?? false)) {
                summarizeParametersSubstanceDependent(
                    data.KineticModelInstances,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(KineticModelsSections.KineticConversionFactorSection)
                && (data.KineticConversionFactors?.Any() ?? false)) {
                summarizeKineticConversionFactors(
                    data.KineticConversionFactors,
                    data.ActiveSubstances ?? data.AllCompounds,
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
            var subHeader = header.AddSubSectionHeaderFor(section, "Human kinetic models", order);
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
            var subHeader = header.AddSubSectionHeaderFor(section, "Animal kinetic models", order);
            section.SummarizeAnimalKineticModels(kineticModelInstances);
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
        /// Substance dependent parameters (metabolic)
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        public void summarizeParametersSubstanceDependent(
            ICollection<KineticModelInstance> kineticModelInstances,
            SectionHeader header,
            int order
        ) {
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.ParametersSubstanceDependentSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic parameters substance dependent", order);
            section.SummarizeParametersSubstanceDependent(kineticModelInstances);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Substance independent parameters
        /// </summary>
        /// <param name="kineticModelInstances"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeParametersSubstanceIndependent(
            ICollection<KineticModelInstance> kineticModelInstances,
            SectionHeader header,
            int order
        ) {
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.ParametersSubstanceIndependentSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic parameters substance independent", order);
            section.SummarizeParametersSubstanceIndependent(kineticModelInstances);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize conversion factors
        /// </summary>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="substances"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeKineticConversionFactors(
            ICollection<KineticConversionFactor> kineticConversionFactors,
            ICollection<Compound> substances,
            SectionHeader header,
            int order
        ) {
            var section = new KineticModelsSummarySection() {
                SectionLabel = getSectionLabel(KineticModelsSections.KineticConversionFactorSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors", order);
            section.SummarizeConversionFactors(kineticConversionFactors, substances);
            subHeader.SaveSummarySection(section);
        }
    }
}
