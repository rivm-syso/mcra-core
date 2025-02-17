using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures {
    public sealed class DustExposuresPercentilesRecord : BoxPlotChartRecord {

        [Description("Exposure route")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetLabel() {
            return SubstanceName;
       }
    }
}
