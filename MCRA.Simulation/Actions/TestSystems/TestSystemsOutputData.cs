
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.TestSystems {
    public class TestSystemsOutputData : IModuleOutputData {
        public ICollection<TestSystem> TestSystems { get; set; }
        public IModuleOutputData Copy() {
            return new TestSystemsOutputData() {
                TestSystems = TestSystems
            };
        }
    }
}

