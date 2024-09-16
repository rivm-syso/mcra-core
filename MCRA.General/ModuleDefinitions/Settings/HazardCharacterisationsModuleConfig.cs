using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class HazardCharacterisationsModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;

        [XmlIgnore]
        public bool ConvertToSingleTargetMatrix => HazardCharacterisationsConvertToSingleTargetMatrix;

        public bool RequireDoseResponseModels() {
            return TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms
                || TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds;
        }

        public PointOfDepartureType GetTargetHazardDoseType() {
            switch (PointOfDeparture) {
                case PointOfDeparture.FromReference:
                    return PointOfDepartureType.Unspecified;
                case PointOfDeparture.BMD:
                    return PointOfDepartureType.Bmd;
                case PointOfDeparture.NOAEL:
                    return PointOfDepartureType.Noael;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
