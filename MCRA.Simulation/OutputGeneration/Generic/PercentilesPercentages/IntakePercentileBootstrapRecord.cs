using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public interface IIntakePercentileBootstrapRecord {
        int? Bootstrap { get; set; }
        double Percentile { get; set; }
        double Value { get; set; }
    }

    public class IntakePercentileExposureBootstrapRecord: IIntakePercentileBootstrapRecord {
        [DisplayName("Bootstrap")]
        public int? Bootstrap { get; set; }

        [DisplayName("Percentile")]
        public double Percentile { get; set; }

        [DisplayName("Exposure (IntakeUnit)")]
        public double Value { get; set; }
    }

    public class IntakePercentileRiskBootstrapRecord : IIntakePercentileBootstrapRecord {
        [DisplayName("Bootstrap")]
        public int? Bootstrap { get; set; }

        [DisplayName("Percentile")]
        public double Percentile { get; set; }

        [DisplayName("Threshold value/exposure")]
        public double Value { get; set; }
    }
}
