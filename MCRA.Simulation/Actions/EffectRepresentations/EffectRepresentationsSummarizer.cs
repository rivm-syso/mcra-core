using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.EffectRepresentations {
    public enum EffectRepresentationsSections {
        //No sub-sections
    }
    public sealed class EffectRepresentationsSummarizer : ActionResultsSummarizerBase<IEffectRepresentationsActionResult> {

        public override ActionType ActionType => ActionType.EffectRepresentations;

        public override void Summarize(ProjectDto project, IEffectRepresentationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<EffectRepresentationsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new EffectRepresentationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            var records = new List<EffectRepresentationRecord>();
            var allEffectRepresentations = data.AllEffectRepresentations.SelectMany(r => r).ToList();
            foreach (var effectRepresentationInfo in allEffectRepresentations) {
                var record = new EffectRepresentationRecord() {
                    EffectCode = effectRepresentationInfo.Effect.Code,
                    EffectName = effectRepresentationInfo.Effect.Name,
                    ResponseCode = effectRepresentationInfo.Response.Code,
                    ResponseName = effectRepresentationInfo.Response.Name,
                    BenchmarkResponse = effectRepresentationInfo.BenchmarkResponse ?? double.NaN,
                    BenchmarkResponseType = effectRepresentationInfo.BenchmarkResponse != null ? effectRepresentationInfo.BenchmarkResponseType.GetShortDisplayName() : null,
                    BenchmarkResponseUnit = effectRepresentationInfo.Response.ResponseUnit,
                };
                records.Add(record);
            }
            records.TrimExcess();
            section.Records = records;
            subHeader.SaveSummarySection(section);
        }
    }
}
