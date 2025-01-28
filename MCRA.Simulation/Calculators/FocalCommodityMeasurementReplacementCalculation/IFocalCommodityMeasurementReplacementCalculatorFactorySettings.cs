using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {
    public interface IFocalCommodityMeasurementReplacementCalculatorFactorySettings {
        FocalCommodityReplacementMethod FocalCommodityReplacementMethod { get; }
        double FocalCommodityScenarioOccurrencePercentage { get; }
        double FocalCommodityConcentrationAdjustmentFactor { get; }
        bool FocalCommodityIncludeProcessedDerivatives { get; }
    }
}
