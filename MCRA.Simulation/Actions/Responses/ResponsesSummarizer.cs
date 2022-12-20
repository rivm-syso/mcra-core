using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Actions.Responses {
    public enum ResponsesSections {
        //No sub-sections
    }
    public sealed class ResponsesSummarizer : ActionResultsSummarizerBase<IResponsesActionResult> {

        public override ActionType ActionType => ActionType.Responses;

        public override void Summarize(ProjectDto project, IResponsesActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ResponsesSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ResponseSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            summarize(data.Responses, section);
            subHeader.SaveSummarySection(section);
        }

        private static void summarize(IDictionary<string, Response> responses, ResponseSummarySection section) {
            section.Records = responses.Values.Select(c => new ResponseSummaryRecord() {
                ResponseCode = c.Code,
                ResponseName = c.Name,
                Description = c.Description,
                IdSystem = c.TestSystem?.Code,
                ResponseType = c.ResponseTypeString,
                ResponseUnit = c.ResponseUnit,
                GuidelineMethod = c.GuidelineMethod,
            }).ToList();
        }
    }
}
