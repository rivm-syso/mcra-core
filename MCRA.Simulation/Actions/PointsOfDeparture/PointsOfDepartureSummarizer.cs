using System.Linq;
using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.PointsOfDeparture {
    public enum PointsOfDepartureSections {
        //No sub-sections
    }
    public sealed class PointsOfDepartureSummarizer : ActionResultsSummarizerBase<IPointsOfDepartureActionResult> {

        public override ActionType ActionType => ActionType.PointsOfDeparture;

        public override void Summarize(ProjectDto project, IPointsOfDepartureActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<PointsOfDepartureSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new PointsOfDepartureSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.PointsOfDeparture.GetDisplayName(), order);
            section.Records = data.PointsOfDeparture.Select(c => new PointsOfDepartureSummaryRecord() {
                Code = c.Code,
                CompoundCode = c.Compound.Code,
                CompoundName = c.Compound.Name,
                EffectCode = c.Effect.Code,
                EffectName = c.Effect.Name,
                ModelEquation = c.DoseResponseModelEquation,
                ParameterValues = c.DoseResponseModelParameterValues,
                PointOfDeparture = c.LimitDose,
                PointOfDepartureType = c.PointOfDepartureType.GetShortDisplayName(),
                ExposureRoute = c.ExposureRoute.GetShortDisplayName(),
                System = c.Species,
                Unit = c.DoseUnit.GetShortDisplayName(),
                CriticalEffectSize = c.CriticalEffectSize,
            }).ToList();
            subHeader.SaveSummarySection(section);
        }
    }
}
