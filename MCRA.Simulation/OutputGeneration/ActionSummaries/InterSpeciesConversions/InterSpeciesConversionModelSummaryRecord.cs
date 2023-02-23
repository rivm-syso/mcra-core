using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InterSpeciesConversionModelSummaryRecord {
        [Description("effect")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("effect")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("compound")]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("compound")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("Species")]
        [DisplayName("Species")]
        public string Species { get; set; }

        [Description("inter species factor, is used for interpolation between the critical effect dose from animal to humans")]
        [DisplayName("Inter-species geometric mean")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double GeometricMean { get; set; }

        [Description("specifies the uncertainty in the intersystem factor (geometric mean)")]
        [DisplayName("Inter-system geometric standard deviation")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double GeometricStDev { get; set; }

        [Description("standard human weight")]
        [DisplayName("Human weight")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardHumanBodyWeight { get; set; }

        [Description("human weight unit")]
        [DisplayName("Human unit")]
        public string HumanBodyWeightUnit { get; set; }

        [Description("standard animal weight")]
        [DisplayName("Animal weight")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardAnimalBodyWeight { get; set; }

        [Description("animal weight unit")]
        [DisplayName("Animal unit")]
        public string AnimalBodyWeightUnit { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> InterSpeciesFactorUncertaintyValues { get; set; }

        [Description("Inter species factor lower bound (2.5 percentile).")]
        [DisplayName("Inter species factor lower bound (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double InterSpeciesFactorLowerBoundPercentile {
            get {
                if (InterSpeciesFactorUncertaintyValues.Count > 1) {
                    return InterSpeciesFactorUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Inter species factor upper bound (97.5 percentile).")]
        [DisplayName("Inter species factor upper bound (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double InterSpeciesFactorUpperBoundPercentile {
            get {
                if (InterSpeciesFactorUncertaintyValues.Count > 1) {
                    return InterSpeciesFactorUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

    }
}
