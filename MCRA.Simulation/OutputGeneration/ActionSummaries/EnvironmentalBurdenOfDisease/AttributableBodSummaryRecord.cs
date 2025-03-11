using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class AttributableBodSummaryRecord {
        [Description("Burden of disease indicator.")]
        [DisplayName("Bod indicator")]
        public string BodIndicator { get; set; }

        [Description("The code of the exposure response function.")]
        [DisplayName("EEF Code")]
        public string ExposureResponseFunctionCode { get; set; }

        [Description("Exposure bin.")]
        [DisplayName("Exposure bin")]
        public string ExposureBin { get; set; }

        [Description("Exposure.")]
        [DisplayName("Exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("The target unit of the exposure.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Percentile specific odds ratio.")]
        [DisplayName("Percentile specific {EffectMetric}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Ratio { get; set; }

        [Description("Percentile specific attributable fraction.")]
        [DisplayName("Percentile specific AF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AttributableFraction { get; set; }

        [Description("Total burden of disease.")]
        [DisplayName("Total BoD")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double TotalBod { get; set; }

        [Description("Burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double AttributableBod { get; set; }
    }
}
