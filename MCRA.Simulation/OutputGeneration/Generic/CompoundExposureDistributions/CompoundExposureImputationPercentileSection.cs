using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundExposureImputationPercentileSection : SummarySection {

        [DisplayName("Percentiles")]
        public IntakePercentileSection IntakePercentileSection { get; set; }

        [DisplayName("Retain & Refine percentiles")]
        public IntakePercentileSection RRIntakePercentileSection { get; set; }

    }
}
