using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Summarizes all frequency info for an individual
    /// </summary>
    public class IndividualFrequency(SimulatedIndividual simulatedIndividual) {

        /// <summary>
        /// Identifier of the simulated individual.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; } = simulatedIndividual;

        /// <summary>
        /// Sampling weight of the simulated individual.
        /// </summary>
        public double SamplingWeight => SimulatedIndividual.SamplingWeight;

        /// <summary>
        /// Cofactor value of the individual.
        /// </summary>
        public string Cofactor { get; set; }

        /// <summary>
        /// Covariable value of the individual.
        /// </summary>
        public double Covariable { get; set; }

        /// <summary>
        /// Number of days individual is in survey.
        /// </summary>
        public int? Nbinomial { get; set; }

        /// <summary>
        /// The intake frequency of the simulated individual.
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// The frequency prediction.
        /// </summary>
        public double Prediction { get; set; }

        /// <summary>
        /// Prediction are based on the fitted frequency model.
        /// </summary>
        public double ModelAssistedFrequency { get; set; }

        public int NumberOfIndividuals { get; set; }
    }
}
