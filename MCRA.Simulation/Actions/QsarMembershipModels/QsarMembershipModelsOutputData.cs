
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.QsarMembershipModels {
    public class QsarMembershipModelsOutputData : IModuleOutputData {
        public ICollection<QsarMembershipModel> QsarMembershipModels { get; set; }
        public IModuleOutputData Copy() {
            return new QsarMembershipModelsOutputData() {
                QsarMembershipModels = QsarMembershipModels,
            };
        }
    }
}

