using System;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class ActionSettingsManagerFactory {

        public static IActionSettingsManager Create(ActionType actionType) {
            return Create(actionType.ToString());
        }

        public static IActionSettingsManager Create(string actionType) {
            IActionSettingsManager result = null;

            var managerType = Type.GetType($"MCRA.General.Action.ActionSettingsManagement.{actionType}SettingsManager", false, true);
            if (managerType != null) {
                result = (IActionSettingsManager)Activator.CreateInstance(managerType);
            }

            return result;
        }
    }
}
