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
            return PointOfDeparture switch {
                PointOfDeparture.FromReference => PointOfDepartureType.Unspecified,
                PointOfDeparture.BMD => PointOfDepartureType.Bmd,
                PointOfDeparture.NOAEL => PointOfDepartureType.Noael,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
