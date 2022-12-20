using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorModelCollectionBuilderSettings : IProcessingFactorModelCollectionBuilderSettings {

        private readonly ConcentrationModelSettingsDto _processingFactorModelSettings;

        public ProcessingFactorModelCollectionBuilderSettings(ConcentrationModelSettingsDto processingFactorModelSettings) {
            _processingFactorModelSettings = processingFactorModelSettings;
        }
        public bool IsProcessing => _processingFactorModelSettings.IsProcessing;

        public bool IsDistribution => _processingFactorModelSettings.IsDistribution;

        public bool AllowHigherThanOne => _processingFactorModelSettings.AllowHigherThanOne;
    }
}
