using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Helper class. For each individual the ID and usual exposure are stored
    /// </summary>
    public class ModelAssistedIntake {

        /// <summary>
        /// The individual.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; set; }

        /// <summary>
        /// Usual exposure based on BBN or LNN0.
        /// </summary>
        public double UsualIntake { get; set; }

        /// <summary>
        /// Back transformed frequency predictions.
        /// </summary>
        public double FrequencyPrediction { get; set; }

        /// <summary>
        /// Back transformed model assisted frequency predictions.
        /// </summary>
        public double ModelAssistedPrediction { get; set; }

        /// <summary>
        /// Model assisted amounts predictions.
        /// </summary>
        public double ModelAssistedAmount { get; set; }

        /// <summary>
        /// Amount predictions.
        /// </summary>
        public double AmountPrediction { get; set; }

        /// <summary>
        /// SqrtRoot(Sigma2Between/(Sigma2Between + Sigma2Within/nDays))
        /// </summary>
        public double ShrinkageFactor { get; set; }

        /// <summary>
        /// Number of positive survey days = frequency
        /// </summary>
        public int NDays { get; set; }

        /// <summary>
        /// Cofactor of amounts model.
        /// </summary>
        public string AmountsCofactor { get; set; }

        /// <summary>
        /// Covariable of amounts model.
        /// </summary>
        public double AmountsCovariable { get; set; }

        /// <summary>
        /// Cofactor of frequency model.
        /// </summary>
        public string FrequencyCofactor { get; set; }

        /// <summary>
        /// Covariable of frequency model.
        /// </summary>
        public double FrequencyCovariable { get; set; }

        /// <summary>
        /// Average transformed positive daily amounts.
        /// </summary>
        public double TransformedOIM { get; set; }

    }
}
