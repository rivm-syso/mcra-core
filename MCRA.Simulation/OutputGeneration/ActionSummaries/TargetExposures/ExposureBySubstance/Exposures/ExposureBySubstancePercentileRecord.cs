using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySubstancePercentileRecord : BoxPlotChartRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{SubstanceName} ({Stratification})";
            } else {
                return SubstanceName;
            }
        }
    }
}
