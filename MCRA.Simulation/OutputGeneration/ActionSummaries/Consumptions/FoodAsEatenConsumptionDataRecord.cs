using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class FoodAsEatenConsumptionDataRecord : HierarchyRecord {

        public FoodAsEatenConsumptionDataRecord() {
            __IsSummaryRecord = false;
            MeanConsumptionAll = double.NaN;
            MeanConsumption = double.NaN;
            NConsumptionDays = 0;
            NIndividualDays = 0;
            TotalSamplingWeightsConsumptionDays = 0;
            TotalSamplingWeightsAllDays = 0;
            BrandLoyalty = double.NaN;
        }

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Base food name")]
        [Description("Base (raw/unprocessed) food name.")]
        public string BaseFoodName { get; set; }

        [DisplayName("Base food code")]
        [Description("Base (raw/unprocessed) food code.")]
        public string BaseFoodCode { get; set; }

        [DisplayName("Treatment code(s)")]
        [Description("Code(s) of the treatments/facets of the consumed food.")]
        public string TreatmentCodes { get; set; }

        [DisplayName("Treatment name(s)")]
        [Description("Name(s) of the treatments/facets of the consumed food.")]
        public string TreatmentNames { get; set; }

        [Description("For person-days only: total sum of consumption amount * individual sampling weight / total sum of individual sampling weights.")]
        [DisplayName("Mean consumption all days (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConsumptionAll { get; set; }

        [Description("p50 percentile of all consumption values.")]
        [DisplayName("Median for all days (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianConsumptionAll { get; set; }

        [Description("Percentile point of all consumption values (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} for all days (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25ConsumptionAll { get; set; }

        [Description("Percentile point of all consumption values (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} for all days (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75ConsumptionAll { get; set; }

        [Description("For user-days only: total sum of consumption amount * individual sampling weight / total sum of individual sampling weights.")]
        [DisplayName("Mean consumption days (> 0) (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConsumption { get; set; }

        [Description("For user-days only: p50 percentile of consumption values.")]
        [DisplayName("Median user-days only (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianConsumption { get; set; }

        [Description("For user-days only: percentile point of consumption values (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} user-days only (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25Consumption { get; set; }

        [Description("For user-days only: percentile point of consumption values (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} user-days only (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75Consumption { get; set; }

        [Description("Total number of user-days (= days with one or more consumptions).")]
        [DisplayName("Consumption days (> 0) (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NConsumptionDays { get; set; }

        [Description("Total number of person-days (= all survey days).")]
        [DisplayName("Total consumption days (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(AutoGenerateField = false)]
        public int NIndividualDays { get; set; }

        [Description("Percentage user-days (= days with one or more consumptions).")]
        [DisplayName("Percentage consumption days (> 0) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageOfConsumptionDays {
            get { return (double)NConsumptionDays / (double)NIndividualDays * 100; }
        }

        [Description("Total sum of sampling weights on user-days (= days with one or more consumptions).")]
        [DisplayName("Total weights consumption days (> 0)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeightsConsumptionDays { get; set; }

        [Description("Total sum of sampling weights on person-days (on all survey days).")]
        [DisplayName("Total weights all days")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        [Display(AutoGenerateField = false)]
        public double TotalSamplingWeightsAllDays { get; set; }

        [Description("Percentage of total sum of sampling weights on user-days.")]
        [DisplayName("Percentage total weights consumption days (> 0) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageTotalSamplingWeightsAllDays {
            get { return TotalSamplingWeightsConsumptionDays / TotalSamplingWeightsAllDays * 100; }
        }

        [Description("Brand loyalty scaled from L = 0 (no brandloyalty) to L = 1 (absolute brandloyalty).")]
        [DisplayName("Brand loyalty")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double BrandLoyalty { get; set; }
    }
}
