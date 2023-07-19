using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.IntraSpeciesFactors {
    public enum IntraSpeciesFactorsSections { }
    public sealed class IntraSpeciesFactorsSummarizer : ActionResultsSummarizerBase<IIntraSpeciesFactorsActionResult> {

        public override ActionType ActionType => ActionType.IntraSpeciesFactors;

        public override void Summarize(ProjectDto project, IIntraSpeciesFactorsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<IntraSpeciesFactorsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new IntraSpeciesFactorsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Records = data.IntraSpeciesFactors.Select(c => new IntraSpeciesFactorsSummaryRecord() {
                EffectCode = c.Effect.Code,
                EffectName = c.Effect.Name,
                CompoundCode = c.Compound?.Code,
                CompoundName = c.Compound?.Name,
                LowerVariationFactor = c.LowerVariationFactor ?? double.NaN,
                UpperVariationFactor = c.UpperVariationFactor,
            }).ToList();
            section.DefaultIntraSpeciesFactor = project.EffectModelSettings.DefaultIntraSpeciesFactor;
            subHeader.SaveSummarySection(section);
        }
    }
}
