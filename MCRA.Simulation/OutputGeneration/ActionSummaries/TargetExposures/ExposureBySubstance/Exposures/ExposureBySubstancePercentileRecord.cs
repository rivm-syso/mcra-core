using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySubstancePercentileRecord : BoxPlotChartRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetLabel() {
            return SubstanceName;
        }
    }
}
