namespace MCRA.General.Action.Settings {

    public class NonDietarySettingsDto {

        public virtual bool MatchSpecificIndividuals { get; set; }

        public virtual bool IsCorrelationBetweenIndividuals { get; set; }

        public virtual double OralAbsorptionFactorForDietaryExposure { get; set; } = 1D;

        public virtual double OralAbsorptionFactor { get; set; } = 0.1D;

        public virtual double DermalAbsorptionFactor { get; set; } = 0.1D;

        public virtual double InhalationAbsorptionFactor { get; set; } = 0.1D;
    }
}
