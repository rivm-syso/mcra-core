using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Summarizes all amount info for a individual
    /// </summary>
    public class ModelledIndividualAmount(SimulatedIndividual simulatedIndividual) {

        /// <summary>
        /// Identifier of the simulated individual.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; } = simulatedIndividual;

        /// <summary>
        /// Sampling weight of the simulated individual.
        /// </summary>
        public double IndividualSamplingWeight => SimulatedIndividual.SamplingWeight;

        /// <summary>
        /// Cofactor value of the individual.
        /// </summary>
        public string Cofactor { get; set; }

        /// <summary>
        /// Covariable value of the individual.
        /// </summary>
        public double Covariable { get; set; }

        /// <summary>
        /// The number of positive intake days.
        /// </summary>
        public int NumberOfPositiveIntakeDays { get; set; }

        /// <summary>
        /// Amount (or OIM) on transformed scale.
        /// Note this is an intake, not amount, so rename.
        /// </summary>
        public double TransformedAmount { get; set; }

        /// <summary>
        /// Individual day amounts (intakes) on transformed scale.
        /// </summary>
        public double[] TransformedDayAmounts { get; set; }

        /// <summary>
        /// Predictions (or linear predictor).
        /// </summary>
        public double Prediction { get; set; }

        /// <summary>
        /// Modified BLUP on the transformed scale (lp - (oim - lp)* sqrt(factor).
        /// </summary>
        public double ModelAssistedAmount { get; set; }

        public double BackTransformedAmount { get; set; }

        public int NumberOfIndividuals { get; set; }

        public double ShrinkageFactor { get; set; }

    }
}
