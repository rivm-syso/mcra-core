using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundRPFDataRecord {

        private bool _isUncertain = false;

        public CompoundRPFDataRecord() {
            LimitDose = double.NaN;
            SystemHazardDose = double.NaN;
            SystemToHumanConversionFactor = double.NaN;
        }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Hazard dose expressed for the original system from which it was derived (e.g., animals)")]
        [DisplayName("Hazard dose original system (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double SystemHazardDose { get; set; }

        [Description("Nominal conversion factor to convert the original system hazard dose to human hazard dose")]
        [DisplayName("System to human conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double SystemToHumanConversionFactor { get; set; }

        [Description("Hazard dose expressed for humans")]
        [DisplayName("Hazard dose human (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LimitDose { get; set; }

        [Description("Relative Potency Factor as recorded in input data")]
        [DisplayName("RPF (nominal)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RPF { get; set; } = double.NaN;

        [Description("Relative Potency Factor (RPF) scaled with respect to the RPF of the reference compound")]
        [DisplayName("RPF (nominal) scaled to reference")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RescaledRPF { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsUncertain {
            get { return _isUncertain; }
            set { _isUncertain = value; }
        }

        [DisplayName("RPF (mean) scaled to reference")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RPFMean { get; set; }

        [DisplayName("RPF (median) scaled to reference")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RPFMedian { get; set; }

        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double Percentile25 { get; set; }

        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double Percentile75 { get; set; }

        [DisplayName("RPF p25-p75 scaled to reference")]
        public string Range {
            get {
                if (!double.IsNaN(Percentile25)) {
                    return $"({Percentile25:G3} - {Percentile75:G3})";
                } else {
                    return "-";
                }
            }
        }

        [DisplayName("Origin")]
        public PotencyOrigin PotencyOrigin { get; set; }
    }
}
