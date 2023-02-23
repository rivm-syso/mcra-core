using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {

    /// <summary>
    /// Base class for unit variability e.g. based on the LogNormal, Beta or Bernoulli distribution.
    /// </summary>
    public abstract class UnitVariabilityModel {

        public Food Food { get; private set; }
        public UnitVariabilityFactor VariabilityFactor { get; private set; }

        public UnitVariabilityModel(Food food, UnitVariabilityFactor unitVariabilityFactor) {
            Food = food;
            VariabilityFactor = unitVariabilityFactor;
        }

        /// <summary>
        /// <summary>
        /// Calculates the exposure as an aggregation of amount * drawresidue
        /// </summary>
        /// <param name="foodAsMeasured">The (measured) consumed food</param>
        /// <param name="amount">The consumption amount</param>
        /// <param name="residue">Drawn residue(concentration) based on monitoring data.</param>
        /// <param name="random">The random generator used to draw from the distribution function</param>
        /// <returns></returns>
        public abstract List<IntakePortion> DrawFromDistribution(Food foodAsMeasured, float amount, float residue, IRandom random);

        /// <summary>
        /// Calculates the exposure as an aggregation of amount * drawresidue based on an exposure portion
        /// </summary>
        /// <param name="foodAsMeasured">The (measured) consumed food</param>
        /// <param name="intakePortion">The exposure portion (amount and residue)</param>
        /// <param name="random">The random generator used to draw from the distribution function</param>
        /// <returns></returns>
        public List<IntakePortion> DrawFromDistribution(Food foodAsMeasured, IntakePortion intakePortion, IRandom random) {
            return DrawFromDistribution(foodAsMeasured, (float)intakePortion.Amount, intakePortion.Concentration, random);
        }

        /// <summary>
        /// Calculates the model parameters from the VariabilityFactor property.
        /// </summary>
        public abstract bool CalculateParameters();

    }
}
