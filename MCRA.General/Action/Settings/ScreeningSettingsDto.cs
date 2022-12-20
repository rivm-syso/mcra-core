namespace MCRA.General.Action.Settings.Dto {

    public class ScreeningSettingsDto {

        public virtual double CriticalExposurePercentage { get; set; } = 95D;

        public virtual double CumulativeSelectionPercentage { get; set; } = 95D;

        public virtual double ImportanceLor { get; set; } = 0D;
    }
}
