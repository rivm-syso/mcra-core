using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorProvider : IProcessingFactorProvider {

        private readonly Dictionary<(Food, Compound, ProcessingType), ProcessingFactorModel> _processingFactorModels;
        private readonly double _defaultMissingProcessingFactor;

        public ProcessingFactorProvider(
            ICollection<ProcessingFactorModel> processingFactorModels,
            double defaultMissingProcessingFactor
        ) {
            _processingFactorModels = processingFactorModels
                .ToDictionary(r => (r.Food, r.Substance, r.ProcessingType));
            _defaultMissingProcessingFactor = defaultMissingProcessingFactor;
        }

        /// <summary>
        /// Gets a (fixed) nominal processing factor for the specified
        /// combination of food, substance, and processing type.
        /// </summary>
        public double GetNominalProcessingFactor(
            Food food,
            Compound substance,
            ProcessingType processingType
        ) {
            if (_processingFactorModels.TryGetValue((food, substance, processingType), out var model)
                || _processingFactorModels.TryGetValue((food, null, processingType), out model)
            ) {
                return model.GetNominalValue();
            } else {
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets a processing factor for the specified combination of food,
        /// substance, and processing type. May be a draw from a distribution
        /// model, using the random generator.
        /// </summary>
        public double GetProcessingFactor(
            Food food,
            Compound substance,
            ProcessingType processingType,
            IRandom generator
        ) {
            if (_processingFactorModels.TryGetValue((food, substance, processingType), out var model)
                || _processingFactorModels.TryGetValue((food, null, processingType), out model)
            ) {
                return model.DrawFromDistribution(generator);
            } else {
                return processingType.IsUnspecified()
                    ? 1D : _defaultMissingProcessingFactor;
            }
        }
    }
}
