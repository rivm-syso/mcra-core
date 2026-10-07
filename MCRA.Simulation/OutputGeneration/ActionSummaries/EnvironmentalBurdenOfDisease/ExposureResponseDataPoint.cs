using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureResponseDataPoint {
        [Display(AutoGenerateField = false)]
        public int ExposureBin { get; set; }

        [Description("Exposure.")]
        [DisplayName("Exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Percentile specific response value ({EffectMetric}).")]
        [DisplayName("Percentile specific {EffectMetric}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ResponseValue { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> ResponseValues { get; set; }

        [Display(AutoGenerateField = false)]
        public double MedianResponseValue { get { return ResponseValues.Count != 0 ? ResponseValues.Percentile(50d) : double.NaN; } }
    }
}
