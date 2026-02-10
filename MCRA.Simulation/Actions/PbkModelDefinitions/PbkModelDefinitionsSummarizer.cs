using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PbkModelDefinitions {
    public enum PbkModelDefinitionsSections {
        //No sub-sections yet
    }
    public sealed class PbkModelDefinitionsSummarizer : ActionResultsSummarizerBase<IPbkModelDefinitionsActionResult> {
        public override ActionType ActionType => ActionType.PbkModelDefinitions;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IPbkModelDefinitionsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<PbkModelDefinitionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            summarizeSbmlModelDefinitions(subHeader, data);
        }

        private static void summarizeSbmlModelDefinitions(
            SectionHeader header,
            ActionData data
        ) {
            var order = 1;
            var sbmlPbkModelDefinitions = data.AllPbkModelDefinitions
                .Where(r => r.KineticModelDefinition.Format == PbkModelType.SBML)
                .OrderBy(r => r.KineticModelDefinition.Name)
                .ToList();
            summarizeSbmlModelsOverview(header, sbmlPbkModelDefinitions, order++);
            summarizeSbmlModelDetails(header, sbmlPbkModelDefinitions, order++);
        }

        private static void summarizeSbmlModelsOverview(SectionHeader header, List<PbkModelDefinition> sbmlPbkModelDefinitions, int order) {
            var section = new PbkModelDefinitionsOverviewSection();
            section.Summarize(sbmlPbkModelDefinitions);
            var subHeader = header.AddSubSectionHeaderFor(section, "Overview", order);
            subHeader.SaveSummarySection(section);
        }

        private static void summarizeSbmlModelDetails(SectionHeader header, List<PbkModelDefinition> sbmlPbkModelDefinitions, int order) {
            if (sbmlPbkModelDefinitions.Count > 0) {
                var subHeader = header.AddEmptySubSectionHeader("PBK model details", order);
                var subOrder = 1;
                foreach (var modelDefinition in sbmlPbkModelDefinitions) {
                    var subSubSubHeader = subHeader.AddEmptySubSectionHeader(modelDefinition.Name, subOrder++);
                    summarizeSbmlPbkModel(subSubSubHeader, modelDefinition);
                }
            }
        }

        private static void summarizeSbmlPbkModel(
            SectionHeader header,
            PbkModelDefinition modelDefinition
        ) {
            var subOrder = 0;
            {
                var section = new PbkModelDefinitionSummarySection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Compartments", subOrder++);
                section.Summarize(modelDefinition);
                subHeader.SaveSummarySection(section);
            }
            {
                var section = new PbkModelDefinitionParametersSummarySection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Parameters", subOrder++);
                section.Summarize(modelDefinition);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
