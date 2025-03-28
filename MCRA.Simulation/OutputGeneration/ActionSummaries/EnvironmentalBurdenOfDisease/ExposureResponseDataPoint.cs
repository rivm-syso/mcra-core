using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MCRA.Simulation.OutputGeneration {
    public class ExposureResponseDataPoint {

        [Description("Exposure.")]
        [DisplayName("Exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Percentile specific response value ({EffectMetric}).")]
        [DisplayName("Percentile specific {EffectMetric}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ResponseValue { get; set; }

    }
}
