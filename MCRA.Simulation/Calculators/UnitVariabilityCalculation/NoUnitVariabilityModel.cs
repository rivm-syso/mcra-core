using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {

    /// <summary>
    /// No unit variability is applied
    /// </summary>
    public sealed class NoUnitVariabilityModel : UnitVariabilityModel {

        public NoUnitVariabilityModel(Food food, UnitVariabilityFactor unitVariabilityFactor)
            : base(food, unitVariabilityFactor) {
        }

        public override List<IntakePortion> DrawFromDistribution(Food FoodAsMeasured, float amount, float residue, IRandom random) {
            return new List<IntakePortion>() {
                new IntakePortion() {
                    Amount = amount,
                    Concentration = residue,
                },
            };
        }

        /// <summary>
        /// Override: calculates the parameters for this unit variability model type.
        /// For this special model, it does not do anything.
        /// </summary>
        public override bool CalculateParameters() {
            return true;
        }
    }
}
