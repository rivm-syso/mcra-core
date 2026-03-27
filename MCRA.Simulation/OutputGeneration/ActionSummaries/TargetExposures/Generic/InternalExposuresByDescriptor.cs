using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class InternalExposuresByDescriptor<T> where T : IExposureContributorKey {

        public T Descriptor { get; set; }

        public List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures { get; set; }

    }
}
