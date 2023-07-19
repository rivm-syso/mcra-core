using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {

    public class KineticModelSettingsDto {

        public virtual string CodeModel { get; set; }

        public virtual string CodeSubstance { get; set; }

        [XmlElement("CodeCompartment")]
        public virtual string CodeCompartment { get; set; }

        public virtual int NumberOfDays { get; set; } = 50;

        public virtual int NumberOfDosesPerDay { get; set; } = 1;

        public virtual int NumberOfDosesPerDayNonDietaryOral { get; set; } = 1;

        public virtual int NumberOfDosesPerDayNonDietaryDermal { get; set; } = 1;

        public virtual int NumberOfDosesPerDayNonDietaryInhalation { get; set; } = 1;

        public virtual int NonStationaryPeriod { get; set; } = 10;

        public virtual bool UseParameterVariability { get; set; }

        public InternalModelType InternalModelType { get; set; }

        public virtual bool SpecifyEvents { get; set; }

        public int[] SelectedEvents { get; set; } = new int[6] { 1, 2, 3, 4, 5, 6 };
    }
}
