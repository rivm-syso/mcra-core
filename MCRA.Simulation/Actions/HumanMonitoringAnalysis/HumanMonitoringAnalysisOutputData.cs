using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public class HumanMonitoringAnalysisOutputData : IModuleOutputData {
        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }
        public ICollection<HbmCumulativeIndividualConcentration> HbmCumulativeIndividualConcentrations { get; set; }
        public ICollection<HbmCumulativeIndividualDayConcentration> HbmCumulativeIndividualDayConcentrations { get; set; }
        public ICollection<HbmIndividualConcentration> HbmIndividualConcentrations { get; set; }
        public IModuleOutputData Copy() {
            return new HumanMonitoringAnalysisOutputData() {
                HbmIndividualConcentrations = HbmIndividualConcentrations,
                HbmCumulativeIndividualConcentrations = HbmCumulativeIndividualConcentrations,
                HbmCumulativeIndividualDayConcentrations = HbmCumulativeIndividualDayConcentrations,
                HbmIndividualDayConcentrations= HbmIndividualDayConcentrations,
            };
        }
    }
}

