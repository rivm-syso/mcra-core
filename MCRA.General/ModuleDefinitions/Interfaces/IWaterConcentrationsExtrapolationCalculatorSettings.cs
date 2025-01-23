namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IWaterConcentrationsExtrapolationCalculatorSettings {
        double WaterConcentrationValue { get; }
        bool RestrictWaterImputationToAuthorisedUses { get; }
        bool RestrictWaterImputationToApprovedSubstances { get; }
        bool RestrictWaterImputationToMostPotentSubstances { get; }
    }
}
