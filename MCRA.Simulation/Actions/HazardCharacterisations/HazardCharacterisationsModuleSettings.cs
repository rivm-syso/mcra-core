using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    public class HazardCharacterisationsModuleSettings {

        private readonly HazardCharacterisationsModuleConfig _configuration;

        public HazardCharacterisationsModuleSettings(HazardCharacterisationsModuleConfig config) {
            _configuration = config;
        }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public bool Aggregate {
            get {
                return _configuration.Aggregate;
            }
        }

        public string CodeReferenceSubstance {
            get {
                return TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms
                    ? _configuration.CodeReferenceSubstance
                    : string.Empty;
            }
        }

        public TargetLevelType TargetDoseLevel {
            get {
                return _configuration.TargetDoseLevelType;
            }
        }

        public PointOfDeparture PointOfDeparture {
            get {
                return _configuration.PointOfDeparture;
            }
        }

        public bool RestrictToCriticalEffect {
            get {
                return _configuration.RestrictToCriticalEffect;
            }
        }

        public bool RestrictToAvailableHazardCharacterisations {
            get {
                return _configuration.FilterByAvailableHazardCharacterisation;
            }
        }

        public bool UseAdditionalAssessmentFactor {
            get {
                return _configuration.UseAdditionalAssessmentFactor;
            }
        }

        public bool UseInterSpeciesConversionFactors {
            get {
                return _configuration.UseInterSpeciesConversionFactors;
            }
        }

        public bool UseIntraSpeciesConversionFactors {
            get {
                return _configuration.UseIntraSpeciesConversionFactors;
            }
        }

        public double AdditionalAssessmentFactor {
            get {
                return _configuration.AdditionalAssessmentFactor;
            }
        }

        public TargetDosesCalculationMethod TargetDosesCalculationMethod {
            get {
                return _configuration.TargetDosesCalculationMethod;
            }
        }

        public TargetDoseSelectionMethod TargetDoseSelectionMethod {
            get {
                return _configuration.TargetDoseSelectionMethod;
            }
        }

        public bool UseBMDL {
            get {
                return _configuration.UseBMDL;
            }
        }

        public bool HCSubgroupDependent {
            get {
                return _configuration.HCSubgroupDependent;
            }
        }
        public bool ImputeMissingHazardDoses {
            get {
                return _configuration.ImputeMissingHazardDoses;
            }
        }

        public HazardDoseImputationMethodType HazardDoseImputationMethod {
            get {
                return _configuration.HazardDoseImputationMethod;
            }
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

        public BiologicalMatrix TargetMatrix {
            get {
                return _configuration.TargetMatrix;
            }
        }

        public bool ConvertToSingleTargetMatrix {
            get {
                return _configuration.HazardCharacterisationsConvertToSingleTargetMatrix;
            }
        }

        public InternalModelType internalModelType {
            get {
                return _configuration.InternalModelType;
            }
        }
    }
}
