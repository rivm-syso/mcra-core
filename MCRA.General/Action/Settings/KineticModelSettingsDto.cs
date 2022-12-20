namespace MCRA.General.Action.Settings.Dto {

    public class KineticModelSettingsDto {

        public virtual string CodeModel { get; set; }

        public virtual string CodeSubstance { get; set; }

        public virtual string CodeCompartment { get; set; }

        public virtual int NumberOfIndividuals { get; set; } = 100;

        public virtual int NumberOfDays { get; set; } = 50;

        public virtual int NumberOfDosesPerDay { get; set; } = 1;

        public virtual int NumberOfDosesPerDayNonDietaryOral { get; set; } = 1;

        public virtual int NumberOfDosesPerDayNonDietaryDermal { get; set; } = 1;

        public virtual int NumberOfDosesPerDayNonDietaryInhalation { get; set; } = 1;

        public virtual int NonStationaryPeriod { get; set; } = 10;

        public virtual bool UseParameterVariability { get; set; }

        public InternalModelType InternalModelType { get; set; }
    }
}
