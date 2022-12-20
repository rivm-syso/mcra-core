using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ComponentSelectionSection : SummarySection {
        /// <summary>
        /// Contains for each component a list of the nmf-values of all substances
        /// </summary>
        public List<SubstanceComponentRecord> SubstancecComponentRecords { get; set; }
        public double Sparseness { get; set; }
        public double PercentageExplainedVariance { get; set; }
        public int NumberOfIterations { get; set; }
        public int NumberOfCompounds { get; set; }
        public int ComponentNumber { get; set; }
        /// <summary>
        /// Remove substances with zero coefficients in all components (no information)
        /// </summary>
        public bool Selection { get; set; }

        /// <summary>
        /// Summary of statistics per component
        /// </summary>
        /// <param name="componentRecord"></param>
        /// <param name="substanceComponentRecords"></param>
        /// <param name="substances"></param>
        /// <param name="componentId"></param>
        /// <param name="removeZeros"></param>
        public void SummarizePerComponent(
            ComponentRecord componentRecord,
            List<SubstanceComponentRecord> substanceComponentRecords,
            List<Compound> substances,
            int componentId,
            bool removeZeros
        ) {
            Sparseness = componentRecord.Sparseness;
            NumberOfIterations = componentRecord.Iteration;
            PercentageExplainedVariance = componentRecord.Variance;
            NumberOfCompounds = substances.Count;
            Selection = removeZeros;
            ComponentNumber = componentId + 1;
            SubstancecComponentRecords = substanceComponentRecords;
        }
    }
}
