using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorModelCollection {

        private readonly Dictionary<(Food Food, Compound Substance, ProcessingType Processing), ProcessingFactorModel> _processingFactorModels;

        public ProcessingFactorModelCollection(
            Dictionary<(Food Food, Compound Substance, ProcessingType Processing), ProcessingFactorModel> processingFactorModels
        ) {
            _processingFactorModels = processingFactorModels;
        }

        public ProcessingFactorModel GetProcessingFactorModel(Food food, Compound substance, ICollection<ProcessingType> processingTypes) {
            var processingType = processingTypes?.LastOrDefault();
            if (!_processingFactorModels.TryGetValue((food, substance, processingType), out var processingFactorModel)) {
                _processingFactorModels.TryGetValue((food, null, processingType), out processingFactorModel);
            }
            return processingFactorModel;
        }


        public double GetNominalProcessingFactor(Food food, Compound substance, ProcessingType processingType) {
            if (_processingFactorModels.TryGetValue((food, substance, processingType), out var processingFactorModel) ||
                _processingFactorModels.TryGetValue((food, null, processingType), out processingFactorModel)
            ) {
                return processingFactorModel.GetNominalValue().Value;
            }
            return double.NaN;
        }

        public bool TryGetProcessingFactorModel(
            Food food,
            Compound substance,
            ProcessingType processingType,
            out ProcessingFactorModel processingFactorModel
        ) {
            return _processingFactorModels.TryGetValue((food, substance, processingType), out processingFactorModel)
                || _processingFactorModels.TryGetValue((food, null, processingType), out processingFactorModel);
        }

        public ICollection<ProcessingFactorModel> Values => _processingFactorModels.Values;
    }
}
