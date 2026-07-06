using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmSubstanceContributorKey : IHbmExposureContributorKey {
        public Compound Substance { get; set; }

        public string GetKey() {
            return Substance.Code;
        }
    }
}
