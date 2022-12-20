using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.FoodConversionCalculation;

namespace MCRA.Simulation.Actions.FoodConversions {

    public sealed class FoodConversionModuleSettings
        : IConversionCalculatorSettings {

        private readonly ProjectDto _project;

        public FoodConversionModuleSettings(ProjectDto project) {
            _project = project;
            //To hit for summarysettings
            _ = _project.ConversionSettings.UseSuperTypes;
        }

        public bool UseProcessing {
            get {
                return _project.ConversionSettings.UseProcessing;
            }
        }

        public bool UseComposition {
            get {
                return _project.ConversionSettings.UseComposition;
            }
        }

        public bool UseTdsCompositions {
            get {
                return _project.AssessmentSettings.TotalDietStudy;
            }
        }

        public bool UseReadAcrossFoodTranslations {
            get {
                return _project.ConversionSettings.UseReadAcrossFoodTranslations;
            }
        }

        public bool UseMarketShares {
            get {
                return _project.ConversionSettings.UseMarketShares;
            }
        }

        public bool UseSubTypes {
            get {
                return _project.ConversionSettings.UseSubTypes;
            }
        }

        public bool UseSuperTypes {
            get {
                return _project.ConversionSettings.UseSuperTypes;
            }
        }

        public bool UseDefaultProcessingFactor {
            get {
                return _project.ConversionSettings.UseDefaultProcessingFactor;
            }
        }

        public bool UseWorstCaseValues {
            get {
                return _project.ConversionSettings.UseWorstCaseValues;
            }
        }

        public bool FoodIncludeNonDetects {
            get {
                return _project.ConversionSettings.FoodIncludeNonDetects;
            }
        }

        public bool CompoundIncludeNonDetects {
            get {
                return _project.ConversionSettings.CompoundIncludeNonDetects;
            }
        }

        public bool CompoundIncludeNoMeasurements {
            get {
                return _project.ConversionSettings.CompoundIncludeNoMeasurements;
            }
        }

        public bool SubstanceIndependent {
            get {
                return _project.ConversionSettings.SubstanceIndependent;
            }
        }
    }
}