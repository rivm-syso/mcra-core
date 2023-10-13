using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.Consumptions {

    public sealed class ConsumptionsModuleSettings {

        private readonly ProjectDto _project;

        public ConsumptionsModuleSettings(ProjectDto project) {
            _project = project;
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public List<IndividualsSubsetDefinition> IndividualsSubsetDefinitions {
            get {
                return _project.IndividualsSubsetDefinitions;
            }
        }

        public IndividualDaySubsetDefinition IndividualDaySubsetDefinition {
            get {
                return _project.IndividualDaySubsetDefinition;
            }
        }

        public string NameCofactor {
            get {
                return _project.CovariatesSelectionSettings.NameCofactor;
            }
        }

        public string NameCovariable {
            get {
                return _project.CovariatesSelectionSettings.NameCovariable;
            }
        }

        public bool IsDefaultSamplingWeight {
            get {
                return _project.SubsetSettings.IsDefaultSamplingWeight;
            }
        }

        public bool RestrictConsumptionsByFoodAsEatenSubset {
            get {
                return _project.SubsetSettings.RestrictConsumptionsByFoodAsEatenSubset;
            }
        }

        public bool ExcludeIndividualsWithLessThanNDays {
            get {
                return _project.SubsetSettings?.ExcludeIndividualsWithLessThanNDays ?? false;
            }
        }

        public int MinimumNumberOfDays {
            get {
                return _project.SubsetSettings?.MinimumNumberOfDays ?? 2;
            }
        }

        public List<string> FoodAsEatenSubset {
            get {
                return _project.FoodAsEatenSubset;
            }
        }

        public List<string> FocalFoodAsEatenSubset {
            get {
                return _project.FocalFoodAsEatenSubset;
            }
        }

        public bool ConsumerDaysOnly {
            get {
                return _project.SubsetSettings.ConsumerDaysOnly;
            }
        }

        public bool RestrictPopulationByFoodAsEatenSubset {
            get {
                return _project.SubsetSettings.RestrictPopulationByFoodAsEatenSubset;
            }
        }

        public IndividualSubsetType MatchIndividualSubsetWithPopulation {
            get {
                return _project.SubsetSettings.MatchIndividualSubsetWithPopulation;
            }
        }

        public List<string> SelectedFoodSurveySubsetProperties {
            get {
                if (MatchIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                    return _project.SelectedFoodSurveySubsetProperties;
                } else {
                    return null;
                }
            }
        }
    }
}
