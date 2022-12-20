using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorModelCollection {

        private readonly ThreeKeyDictionary<Food, Compound, ProcessingType, ProcessingFactorModel> _processingFactorModels;

        public ProcessingFactorModelCollection(ThreeKeyDictionary<Food, Compound, ProcessingType, ProcessingFactorModel> processingFactorModels) {
            _processingFactorModels = processingFactorModels;
        }

        public ProcessingFactorModel GetProcessingFactorModel(Food food, Compound substance, ICollection<ProcessingType> processingTypes) {
            var processingType = processingTypes?.LastOrDefault() ?? null;
            if (!_processingFactorModels.TryGetValue(food, substance, processingType, out var processingFactorModel)) {
                _processingFactorModels.TryGetValue(food, null, processingType, out processingFactorModel);
            }
            return processingFactorModel;
        }


        public double GetNominalProcessingFactor(Food food, Compound substance, ProcessingType processingType) {
            if (!_processingFactorModels.TryGetValue(food, substance, processingType, out var processingFactorModel)) {
                _processingFactorModels.TryGetValue(food, null, processingType, out processingFactorModel);
            }
            return processingFactorModel?.GetNominalValue().Item1 ?? double.NaN;
        }

        public bool TryGetProcessingFactorModel(
            Food food,
            Compound substance,
            ProcessingType processingType,
            out ProcessingFactorModel processingFactorModel
        ) {
            if (!_processingFactorModels.TryGetValue(food, substance, processingType, out processingFactorModel)) {
                _processingFactorModels.TryGetValue(food, null, processingType, out processingFactorModel);
            }
            return processingFactorModel != null;
        }

        public ICollection<ProcessingFactorModel> Values {
            get {
                return _processingFactorModels.Values;
            }
        }
    }
}
