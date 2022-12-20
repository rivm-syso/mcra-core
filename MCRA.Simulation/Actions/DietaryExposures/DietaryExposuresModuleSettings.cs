using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ISUFCalculator;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public sealed class DietaryExposuresModuleSettings :
        IPopulationGeneratorFactorySettings,
        IIntakeCalculatorFactorySettings,
        IUnitVariabilityCalculatorSettings,
        IDriverSubstanceCalculatorSettings,
        IResidueGeneratorSettings
    {

        private readonly ProjectDto _project;

        private readonly bool _isUncertaintyCycle;

        public DietaryExposuresModuleSettings(ProjectDto project, bool isUncertaintyCycle) {
            _project = project;
            //Hit summarizer settings
            _ = _project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier;
            _ = _project.IntakeModelSettings.CovariateModelling;
            FrequencyModelCalculationSettings = new FrequencyModelCalculationSettings(_project.FrequencyModelSettings);
            AmountModelCalculationSettings = new AmountModelCalculationSettings(_project.AmountModelSettings);
            ISUFModelCalculationSettings = new ISUFModelCalculationSettings(_project.IntakeModelSettings);
            _isUncertaintyCycle = isUncertaintyCycle;
        }

        public IIntakeModelCalculationSettings FrequencyModelCalculationSettings { get; set; }

        public IIntakeModelCalculationSettings AmountModelCalculationSettings { get; set; }

        public IISUFModelCalculationSettings ISUFModelCalculationSettings { get; set; }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public DietaryIntakeCalculationTier DietaryIntakeCalculationTier {
            get {
                return _project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier;
            }
        }

        public bool TotalDietStudy {
            get {
                return _project.AssessmentSettings.TotalDietStudy;
            }
        }

        public bool Cumulative {
            get {
                return _project.AssessmentSettings.Cumulative;
            }
        }

        // Monte-carlo settings

        public int NumberOfMonteCarloIterations {
            get {
                if (!_isUncertaintyCycle) {
                    return _project.MonteCarloSettings.NumberOfMonteCarloIterations;
                } else {
                    return _project.UncertaintyAnalysisSettings.NumberOfIterationsPerResampledSet;
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
                return _project.MonteCarloSettings.IsSurveySampling;
            }
        }

        // Unit variability

        public bool UseUnitVariability {
            get {
                return _project.UnitVariabilitySettings.UseUnitVariability;
            }
        }

        public UnitVariabilityModelType UnitVariabilityModelType {
            get {
                return _project.UnitVariabilitySettings.UnitVariabilityModel;
            }
        }

        public UnitVariabilityType UnitVariabilityType {
            get {
                return _project.UnitVariabilitySettings.UnitVariabilityType;
            }
        }

        public EstimatesNature EstimatesNature {
            get {
                return _project.UnitVariabilitySettings.EstimatesNature;
            }
        }

        public int DefaultFactorLow {
            get {
                return _project.UnitVariabilitySettings.DefaultFactorLow;
            }
        }

        public int DefaultFactorMid {
            get {
                return _project.UnitVariabilitySettings.DefaultFactorMid;
            }
        }

        public MeanValueCorrectionType MeanValueCorrectionType {
            get {
                return _project.UnitVariabilitySettings.MeanValueCorrectionType;
            }
        }

        public UnitVariabilityCorrelationType UnitVariabilityCorrelationType {
            get {
                return _project.UnitVariabilitySettings.CorrelationType;
            }
        }

        // Processing

        public bool IsProcessing {
            get {
                return _project.ConcentrationModelSettings.IsProcessing;
            }
        }

        // Residue generation

        public bool IsSampleBased {
            get {
                return _project.ConcentrationModelSettings.IsSampleBased;
            }
        }

        public bool IsSingleSamplePerDay {
            get {
                return _project.ConcentrationModelSettings.IsSingleSamplePerDay;
            }
        }

        public bool IsCorrelation {
            get {
                return !_project.ConcentrationModelSettings.IsSampleBased && _project.ConcentrationModelSettings.IsCorrelation;
            }
        }

        public ConcentrationModelType DefaultConcentrationModel {
            get {
                return _project.ConcentrationModelSettings.DefaultConcentrationModel;
            }
        }

        public NonDetectsHandlingMethod NonDetectsHandlingMethod {
            get {
                return _project.ConcentrationModelSettings.NonDetectsHandlingMethod;
            }
        }

        public bool UseOccurrencePatternsForResidueGeneration {
            get {
                return _project.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration;
            }   
        }

        public bool TreatMissingOccurrencePatternsAsNotOccurring {
            get {
                return _project.AgriculturalUseSettings.SetMissingAgriculturalUseAsUnauthorized;
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
                return _project.DietaryIntakeCalculationSettings.ImputeExposureDistributions;
            }
        }

        public ExposureApproachType ExposureApproachType {
            get {
                return _project.MixtureSelectionSettings.ExposureApproachType;
            }
        }

        // Driver substance calculation

        public double TotalExposureCutOff {
            get {
                return _project.MixtureSelectionSettings.TotalExposureCutOff;
            }
        }

        public double RatioCutOff {
            get {
                return _project.MixtureSelectionSettings.RatioCutOff;
            }
        }

        // Intake modelling

        public bool CovariateModelling {
            get {
                return _project.IntakeModelSettings.CovariateModelling;
            }
        }

        public IntakeModelType IntakeModelType {
            get {
                return _project.IntakeModelSettings.IntakeModelType;
            }
        }

        public bool FirstModelThenAdd {
            get {
                return _project.IntakeModelSettings.FirstModelThenAdd;
            }
        }

        public ICollection<IntakeModelPerCategoryDto> IntakeModelsPerCategory {
            get {
                return _project.IntakeModelSettings.IntakeModelsPerCategory;
            }
        }

        public double Dispersion {
            get {
                return _project.IntakeModelSettings.Dispersion;
            }
        }

        public double VarianceRatio {
            get {
                return _project.IntakeModelSettings.VarianceRatio;
            }
        }

        public TransformType TransformType {
            get {
                return _project.IntakeModelSettings.TransformType;
            }
        }

        public double GridPrecision {
            get {
                return _project.IntakeModelSettings.GridPrecision;
            }
        }

        public bool SplineFit {
            get {
                return _project.IntakeModelSettings.SplineFit;
            }
        }

        // Other

        public bool UseScenario {
            get {
                return _project.ScenarioAnalysisSettings.UseScenario;
            }
        }

        public List<SelectedScenarioAnalysisFoodDto> SelectedScenarioAnalysisFoods {
            get {
                return _project.SelectedScenarioAnalysisFoods;
            }
        }

        // Output settings / reporting

        public bool IsPerPerson {
            get {
                return _project.SubsetSettings.IsPerPerson;
            }
        }

        public bool UseReadAcrossFoodTranslations {
            get {
                return _project.ConversionSettings.UseReadAcrossFoodTranslations;
            }
        }

        public DietaryExposuresDetailsLevel DietaryExposuresDetailsLevel {
            get {
                return _project.DietaryIntakeCalculationSettings.DietaryExposuresDetailsLevel;
            }
        }

        public double Intervals {
            get {
                return _project.OutputDetailSettings.Intervals;
            }
        }

        public double[] ExtraPredictionLevels {
            get {
                return _project.OutputDetailSettings.ExtraPredictionLevels;
            }
        }
    }
}
