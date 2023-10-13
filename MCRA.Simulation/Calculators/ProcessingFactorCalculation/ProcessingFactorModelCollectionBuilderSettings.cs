using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorModelCollectionBuilderSettings : IProcessingFactorModelCollectionBuilderSettings {

        private readonly ConcentrationModelSettings _processingFactorModelSettings;

        public ProcessingFactorModelCollectionBuilderSettings(ConcentrationModelSettings processingFactorModelSettings) {
            _processingFactorModelSettings = processingFactorModelSettings;
        }
        public bool IsProcessing => _processingFactorModelSettings.IsProcessing;

        public bool IsDistribution => _processingFactorModelSettings.IsDistribution;

        public bool AllowHigherThanOne => _processingFactorModelSettings.AllowHigherThanOne;
    }
}
