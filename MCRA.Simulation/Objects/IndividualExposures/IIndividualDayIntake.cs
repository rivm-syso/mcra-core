﻿using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects {
    public interface IIndividualDayIntake : IIndividualDay {
        /// <summary>
        /// Total exposure; i.e., sum of all substance exposures of this individual dat.
        /// </summary>
        double TotalExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        );

        /// <summary>
        /// Total exposure per bodyweight; i.e., the total exposure divided by the bodyweight.
        /// </summary>
        double TotalExposurePerMassUnit(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        );
    }
}
