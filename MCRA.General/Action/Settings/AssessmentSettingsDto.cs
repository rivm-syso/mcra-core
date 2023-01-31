namespace MCRA.General.Action.Settings.Dto {

    public class AssessmentSettingsDto {

        public virtual ExposureType ExposureType { get; set; } = ExposureType.Acute;

        public virtual InternalConcentrationType InternalConcentrationType { get; set; } = InternalConcentrationType.ModelledConcentration;

        public virtual bool MultipleSubstances { get; set; }

        public virtual bool Cumulative { get; set; }

        public virtual bool Aggregate { get; set; }

        public virtual bool FocalCommodity { get; set; }

        public virtual bool MultipleHealthEffects { get; set; }

        public virtual bool TotalDietStudy { get; set; }

        public virtual bool UseMonitoringDataForTDS { get; set; }

    }
}
