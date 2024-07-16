using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DustExposuresDataRecord {

        [DisplayName("Dust survey")]
        public string NonDietarySurvey { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Total individuals")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividuals { get; set; }

        [DisplayName("Mean dermal dust exposure (DustExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanDermal { get; set; }

        [DisplayName("Mean oral dust exposure (DustExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanOral { get; set; }

        [DisplayName("Mean inhalation dust exposure (DustExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanInhalation { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> DustDermalUncertaintyValues { get; set; }

        [Description("uncertainty lower bound dermal (2.5 percentile).")]
        [DisplayName("uncertainty lower bound dermal (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustDermalLowerBoundPercentile {
            get {
                if (DustDermalUncertaintyValues.Count > 1) {
                    return DustDermalUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty upper bound dermal (97.5 percentile).")]
        [DisplayName("uncertainty upper bound dermal (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustDermalUpperBoundPercentile {
            get {
                if (DustDermalUncertaintyValues.Count > 1) {
                    return DustDermalUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty mean value dermal.")]
        [DisplayName("uncertainty mean value dermal")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustDermalMeanUncertaintyValue {
            get {
                if (DustDermalUncertaintyValues.Count > 1) {
                    return DustDermalUncertaintyValues.Average();
                }
                return double.NaN;
            }
        }

        [Display(AutoGenerateField = false)]
        public List<double> DustInhalationUncertaintyValues { get; set; }

        [Description("uncertainty lower bound inhalation (2.5 percentile).")]
        [DisplayName("uncertainty lower bound inhalation (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustInhalationLowerBoundPercentile {
            get {
                if (DustInhalationUncertaintyValues.Count > 1) {
                    return DustInhalationUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty upper bound inhalation (97.5 percentile).")]
        [DisplayName("uncertainty upper bound inhalation (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustInhalationUpperBoundPercentile {
            get {
                if (DustInhalationUncertaintyValues.Count > 1) {
                    return DustInhalationUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("uncertainty mean value inhalation.")]
        [DisplayName("uncertainty mean value inhalation")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DustInhalationMeanUncertaintyValue {
            get {
                if (DustInhalationUncertaintyValues.Count > 1) {
                    return DustInhalationUncertaintyValues.Average();
                }
                return double.NaN;
            }
        }
    }
}