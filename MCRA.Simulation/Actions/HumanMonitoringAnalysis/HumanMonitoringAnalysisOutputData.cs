using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public class HumanMonitoringAnalysisOutputData : IModuleOutputData {
        public ICollection<HbmIndividualDayCollection> HbmIndividualDayCollections { get; set; }
        public ICollection<HbmIndividualCollection> HbmIndividualCollections { get; set; }
        public ICollection<HbmCumulativeIndividualCollection> HbmCumulativeIndividualCollections { get; set; }
        public ICollection<HbmCumulativeIndividualDayCollection> HbmCumulativeIndividualDayCollections { get; set; }
        public IModuleOutputData Copy() {
            return new HumanMonitoringAnalysisOutputData() {
                HbmCumulativeIndividualCollections = HbmCumulativeIndividualCollections,
                HbmCumulativeIndividualDayCollections = HbmCumulativeIndividualDayCollections,
                HbmIndividualDayCollections = HbmIndividualDayCollections,
                HbmIndividualCollections = HbmIndividualCollections,
            };
        }
    }
}

