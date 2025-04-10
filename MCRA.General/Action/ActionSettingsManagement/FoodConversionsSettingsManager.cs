﻿using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class FoodConversionsSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.FoodConversions;

        public override void initializeSettings(ProjectDto project) {
            project.FoodConversionsSettings.SubstanceIndependent = true;
        }

        public override void Verify(ProjectDto project) {
            // Nothing
        }
    }
}
