using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryExposureBySubstancePercentileRecord : BoxPlotChartRecord{
        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Description.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        public override string GetLabel() {
            return Description;
        }
    }
}
