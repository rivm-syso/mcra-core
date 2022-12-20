using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SamplesByFoodSubstanceRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Measured substance name")]
        public string MeasuredCompoundName { get; set; }

        [DisplayName("Measured substance code")]
        public string MeasuredCompoundCode { get; set; }

        [Description("States whether these concentrations were extrapolated.")]
        [DisplayName("Extrapolated (y/n)")]
        public bool Extrapolated { get; set; }

        [Description("States whether the measurements of this food/substance pair are part of the focal commodity scenario analysis.")]
        [DisplayName("Focal combination (y/n)")]
        public bool FocalCombination { get; set; }

        [Description("Total number of samples of the food present in the occurrence data.")]
        [DisplayName("Total food samples (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalCount { get; set; }

        [Description("Total number of samples in which the substance occurrence was analysed in the food.")]
        [DisplayName("Samples analysed for substance (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AnalyticalScopeCount {
            get {
                return ZerosCount + PositivesCount + CensoredValuesCount;
            }
        }

        [Description("Total number of positive measurements.")]
        [DisplayName("Positive measurements (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PositivesCount { get; set; }

        [Description("Total number of zeros.")]
        [DisplayName("Zeros (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ZerosCount { get; set; }

        [Description("Total number of censored values.")]
        [DisplayName("Censored values (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CensoredValuesCount { get; set; }

        [Description("Total number of missing values. Missing values for a substance occur when the substance is not analyzed by the analytical method.")]
        [DisplayName("Missing values (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int MissingValuesCount { get; set; }

        [Description("Percentage of detects.")]
        [DisplayName("Percentage positive samples (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositiveSample {
            get {
                return PositivesCount / (double)TotalCount * 100;
            }
        }

        [Description("Mean concentration of all residue values (including censored values set to zero).")]
        [DisplayName("Mean concentration all residues (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MeanAllResidues { get; set; }

        [Description("Mean concentration of all positive and zero concentration residue values (detects only).")]
        [DisplayName("Mean concentration positives and zeros (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MeanPositiveResidues { get; set; }

        [Description("Mean of the LORs of the censored values.")]
        [DisplayName("Mean LORs (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MeanLORs { get; set; }

        [Description("Mean of the LODs of the non-detects.")]
        [DisplayName("Mean LODs (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MeanLODs { get; set; }

        [Description("Mean of the LOQs of non-quantifications.")]
        [DisplayName("Mean LOQs (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MeanLOQs { get; set; }

        [Description("Lower {LowerPercentage} percentile point of positive residue values (detects only).")]
        [DisplayName("{LowerPercentage} (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("Upper {UpperPercentage} percentile point of positive residue values (detects only).")]
        [DisplayName("{UpperPercentage} (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }

        [Description("Minimum of the positive residue values.")]
        [DisplayName("Min. (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Minimum { get; set; }

        [Description("Maximum of the positive residue values.")]
        [DisplayName("Max. (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Maximum { get; set; }

    }
}
