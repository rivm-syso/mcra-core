﻿using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HazardCharacterisationsSummaryRecordBase {

        [DisplayName("Code")]
        public string ModelCode { get; set; }

        [Description("Effect name.")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("Effect code.")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("The route of exposure.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Hazard characterisation expressed for the target of the assessment.")]
        [DisplayName("HC")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double HazardCharacterisation { get; set; }

        [Description("The target unit of the hazard dose values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Hazard characterisation uncertainty lower bound (LowerBound) of median.")]
        [DisplayName("HC Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double TargetDoseLowerBoundPercentile {
            get {
                if (TargetDoseUncertaintyValues?.Count > 0) {
                    return TargetDoseUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Hazard characterisation uncertainty upper bound (UpperBound) of median.")]
        [DisplayName("HC Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double TargetDoseUpperBoundPercentile {
            get {
                if (TargetDoseUncertaintyValues?.Count > 0) {
                    return TargetDoseUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Display(AutoGenerateField = false)]
        public List<double> TargetDoseUncertaintyValues { get; set; }

        [Description("Geometric standard deviation of the variability distribution of the hazard characterisation.")]
        [DisplayName("GSD")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double GeometricStandardDeviation { get; set; } = double.NaN;

        [Description("Hazard characterisation lower bound (p2.5) of variability distribution.")]
        [DisplayName("HC (p2.5) ")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double TargetDoseLowerBound { get; set; } = double.NaN;

        [Description("Hazard characterisation upper bound (p97.5) of variability distribution.")]
        [DisplayName("HC (p97.5) ")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double TargetDoseUpperBound { get; set; } = double.NaN;

        [Display(AutoGenerateField = false)]
        public List<double> TargetDoseLowerBoundUncertaintyValues { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> TargetDoseUpperBoundUncertaintyValues { get; set; }

        [Description("Hazard characterisation lower uncertainty bound (LowerBound) of p2.5.")]
        [DisplayName("HC (p2.5) - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double TargetDoseLowerBoundPercentileUnc {
            get {
                if (TargetDoseLowerBoundUncertaintyValues?.Count > 0) {
                    return TargetDoseLowerBoundUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Hazard characterisation upper uncertainty bound (UpperBound) of p97.5.")]
        [DisplayName("HC (p97.5) - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double TargetDoseUpperBoundPercentileUnc {
            get {
                if (TargetDoseUpperBoundUncertaintyValues?.Count > 0) {
                    return TargetDoseUpperBoundUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("Type of the origin or source from which the hazard characterisation was derived.")]
        [DisplayName("Origin")]
        public virtual string PotencyOrigin { get; set; }

    }
}
