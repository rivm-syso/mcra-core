﻿using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PbkModelDefinitions {

    public class PbkModelDefinitionsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.PbkModelDefinitions;

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(isCompute, section);
            return section;
        }
    }
}