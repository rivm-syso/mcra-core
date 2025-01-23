namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IFocalCommodityMeasurementReplacementCalculatorFactorySettings {
        FocalCommodityReplacementMethod FocalCommodityReplacementMethod { get; }
        double FocalCommodityScenarioOccurrencePercentage { get; }
        double FocalCommodityConcentrationAdjustmentFactor { get; }
        bool FocalCommodityIncludeProcessedDerivatives { get; }
    }
}
