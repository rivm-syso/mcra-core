using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public class ModelledFoodConsumptionDataRecord : HierarchyRecord {

        [Display(Name = "Food name", Order = 1)]
        public string FoodName { get; set; }

        [Display(Name = "Food code", Order = 2)]
        public string FoodCode { get; set; }

        [Description("For person-days only: total sum of consumption amount * individual sampling weight / total sum of individual sampling weights")]
        [Display(Name = "Mean consumption all days (ConsumptionUnit)", Order = 10)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConsumptionAll { get; set; }

        [Description("p50 percentile of all consumption values.")]
        [Display(Name = "Median for all days (ConsumptionUnit)", Order = 11)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianConsumptionAll { get; set; }

        [Description("Percentile point of all consumption values (default 25%, see Output settings).")]
        [Display(Name = "{LowerPercentage} for all days (ConsumptionUnit)", Order = 12)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25ConsumptionAll { get; set; }

        [Description("Percentile point of all consumption values (default 75%, see Output settings).")]
        [Display(Name = "{UpperPercentage} for all days (ConsumptionUnit)", Order = 13)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75ConsumptionAll { get; set; }

        [Description("p95 percentile of consumption on person-days (= all survey days)")]
        [Display(Name = "p95 all days (ConsumptionUnit)", Order = 14)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile95ConsumptionAll { get; set; }

        [Description("For user-days only: total sum of consumption amount * individual sampling weight / total sum of individual sampling weights")]
        [Display(Name = "Mean consumption days (> 0) (ConsumptionUnit)", Order = 15)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConsumption { get; set; }

        [Description("For user-days only: p50 percentile of consumption values.")]
        [Display(Name = "Median user-days only (ConsumptionUnit)", Order = 16)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianConsumption { get; set; }

        [Description("For user-days only: percentile point of consumption values (default 25%, see Output settings).")]
        [Display(Name = "{LowerPercentage} user-days only (ConsumptionUnit)", Order = 17)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25Consumption { get; set; }

        [Description("For user-days only: percentile point of consumption values (default 75%, see Output settings).")]
        [Display(Name = "{UpperPercentage} user-days only (ConsumptionUnit)", Order = 18)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75Consumption { get; set; }

        [Description("For user-days only: p95 percentile of consumption values")]
        [Display(Name = "p95 user-days only (ConsumptionUnit)", Order = 19)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile95Consumption { get; set; }

        [Description("Total number of user-days (= days with one or more consumption)")]
        [Display(Name = "Consumption days (> 0) (N)", Order = 20)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfConsumptionDays { get; set; }

        [Description("Percentage user-days")]
        [Display(Name = "Percentage consumption days (> 0) (%)", Order = 21)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageOfConsumptionDays {
            get { return (double)NumberOfConsumptionDays / (double)TotalNumberOfIndividualDays * 100; }
        }

        [Description("Total number of person-days (= all survey days)")]
        [Display(Name = "Total consumption days (N)", AutoGenerateField = false, Order = 22)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalNumberOfIndividualDays { get; set; }

        [Description("total sum of sampling weights on user-days (on consumption days)")]
        [Display(Name = "Total weights consumption days (> 0)", Order = 23)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeightsConsumptionDays { get; set; }

        [Description("Total sum of sampling weights on person-days (on all survey days)")]
        [Display(Name = "Total weights all days", AutoGenerateField = false, Order = 24)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeightsAllDays { get; set; }

        [Description("Percentage of total sum of sampling weights on user-days")]
        [Display(Name = "Percentage total weights consumption days (%)", Order = 25)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageTotalSamplingWeightsAllDays {
            get { return TotalSamplingWeightsConsumptionDays / TotalSamplingWeightsAllDays * 100; }
        }
    }
}
