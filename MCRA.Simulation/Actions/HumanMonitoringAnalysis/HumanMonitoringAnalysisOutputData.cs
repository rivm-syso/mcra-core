using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public class HumanMonitoringAnalysisOutputData : IModuleOutputData {
        public List<TargetUnit> HbmTargetConcentrationUnits { get; set; }
        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }
        public ICollection<HbmCumulativeIndividualConcentration> HbmCumulativeIndividualConcentrations { get; set; }
        public ICollection<HbmCumulativeIndividualDayConcentration> HbmCumulativeIndividualDayConcentrations { get; set; }
        public ICollection<HbmIndividualConcentration> HbmIndividualConcentrations { get; set; }
        public IModuleOutputData Copy() {
            return new HumanMonitoringAnalysisOutputData() {
                HbmTargetConcentrationUnits = HbmTargetConcentrationUnits,
                HbmIndividualConcentrations = HbmIndividualConcentrations,
                HbmCumulativeIndividualConcentrations = HbmCumulativeIndividualConcentrations,
                HbmCumulativeIndividualDayConcentrations = HbmCumulativeIndividualDayConcentrations,
                HbmIndividualDayConcentrations= HbmIndividualDayConcentrations,
            };
        }
    }
}

