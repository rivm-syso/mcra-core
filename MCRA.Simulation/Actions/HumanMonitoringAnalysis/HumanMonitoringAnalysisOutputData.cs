using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public class HumanMonitoringAnalysisOutputData : IModuleOutputData {
        public ICollection<HbmIndividualDayCollection> HbmIndividualDayCollections { get; set; }
        public ICollection<HbmIndividualCollection> HbmIndividualCollections { get; set; }
        public HbmCumulativeIndividualCollection HbmCumulativeIndividualCollection { get; set; }
        public HbmCumulativeIndividualDayCollection HbmCumulativeIndividualDayCollection { get; set; }
        public IModuleOutputData Copy() {
            return new HumanMonitoringAnalysisOutputData() {
                HbmCumulativeIndividualCollection = HbmCumulativeIndividualCollection,
                HbmCumulativeIndividualDayCollection = HbmCumulativeIndividualDayCollection,
                HbmIndividualDayCollections = HbmIndividualDayCollections,
                HbmIndividualCollections = HbmIndividualCollections,
            };
        }
    }
}

