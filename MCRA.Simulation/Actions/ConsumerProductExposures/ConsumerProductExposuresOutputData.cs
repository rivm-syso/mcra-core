using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;

namespace MCRA.Simulation.Actions.ConsumerProductExposures {
    public class ConsumerProductExposuresOutputData : IModuleOutputData {

        public ICollection<ConsumerProductIndividualIntake> ConsumerProductIndividualExposures { get; set; }
        public ExposureUnitTriple ConsumerProductExposureUnit { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductExposuresOutputData() {
                ConsumerProductExposureUnit = ConsumerProductExposureUnit,
                ConsumerProductIndividualExposures = ConsumerProductIndividualExposures
            };
        }
    }
}

