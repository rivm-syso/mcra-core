using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryExposureSourceSummaryRecord {

        [DisplayName("Food name")]
        public string Name { get; set; }

        [DisplayName("Food code")]
        public string Code { get; set; }

        [DisplayName("Parent code")]
        public string CodeParent { get; set; }

    }
}
