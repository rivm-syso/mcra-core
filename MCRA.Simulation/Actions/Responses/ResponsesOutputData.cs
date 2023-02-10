
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Responses {
    public class ResponsesOutputData : IModuleOutputData {
        public IDictionary<string, Response> Responses { get; set; }
        public IModuleOutputData Copy() {
            return new ResponsesOutputData() {
                Responses = Responses
            };
        }
    }
}

