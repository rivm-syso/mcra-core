using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.MolecularDockingModels {
    public enum MolecularDockingModelsSections {
        //No sub-sections
    }
    public sealed class MolecularDockingModelsSummarizer : ActionResultsSummarizerBase<IMolecularDockingModelsActionResult> {

        public override ActionType ActionType => ActionType.MolecularDockingModels;

        public override void Summarize(ProjectDto project, IMolecularDockingModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<MolecularDockingModelsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new MolecularDockingModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(
                data.MolecularDockingModels,
                data.AllCompounds.ToHashSet()
            );
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.SaveSummarySection(section);

            var subOrder = 0;
            SummarizeBindingEnergies(
                data.MolecularDockingModels,
                data.AllCompounds,
                subHeader,
                subOrder++
            );
            if (data.MolecularDockingModels.Count > 1) {
                SummarizeDockingModelCorrelations(
                    data.MolecularDockingModels,
                    data.AllCompounds,
                    subHeader,
                    subOrder++
                );
            }
        }

        public void SummarizeBindingEnergies(
                ICollection<MolecularDockingModel> molecularDockingModels,
                ICollection<Compound> allCompounds,
                SectionHeader header,
                int order
            ) {
            var section = new MolecularDockingModelsBindingEnergiesSection();
            section.Summarize(molecularDockingModels, allCompounds.ToHashSet());
            var subHeader = header.AddSubSectionHeaderFor(section, "Substance binding energies", order);
            subHeader.SaveSummarySection(section);
        }

        public void SummarizeDockingModelCorrelations(
                ICollection<MolecularDockingModel> molecularDockingModels,
                ICollection<Compound> allCompounds,
                SectionHeader header,
                int order
            ) {
            var section = new MolecularDockingModelCorrelationsSummarySection();
            section.Summarize(molecularDockingModels, allCompounds.ToHashSet());
            var subHeader = header.AddSubSectionHeaderFor(section, "Docking model correlations", order);
            subHeader.SaveSummarySection(section);
        }
    }
}
