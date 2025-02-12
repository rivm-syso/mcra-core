﻿using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class RisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.Risks;

        public override void initializeSettings(ProjectDto project) {
            project.PopulationsSettings.IsCompute = true;
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ActionSettings.SelectedTier, ActionType);
        }
    }
}
