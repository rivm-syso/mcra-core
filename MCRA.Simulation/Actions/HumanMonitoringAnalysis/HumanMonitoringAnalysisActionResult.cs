using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public class HumanMonitoringAnalysisActionResult : IActionResult {
        public TargetUnit HbmTargetUnit { get; set; }
        public List<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }
        public List<HbmIndividualConcentration> HbmIndividualConcentrations { get; set; }
        public List<HbmCumulativeIndividualConcentration> HbmCumulativeIndividualConcentrations { get; set; }
        public List<HbmCumulativeIndividualDayConcentration> HbmCumulativeIndividualDayConcentrations { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
        public List<DriverSubstance> DriverSubstances { get; set; }
        public ExposureMatrix ExposureMatrix { get; set; }
        public IDictionary<Compound, ConcentrationModel> HbmConcentrationModels { get; set; }
    }
}
