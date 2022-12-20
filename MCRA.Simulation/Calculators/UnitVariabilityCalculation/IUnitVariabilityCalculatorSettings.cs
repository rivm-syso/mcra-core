using MCRA.General;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {
    public interface IUnitVariabilityCalculatorSettings {

        bool UseUnitVariability { get; }
        UnitVariabilityModelType UnitVariabilityModelType { get; }
        UnitVariabilityType UnitVariabilityType { get; }
        EstimatesNature EstimatesNature { get; }
        int DefaultFactorLow { get; }
        int DefaultFactorMid { get; }
        MeanValueCorrectionType MeanValueCorrectionType { get; }
        UnitVariabilityCorrelationType UnitVariabilityCorrelationType { get; }
    }
}
