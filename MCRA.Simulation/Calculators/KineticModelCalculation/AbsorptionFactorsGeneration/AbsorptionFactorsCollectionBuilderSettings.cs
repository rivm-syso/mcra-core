using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration {
    public sealed class AbsorptionFactorsCollectionBuilderSettings : IAbsorptionFactorsCollectionBuilderSettings {

        private readonly NonDietarySettingsDto _nonDietarySettings;
        public AbsorptionFactorsCollectionBuilderSettings(NonDietarySettingsDto nonDietarySettings) {
            _nonDietarySettings = nonDietarySettings;
        }

        public double DefaultFactorDietary => _nonDietarySettings.OralAbsorptionFactorForDietaryExposure;

        public double DefaultFactorDermalNonDietary => _nonDietarySettings.DermalAbsorptionFactor;

        public double DefaultFactorOralNonDietary => _nonDietarySettings.OralAbsorptionFactor;

        public double DefaultFactorInhalationNonDietary => _nonDietarySettings.InhalationAbsorptionFactor;
    }
}
