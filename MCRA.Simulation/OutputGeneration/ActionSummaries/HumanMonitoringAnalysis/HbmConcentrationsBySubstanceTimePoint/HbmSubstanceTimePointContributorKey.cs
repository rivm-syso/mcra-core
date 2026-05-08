using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmSubstanceTimePointContributorKey : IHbmExposureContributorKey {
        public Compound Substance { get; set; }
        public HumanMonitoringTimepoint TimePoint { get; set; }

        public string GetKey() {
            return $"{Substance.Code}-{TimePoint.Code}";
        }
    }
}
