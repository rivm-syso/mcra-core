using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Consumptions {

    public sealed class ConsumptionsModuleSettings {

        private readonly ConsumptionsModuleConfig _moduleConfig;

        public ConsumptionsModuleSettings(ConsumptionsModuleConfig config) {
            _moduleConfig = config;
        }

        public ExposureType ExposureType {
            get {
                return _moduleConfig.ExposureType;
            }
        }

        public List<IndividualsSubsetDefinition> IndividualsSubsetDefinitions {
            get {
                return _moduleConfig.IndividualsSubsetDefinitions;
            }
        }

        public IndividualDaySubsetDefinition IndividualDaySubsetDefinition {
            get {
                return _moduleConfig.IndividualDaySubsetDefinition;
            }
        }

        public string NameCofactor {
            get {
                return _moduleConfig.NameCofactor;
            }
        }

        public string NameCovariable {
            get {
                return _moduleConfig.NameCovariable;
            }
        }

        public bool IsDefaultSamplingWeight {
            get {
                return _moduleConfig.IsDefaultSamplingWeight;
            }
        }

        public bool RestrictConsumptionsByFoodAsEatenSubset {
            get {
                return _moduleConfig.RestrictConsumptionsByFoodAsEatenSubset;
            }
        }

        public bool ExcludeIndividualsWithLessThanNDays {
            get {
                return _moduleConfig?.ExcludeIndividualsWithLessThanNDays ?? false;
            }
        }

        public int MinimumNumberOfDays {
            get {
                return _moduleConfig?.MinimumNumberOfDays ?? 2;
            }
        }

        public List<string> FoodAsEatenSubset {
            get {
                return _moduleConfig.FoodAsEatenSubset;
            }
        }

        public List<string> FocalFoodAsEatenSubset {
            get {
                return _moduleConfig.FocalFoodAsEatenSubset;
            }
        }

        public bool ConsumerDaysOnly {
            get {
                return _moduleConfig.ConsumerDaysOnly;
            }
        }

        public bool RestrictPopulationByFoodAsEatenSubset {
            get {
                return _moduleConfig.RestrictPopulationByFoodAsEatenSubset;
            }
        }

        public IndividualSubsetType MatchIndividualSubsetWithPopulation {
            get {
                return _moduleConfig.MatchIndividualSubsetWithPopulation;
            }
        }

        public List<string> SelectedFoodSurveySubsetProperties {
            get {
                if (MatchIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                    return _moduleConfig.SelectedFoodSurveySubsetProperties;
                } else {
                    return null;
                }
            }
        }
    }
}
