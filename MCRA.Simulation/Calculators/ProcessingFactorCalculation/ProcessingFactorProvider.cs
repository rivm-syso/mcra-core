using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorProvider {

        private readonly Dictionary<(Food Food, Compound Substance, ProcessingType Processing), ProcessingFactorModel> _processingFactorModels;
        private readonly double _defaultMissingProcessingFactor;

        public ProcessingFactorProvider(
            Dictionary<(Food Food, Compound Substance, ProcessingType Processing), ProcessingFactorModel> processingFactorModels,
            double defaultMissingProcessingFactor
        ) {
            _processingFactorModels = processingFactorModels;
            _defaultMissingProcessingFactor = defaultMissingProcessingFactor;
        }

        public double GetNominalProcessingFactor(Food food, Compound substance, ProcessingType processingType) {
            if (_processingFactorModels.TryGetValue((food, substance, processingType), out var processingFactorModel) ||
                _processingFactorModels.TryGetValue((food, null, processingType), out processingFactorModel)
            ) {
                return processingFactorModel.GetNominalValue();
            }
            return double.NaN;
        }

        private bool tryGetProcessingFactorModel(
            Food food,
            Compound substance,
            ProcessingType processingType,
            out ProcessingFactorModel processingFactorModel
        ) {
            if (_processingFactorModels.TryGetValue((food, substance, processingType), out processingFactorModel)
                || _processingFactorModels.TryGetValue((food, null, processingType), out processingFactorModel)) {
                return true;
            } else {
                processingFactorModel = new PFFixedModel();
                processingFactorModel.CalculateParameters(new ProcessingFactor() {
                    Nominal = _defaultMissingProcessingFactor,
                    ProcessingType = new ProcessingType()
                });
                return false;
            }
        }

        public double GetProcessingFactor(
            Food foodAsMeasured,
            Compound substance,
            ProcessingType processingType,
            IRandom processingFactorsRandomGenerator
        ) {
            if (tryGetProcessingFactorModel(foodAsMeasured, substance, processingType, out var processingFactorModel)) {
                return processingFactorModel.DrawFromDistribution(processingFactorsRandomGenerator);
            } else {
                return (processingType?.IsUnspecified() ?? true) ? 1D : processingFactorModel.GetNominalValue();
            }
        }
        public ICollection<ProcessingFactorModel> Values => _processingFactorModels.Values;
    }
}
