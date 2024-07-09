using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    public sealed class HumanMonitoringAnalysisModuleSettings {

        private readonly HumanMonitoringAnalysisModuleConfig _configuration;
        private readonly bool _isUncertaintyCycle;
        public HumanMonitoringAnalysisModuleSettings(HumanMonitoringAnalysisModuleConfig config, bool isUncertaintyCycle) {
            _configuration = config;
            _isUncertaintyCycle = isUncertaintyCycle;
        }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public NonDetectsHandlingMethod NonDetectsHandlingMethod => _configuration.HbmNonDetectsHandlingMethod;

        public double LorReplacementFactor => _configuration.HbmFractionOfLor;

        public MissingValueImputationMethod MissingValueImputationMethod => _configuration.MissingValueImputationMethod;

        public NonDetectImputationMethod NonDetectImputationMethod => _configuration.NonDetectImputationMethod;

        public BiologicalMatrix TargetMatrix => _configuration.TargetMatrix;

        public bool ApplyKineticConversions => _configuration.ApplyKineticConversions;

        public bool HbmConvertToSingleTargetMatrix => _configuration.HbmConvertToSingleTargetMatrix;

        public TargetLevelType TargetLevelType => _configuration.HbmTargetSurfaceLevel;

        public double MissingValueCutOff => _configuration.MissingValueCutOff;

        public bool StandardiseBlood => _configuration.StandardiseBlood;

        public StandardiseBloodMethod StandardiseBloodMethod => _configuration.StandardiseBloodMethod;

        public bool StandardiseBloodExcludeSubstances => _configuration.StandardiseBloodExcludeSubstances;

        public List<string> StandardiseBloodExcludedSubstancesSubset => _configuration.StandardiseBloodExcludedSubstancesSubset;

        public bool StandardiseUrine => _configuration.StandardiseUrine;

        public StandardiseUrineMethod StandardiseUrineMethod => _configuration.StandardiseUrineMethod;

        public double SpecificGravityConversionFactor => _configuration.SpecificGravityConversionFactor;

        public bool StandardiseUrineExcludeSubstances => _configuration.StandardiseUrineExcludeSubstances;

        public bool SkipPrivacySensitiveOutputs => _configuration.SkipPrivacySensitiveOutputs;

        public List<string> StandardiseUrineExcludedSubstancesSubset => _configuration.StandardiseUrineExcludedSubstancesSubset;

        public bool ApplyExposureBiomarkerConversions => _configuration.ApplyExposureBiomarkerConversions;
    }
}
