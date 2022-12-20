using MCRA.General;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {
    public interface IFocalCommodityMeasurementReplacementCalculatorFactorySettings {
        FocalCommodityReplacementMethod FocalCommodityReplacementMethod { get; }
        double FocalCommodityScenarioOccurrencePercentage { get; }
        double FocalCommodityConcentrationAdjustmentFactor { get; }
    }
}
