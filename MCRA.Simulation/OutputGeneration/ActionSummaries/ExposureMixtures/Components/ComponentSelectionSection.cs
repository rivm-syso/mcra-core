using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ComponentCalculation.Component;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ComponentSelectionSection : SummarySection {
        /// <summary>
        /// Contains for each component a list of the nmf-values of all substances
        /// </summary>
        public List<List<SubstanceComponentRecord>> SubstancecComponentRecords { get; set; }

        public List<ComponentRecord> ComponentRecords { get; set; }
        public int NumberOfCompounds { get; set; }
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
        /// <param name="removeZeros"></param>
        public void SummarizePerComponent(
            List<ComponentRecord> componentRecords,
            List<List<SubstanceComponentRecord>> substanceComponentRecords,
            List<Compound> substances,
            bool removeZeros
        ) {
            NumberOfCompounds = substances.Count;
            Selection = removeZeros;
            SubstancecComponentRecords = substanceComponentRecords;
            ComponentRecords = componentRecords;
        }
    }
}
