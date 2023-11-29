using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public class HumanMonitoringAnalysisActionResult : IActionResult {

        public List<HbmIndividualDayCollection> HbmIndividualDayConcentrations { get; set; }
        public List<HbmIndividualCollection> HbmIndividualConcentrations { get; set; }
        public HbmCumulativeIndividualCollection HbmCumulativeIndividualCollection { get; set; }
        public HbmCumulativeIndividualDayCollection HbmCumulativeIndividualDayCollection { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
        public List<DriverSubstance> DriverSubstances { get; set; }
        public ExposureMatrix ExposureMatrix { get; set; }
        public IDictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel> HbmConcentrationModels { get; set; }

        public List<HbmIndividualDayCollection> HbmMeasuredMatrixIndividualDayCollections { get ; set; } 
        public List<HbmIndividualCollection> HbmMeasuredMatrixIndividualCollections { get ; set; }
    }
}
