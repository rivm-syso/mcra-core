using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures {
    public class ExternalExposuresSection : ActionSummarySectionBase {

        public int TotalIndividuals { get; set; }

        public int TotalSubstances { get; set; }

        public virtual void Summarize<T>(
            ICollection<T> externalIndividualExposures,
            ICollection<Compound> substances
        ) where T : IExternalIndividualExposure {
            TotalSubstances = substances.Count;
            TotalIndividuals = externalIndividualExposures
                .Select(r => r.SimulatedIndividual.Id)
                .Distinct()
                .Count();
        }
    }
}
