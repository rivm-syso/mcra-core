using System.Xml.Serialization;

namespace MCRA.General.Action.Settings.Dto {

    public class KineticModelSettingsDto {

        private string _codeCompartment;

        public virtual string CodeModel { get; set; }

        public virtual string CodeSubstance { get; set; }

        [XmlElement("CodeCompartment")]
        public virtual string CodeCompartment {
            get { return _codeCompartment; }
            set {
                _codeCompartment = value;
                BiologicalMatrix = BiologicalMatrixConverter.FromString(value, BiologicalMatrix.Undefined);
            }
        }

        [XmlIgnore]
        public virtual BiologicalMatrix BiologicalMatrix { get; set; }

        public virtual int NumberOfIndividuals { get; set; } = 100;

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
