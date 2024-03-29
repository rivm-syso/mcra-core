﻿using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetExposure {

        Individual Individual { get; }
        double CompartmentWeight { get; }
        double RelativeCompartmentWeight { get; }
        double IntraSpeciesDraw { get; set; }

        ISubstanceTargetExposureBase GetSubstanceTargetExposure(Compound compound);

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amount divided by the
        /// volume of the target.
        /// </summary>
        double GetSubstanceConcentrationAtTarget(
            Compound substance,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the target exposure value for a substance, corrected for 
        /// relative potency and membership probability.
        /// </summary>
        double GetExposureForSubstance(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        );

        /// <summary>
        /// Get the total (cumulative) substance amount of the target 
        /// exposure corrected for the provided RPFs and memberships.
        /// I.e., the total absolute (corrected) amounts, not divided by
        /// a volume or unit (making it a concentration).
        /// </summary>
        double TotalAmountAtTarget(
            IDictionary<Compound, double> relativePotencyFactors, 
            IDictionary<Compound, double> membershipProbabilities
        );
        
        /// <summary>
        /// Get the total substance concentration
        /// </summary>
        double TotalConcentrationAtTarget(
            IDictionary<Compound, double> relativePotencyFactors, 
            IDictionary<Compound, double> membershipProbabilities, 
            bool isPerPerson
        );

        /// <summary>
        /// Returns true if the exposure is positive.
        /// </summary>
        bool IsPositiveExposure();
    }
}
