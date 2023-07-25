using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockActionData {
    public class MockActionData : ActionData {

        public MockActionData(ActionData data) {
            ModuleOutputData = data.ModuleOutputData;
            TargetUnitsModels = data.TargetUnitsModels;
        }

        public HashSet<ActionType> Modules { get; set; } = new();

        public override T GetOrCreateModuleOutputData<T>(ActionType actionType) {
            Modules.Add(actionType);
            return base.GetOrCreateModuleOutputData<T>(actionType);
        }
    }
}
