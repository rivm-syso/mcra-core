using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public interface IProcessingFactorProvider {

        /// <summary>
        /// Gets a (fixed) nominal processing factor for the specified
        /// combination of food, substance, and processing type.
        /// </summary>
        double GetNominalProcessingFactor(
            Food food,
            Compound substance,
            ProcessingType processingType
        );

        /// <summary>
        /// Gets a processing factor for the specified combination of food,
        /// substance, and processing type. May be a draw from a distribution
        /// model, using the random generator.
        /// </summary>
        double GetProcessingFactor(
            Food food,
            Compound substance,
            ProcessingType processingType,
            IRandom generator
        );
    }
}
