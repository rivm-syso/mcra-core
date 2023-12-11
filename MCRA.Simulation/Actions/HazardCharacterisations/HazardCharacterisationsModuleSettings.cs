using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    public class HazardCharacterisationsModuleSettings {

        private readonly ProjectDto _project;

        public HazardCharacterisationsModuleSettings(ProjectDto project) {
            _project = project;
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public bool Aggregate {
            get {
                return _project.AssessmentSettings.Aggregate;
            }
        }

        public string CodeReferenceSubstance {
            get {
                return TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms
                    ? _project.EffectSettings?.CodeReferenceCompound
                    : string.Empty;
            }
        }

        public TargetLevelType TargetDoseLevel {
            get {
                return _project.EffectSettings.TargetDoseLevelType;
            }
        }

        public PointOfDeparture PointOfDeparture {
            get {
                return _project.EffectSettings.PointOfDeparture;
            }
        }

        public bool RestrictToCriticalEffect {
            get {
                return _project.EffectSettings.RestrictToCriticalEffect;
            }
        }

        public bool RestrictToAvailableHazardCharacterisations {
            get {
                return _project.EffectSettings.RestrictToAvailableHazardCharacterisations;
            }
        }

        public bool UseAdditionalAssessmentFactor {
            get {
                return _project.EffectSettings.UseAdditionalAssessmentFactor;
            }
        }

        public bool UseInterSpeciesConversionFactors {
            get {
                return _project.EffectSettings.UseInterSpeciesConversionFactors;
            }
        }

        public bool UseIntraSpeciesConversionFactors {
            get {
                return _project.EffectSettings.UseIntraSpeciesConversionFactors;
            }
        }

        public double AdditionalAssessmentFactor {
            get {
                return _project.EffectSettings.AdditionalAssessmentFactor;
            }
        }

        public TargetDosesCalculationMethod TargetDosesCalculationMethod {
            get {
                return _project.EffectSettings.TargetDosesCalculationMethod;
            }
        }

        public TargetDoseSelectionMethod TargetDoseSelectionMethod {
            get {
                return _project.EffectSettings.TargetDoseSelectionMethod;
            }
        }

        public bool UseBMDL {
            get {
                return _project.EffectSettings.UseBMDL;
            }
        }

        public bool HCSubgroupDependent {
            get {
                return _project.EffectSettings.HCSubgroupDependent;
            }
        }
        public bool ImputeMissingHazardDoses {
            get {
                return _project.EffectSettings.ImputeMissingHazardDoses;
            }
        }

        public HazardDoseImputationMethodType HazardDoseImputationMethod {
            get {
                return _project.EffectSettings.HazardDoseImputationMethod;
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
                return _project.EffectSettings.TargetMatrix;
            }
        }

        public bool ConvertToSingleTargetMatrix {
            get {
                return _project.EffectSettings.HazardCharacterisationsConvertToSingleTargetMatrix;
            }
        }
    }
}
