using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class IntakePercentileBootstrapRecord {

        [DisplayName("Bootstrap")]
        public int? Bootstrap { get; set; }

        [DisplayName("Percentile")]
        public double Percentile { get; set; }

        [DisplayName("Exposure (IntakeUnit)")]
        public double Exposure { get; set; }

        [DisplayName("Percentage of PoD")]
        public double PercentageOfReferenceDose { get; set; }

        [DisplayName("Margin of exposure")]
        public double MarginOfExposure { get; set; }
    }
}
