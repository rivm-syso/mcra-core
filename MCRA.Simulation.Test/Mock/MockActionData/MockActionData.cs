using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockActionData {
    public class MockActionData : ActionData {

        public MockActionData(ActionData data) {
            ModuleOutputData = data.ModuleOutputData;
        }

        public HashSet<ActionType> Modules { get; set; } = [];

        public override T GetOrCreateModuleOutputData<T>(ActionType actionType) {
            Modules.Add(actionType);
            return base.GetOrCreateModuleOutputData<T>(actionType);
        }
    }
}
