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

        public SettingsTemplateType DietaryIntakeCalculationTier {
            get {
                return _configuration.DietaryIntakeCalculationTier;
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
                    return _configuration.NumberOfIterationsPerResampledSet;
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

        // Processing

        public bool IsProcessing {
            get {
                return _configuration.IsProcessing;
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

        public bool IsCorrelation {
            get {
                return !_configuration.IsSampleBased && _configuration.IsCorrelation;
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
                return _configuration.ExposureApproachType;
            }
        }

        // Driver substance calculation

        public double TotalExposureCutOff {
            get {
                return _configuration.MixtureSelectionTotalExposureCutOff;
            }
        }

        public double RatioCutOff {
            get {
                return _configuration.MixtureSelectionRatioCutOff;
            }
        }

        // Intake modelling

        public bool CovariateModelling {
            get {
                return _configuration.CovariateModelling;
            }
        }

        public IntakeModelType IntakeModelType {
            get {
                return _configuration.IntakeModelType;
            }
        }

        public bool FirstModelThenAdd {
            get {
                return _configuration.FirstModelThenAdd;
            }
        }

        public ICollection<IntakeModelPerCategory> IntakeModelsPerCategory {
            get {
                return _configuration.IntakeModelsPerCategory;
            }
        }

        public double Dispersion {
            get {
                return _configuration.Dispersion;
            }
        }

        public double VarianceRatio {
            get {
                return _configuration.VarianceRatio;
            }
        }

        public TransformType TransformType {
            get {
                return _configuration.TransformType;
            }
        }

        public double GridPrecision {
            get {
                return _configuration.GridPrecision;
            }
        }

        public bool SplineFit {
            get {
                return _configuration.SplineFit;
            }
        }

        // Other

        public bool UseScenario {
            get {
                return _configuration.UseScenario;
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

        public double Intervals {
            get {
                return _configuration.Intervals;
            }
        }

        public double[] ExtraPredictionLevels {
            get {
                return _configuration.ExtraPredictionLevels.ToArray();
            }
        }
    }
}
