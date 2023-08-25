using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;

namespace MCRA.Simulation.Actions.ConcentrationModels {

    public sealed class ConcentrationModelsModuleSettings :
        IConcentrationModelCalculationSettings 
    {

        private readonly ProjectDto _project;

        public ConcentrationModelsModuleSettings(ProjectDto project) {
            _project = project;
        }

        public bool TotalDietStudy {
            get {
                return _project.AssessmentSettings.TotalDietStudy;
            }
        }

        public bool IsSampleBased {
            get {
                return _project.ConcentrationModelSettings.IsSampleBased;
            }
        }

        public bool IsMultipleSubstances {
            get {
                return _project.AssessmentSettings.MultipleSubstances;
            }
        }

        public bool Cumulative {
            get {
                return _project.AssessmentSettings.Cumulative;
            }
        }

        // Concentration models

        public SettingsTemplateType ConcentrationModelChoice {
            get {
                return _project.ConcentrationModelSettings.ConcentrationModelChoice;
            }
        }

        public ConcentrationModelType DefaultConcentrationModel {
            get {
                return _project.ConcentrationModelSettings.DefaultConcentrationModel;
            }
        }

        public ICollection<ConcentrationModelTypePerFoodCompoundDto> ConcentrationModelTypesPerFoodCompound {
            get {
                return _project.ConcentrationModelSettings.ConcentrationModelTypesPerFoodCompound;
            }
        }

        public NonDetectsHandlingMethod NonDetectsHandlingMethod {
            get {
                return _project.ConcentrationModelSettings.NonDetectsHandlingMethod;
            }
        }

        public double FractionOfLOR {
            get {
                return _project.ConcentrationModelSettings.FractionOfLOR;
            }
        }

        public bool RestrictLorImputationToAuthorisedUses {
            get {
                return _project.ConcentrationModelSettings.RestrictLorImputationToAuthorisedUses;
            }
        }

        public double FractionOfMrl {
            get {
                return _project.ConcentrationModelSettings.FractionOfMrl;
            }
        }

        public bool IsFallbackMrl {
            get {
                return _project.ConcentrationModelSettings.IsFallbackMrl;
            }
        }

        public bool CorrelateImputedValueWithSamplePotency {
            get {
                return _project.ConcentrationModelSettings.CorrelateImputedValueWithSamplePotency;
            }
        }

        public bool ImputeMissingValues {
            get {
                return _project.ConcentrationModelSettings.ImputeMissingValues;
            }
        }

        // Uncertainty settings

        public bool ReSampleConcentrations {
            get {
                return _project.UncertaintyAnalysisSettings.ReSampleConcentrations;
            }
        }

        public bool IsParametric {
            get {
                return _project.UncertaintyAnalysisSettings.IsParametric;
            }
        }
    }
}
