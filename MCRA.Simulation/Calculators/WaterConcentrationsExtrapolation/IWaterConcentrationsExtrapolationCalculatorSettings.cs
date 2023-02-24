namespace MCRA.Simulation.Calculators.WaterConcentrationsExtrapolation {
    public interface IWaterConcentrationsExtrapolationCalculatorSettings {
        double WaterConcentrationValue { get; }
        bool RestrictWaterImputationToAuthorisedUses { get; }
        bool RestrictWaterImputationToApprovedSubstances { get; }
        bool RestrictWaterImputationToMostPotentSubstances { get; }
    }
}
