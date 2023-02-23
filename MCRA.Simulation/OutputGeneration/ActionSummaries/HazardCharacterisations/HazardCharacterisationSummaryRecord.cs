using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardCharacterisationSummaryRecord {

        [DisplayName("Code")]
        public string ModelCode { get; set; }

        [Description("Effect name.")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("Effect code.")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Hazard characterisation (IntakeUnit) expressed for the target of the assessment.")]
        [DisplayName("Hazard characterisation (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HazardCharacterisation { get; set; }

        [Description("Geometric standard deviation of the variability distribution of the hazard characterisation.")]
        [DisplayName("GSD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double GeometricStandardDeviation { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> TargetDoseUncertaintyValues { get; set; }

        [Description("Hazard characterisation lower bound (LowerBound).")]
        [DisplayName("HC (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TargetDoseLowerBoundPercentile {
            get {
                if (TargetDoseUncertaintyValues?.Any() ?? false) {
                    return TargetDoseUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Hazard characterisation upper bound (UpperBound).")]
        [DisplayName("HC (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TargetDoseUpperBoundPercentile {
            get {
                if (TargetDoseUncertaintyValues?.Any() ?? false) {
                    return TargetDoseUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("Type of the origin or source from which the hazard characterisation was derived.")]
        [DisplayName("Origin")]
        public virtual string PotencyOrigin { get; set; }

    }
}
