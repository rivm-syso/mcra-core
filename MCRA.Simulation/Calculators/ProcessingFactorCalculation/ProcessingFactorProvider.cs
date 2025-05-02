using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorProvider : IProcessingFactorProvider {

        private readonly Dictionary<(Food, Compound, ProcessingType), ProcessingFactorModel> _processingFactorModels;

        private readonly bool _useDefaultForMissing;
        private readonly double _defaultMissingProcessingFactor;

        public ProcessingFactorProvider(
            ICollection<ProcessingFactorModel> processingFactorModels,
            bool useDefaultForMissing,
            double defaultMissingProcessingFactor
        ) {
            _processingFactorModels = processingFactorModels
                .ToDictionary(r => (r.Food, r.Substance, r.ProcessingType));
            _useDefaultForMissing = useDefaultForMissing;
            _defaultMissingProcessingFactor = defaultMissingProcessingFactor;
        }

        /// <summary>
        /// Checks if there is a processing factor for the specified
        /// combination of food, substance, and processing type.
        /// </summary>
        public bool HasProcessingFactor(
            Food food,
            Compound substance,
            ProcessingType processingType
        ) {
            if (_processingFactorModels.ContainsKey((food, substance, processingType))
                || _processingFactorModels.ContainsKey((food, null, processingType))
            ) {
                return true;
            } else if (_useDefaultForMissing && !processingType.IsUnspecified) {
                return true;
            }
            return false;
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
            } else if (_useDefaultForMissing && !processingType.IsUnspecified) {
                return _defaultMissingProcessingFactor;
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
            } else if (_useDefaultForMissing && !processingType.IsUnspecified) {
                return _defaultMissingProcessingFactor;
            } else {
                return double.NaN;
            }
        }

        //Checks whether the processing result should be corrected for processing proportion
        public bool GetProportionProcessingApplication(
            Food food,
            Compound substance,
            ProcessingType processingType
        ) {
            if (_processingFactorModels.TryGetValue((food, substance, processingType), out var model)
                || _processingFactorModels.TryGetValue((food, null, processingType), out model)
            ) {
                return model.GetApplyProcessingCorrectionFactor();
            }
            return false;
        }
    }
}
