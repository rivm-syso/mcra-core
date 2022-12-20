using MCRA.General;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockUnitVariabilityCalculatorSettings : IUnitVariabilityCalculatorSettings {
        public bool UseUnitVariability { get; set; }
        public UnitVariabilityModelType UnitVariabilityModelType { get; set; }
        public UnitVariabilityType UnitVariabilityType { get; set; }
        public EstimatesNature EstimatesNature { get; set; }
        public int DefaultFactorLow { get; set; }
        public int DefaultFactorMid { get; set; }
        public MeanValueCorrectionType MeanValueCorrectionType { get; set; }
        public UnitVariabilityCorrelationType UnitVariabilityCorrelationType { get; set; }
    }
}
