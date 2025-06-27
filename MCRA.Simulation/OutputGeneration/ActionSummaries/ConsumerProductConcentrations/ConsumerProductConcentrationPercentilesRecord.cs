using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductConcentrationPercentilesRecord : BoxPlotChartRecord {

        [Description("Product name")]
        [DisplayName("Product name")]
        public string ProductName { get; set; }

        [Description("Product code")]
        [DisplayName("Product code")]
        public string ProductCode { get; set; }

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
            return ProductName;
        }
    }
}
