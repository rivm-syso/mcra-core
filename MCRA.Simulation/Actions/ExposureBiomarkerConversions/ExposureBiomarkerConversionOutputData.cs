
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {
    public class ExposureBiomarkerConversionsOutputData : IModuleOutputData {

        public ICollection<ExposureBiomarkerConversion> ExposureBiomarkerConversions { get; set; }

        public IModuleOutputData Copy() {
            return new ExposureBiomarkerConversionsOutputData() {
                ExposureBiomarkerConversions = ExposureBiomarkerConversions
            };
        }
    }
}

