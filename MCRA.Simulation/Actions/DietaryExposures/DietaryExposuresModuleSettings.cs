using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ISUFCalculator;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public sealed class DietaryExposuresModuleSettings :
        IPopulationGeneratorFactorySettings,
        IIntakeCalculatorFactorySettings,
        IUnitVariabilityCalculatorSettings,
        IDriverSubstanceCalculatorSettings,
        IResidueGeneratorSettings
    {

        private readonly DietaryExposuresModuleConfig _configuration;

        private readonly bool _isUncertaintyCycle;

        public DietaryExposuresModuleSettings(DietaryExposuresModuleConfig config, bool isUncertaintyCycle) {
            _configuration = config;

            FrequencyModelCalculationSettings = new FrequencyModelCalculationSettings(_configuration);

            AmountModelCalculationSettings = new AmountModelCalculationSettings(_configuration);

            ISUFModelCalculationSettings = new ISUFModelCalculationSettings(new () {
                IntakeModelsPerCategory = config.IntakeModelsPerCategory,
                IntakeModelType = config.IntakeModelType
            });

            _isUncertaintyCycle = isUncertaintyCycle;
        }

        public IIntakeModelCalculationSettings FrequencyModelCalculationSettings { get; set; }

        public IIntakeModelCalculationSettings AmountModelCalculationSettings { get; set; }

        public IISUFModelCalculationSettings ISUFModelCalculationSettings { get; set; }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public SettingsTemplateType DietaryExposuresTier {
            get {
                return _configuration.SelectedTier;
            }
        }

        public bool TotalDietStudy {
            get {
                return _configuration.TotalDietStudy;
            }
        }

        public bool Cumulative {
            get {
                return _configuration.Cumulative;
            }
        }

        // Monte-carlo settings

        public int NumberOfMonteCarloIterations {
            get {
                if (!_isUncertaintyCycle) {
                    return _configuration.NumberOfMonteCarloIterations;
                } else {
                    return _configuration.UncertaintyIterationsPerResampledSet;
                }
            }
        }

        public int NumberOfSimulatedIndividualDays {
            get {
                return NumberOfMonteCarloIterations;
            }
        }

        public bool IsSurveySampling {
            get {
                return _configuration.IsSurveySampling;
            }
        }

        // Unit variability

        public bool UseUnitVariability {
            get {
                return _configuration.UseUnitVariability;
            }
        }

        public UnitVariabilityModelType UnitVariabilityModelType {
            get {
                return _configuration.UnitVariabilityModel;
            }
        }

        public UnitVariabilityType UnitVariabilityType {
            get {
                return _configuration.UnitVariabilityType;
            }
        }

        public EstimatesNature EstimatesNature {
            get {
                return _configuration.EstimatesNature;
            }
        }

        public int DefaultFactorLow {
            get {
                return _configuration.DefaultFactorLow;
            }
        }

        public int DefaultFactorMid {
            get {
                return _configuration.DefaultFactorMid;
            }
        }

        public MeanValueCorrectionType MeanValueCorrectionType {
            get {
                return _configuration.MeanValueCorrectionType;
            }
        }

        public UnitVariabilityCorrelationType UnitVariabilityCorrelationType {
            get {
                return _configuration.CorrelationType;
            }
        }

        // Residue generation

        public bool IsSampleBased {
            get {
                return _configuration.IsSampleBased;
            }
        }

        public bool IsSingleSamplePerDay {
            get {
                return _configuration.IsSingleSamplePerDay;
            }
        }

        public bool MaximiseCoOccurrenceHighResidues {
            get {
                return !_configuration.IsSampleBased && _configuration.MaximiseCoOccurrenceHighResidues;
            }
        }

        public ConcentrationModelType DefaultConcentrationModel {
            get {
                return _configuration.DefaultConcentrationModel;
            }
        }

        public NonDetectsHandlingMethod NonDetectsHandlingMethod {
            get {
                return _configuration.NonDetectsHandlingMethod;
            }
        }

        public bool UseOccurrencePatternsForResidueGeneration {
            get {
                return _configuration.UseOccurrencePatternsForResidueGeneration;
            }
        }

        public bool TreatMissingOccurrencePatternsAsNotOccurring {
            get {
                return _configuration.SetMissingAgriculturalUseAsUnauthorized;
            }
        }

        public bool UseEquivalentsModel {
            get {
                return DefaultConcentrationModel != ConcentrationModelType.Empirical;
            }
        }

        // Exposure imputation

        public bool ImputeExposureDistributions {
            get {
                return _configuration.ImputeExposureDistributions;
            }
        }

        public ExposureApproachType ExposureApproachTypeDietary {
            get {
                return _configuration.McrExposureApproachType;
            }
        }

        // Driver substance calculation

        public double TotalExposureCutOff {
            get {
                return _configuration.McrCalculationTotalExposureCutOff;
            }
        }

        public double RatioCutOff {
            get {
                return _configuration.McrCalculationRatioCutOff;
            }
        }

        // Intake modelling

        public bool IntakeCovariateModelling {
            get {
                return _configuration.IntakeCovariateModelling;
            }
        }

        public IntakeModelType IntakeModelType {
            get {
                return _configuration.IntakeModelType;
            }
        }

        public bool IntakeFirstModelThenAdd {
            get {
                return _configuration.IntakeFirstModelThenAdd;
            }
        }

        public ICollection<IntakeModelPerCategory> IntakeModelsPerCategory {
            get {
                return _configuration.IntakeModelsPerCategory;
            }
        }

        public double FrequencyModelDispersion {
            get {
                return _configuration.FrequencyModelDispersion;
            }
        }

        public double VarianceRatio {
            get {
                return _configuration.AmountModelVarianceRatio;
            }
        }

        public TransformType TransformType {
            get {
                return _configuration.AmountModelTransformType;
            }
        }

        public double GridPrecision {
            get {
                return _configuration.IsufModelGridPrecision;
            }
        }

        public bool SplineFit {
            get {
                return _configuration.IsufModelSplineFit;
            }
        }

        // Other

        public bool ReductionToLimitScenario {
            get {
                return _configuration.ReductionToLimitScenario;
            }
        }

        public List<string> SelectedScenarioAnalysisFoods {
            get {
                return _configuration.ScenarioAnalysisFoods;
            }
        }

        // Output settings / reporting

        public bool IsPerPerson {
            get {
                return _configuration.IsPerPerson;
            }
        }

        public bool UseReadAcrossFoodTranslations {
            get {
                return _configuration.UseReadAcrossFoodTranslations;
            }
        }

        public DietaryExposuresDetailsLevel DietaryExposuresDetailsLevel {
            get {
                return _configuration.DietaryExposuresDetailsLevel;
            }
        }

        public double IntakeModelPredictionIntervals {
            get {
                return _configuration.IntakeModelPredictionIntervals;
            }
        }

        public double[] IntakeExtraPredictionLevels {
            get {
                return _configuration.IntakeExtraPredictionLevels.ToArray();
            }
        }
    }
}
