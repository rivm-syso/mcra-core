using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceContributorKey : IExposureContributorKey {
        public Compound Substance { get; set; }

        public string GetKey() {
            return Substance.Code;
        }
    }
}