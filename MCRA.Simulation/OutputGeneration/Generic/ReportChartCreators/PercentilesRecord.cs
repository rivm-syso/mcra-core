using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PercentilesRecord : BoxPlotChartRecord {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Number of measurements.")]
        [DisplayName("Number of measurements")]
        public int NumberOfMeasurements { get; set; }

        public override string GetLabel() {
            throw new NotImplementedException();
        }
    }
}
