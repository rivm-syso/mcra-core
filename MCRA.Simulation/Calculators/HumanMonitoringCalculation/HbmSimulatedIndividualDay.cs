using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public class HbmSimulatedIndividualDay : SimulatedIndividualDay {
        public HbmSimulatedIndividualDay(SimulatedIndividual simulatedIndividual) : base(simulatedIndividual) {
        }

        public HumanMonitoringTimepoint TimePoint { get; set; }
    }
}
