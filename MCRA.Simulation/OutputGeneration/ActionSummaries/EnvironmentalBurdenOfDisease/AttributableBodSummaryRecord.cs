using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class AttributableBodSummaryRecord {

        [Display(AutoGenerateField = false)]
        public double PopulationSize { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [Display(AutoGenerateField = false)]
        public int ExposureBinId { get; set; }

        [Description("Identification code of the population.")]
        [DisplayName("Population code")]
        public string PopulationCode { get; set; }

        [Description("Name of the population.")]
        [DisplayName("Population name")]
        public string PopulationName { get; set; }

        [Description("Burden of disease indicator.")]
        [DisplayName("Bod indicator")]
        public string BodIndicator { get; set; }

        [Description("The code of the exposure response function.")]
        [DisplayName("ERF Code")]
        public string ExposureResponseFunctionCode { get; set; }

        [Description("Exposure bin.")]
        [DisplayName("Exposure bin")]
        public string ExposureBin { get; set; }

        [Description("Exposure percentile bin.")]
        [DisplayName("Exposure percentile bin")]
        public string ExposurePercentileBin { get; set; }

        [Description("Percentage of population.")]
        [DisplayName("Percentage of population")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BinPercentage { get; set; }

        [Description("Exposure.")]
        [DisplayName("Exposure")]
        public double Exposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Exposures { get; set; }

        [Description("Median exposure.")]
        [DisplayName("Median exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianExposure { get { return Exposures.Any() ? Exposures.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound exposure.")]
        [DisplayName("Exposure lower bound (LowerBound)")]
        public double LowerBoundExposure { get { return Exposures.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound exposure.")]
        [DisplayName("Exposure upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperBoundExposure { get { return Exposures.Percentile(UncertaintyUpperBound); } }

        [Description("The target unit of the exposure.")]
        [DisplayName("Unit")]
        public string TargetUnit { get; set; }

        [Description("Percentile specific response value ({EffectMetric}).")]
        [DisplayName("Percentile specific {EffectMetric}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ResponseValue { get; set; }

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
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AttributableBods { get; set; }

        [Description("Median burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAttributableBod { get { return AttributableBods.Any() ? AttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound burden of disease attributable to part of population.")]
        [DisplayName("Attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound  burden of disease attributable to part of population.")]
        [DisplayName("Attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Cumulative burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double CumulativeAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeAttributableBods { get; set; }

        [Description("Median cumulative burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeAttributableBod { get { return CumulativeAttributableBods.Any() ? CumulativeAttributableBods.Percentile(50) : double.NaN; } }

        [Display(AutoGenerateField = false)]
        [Description("Lower uncertainty bound cumulative burden of disease attributable to part of population.")]
        [DisplayName("Cumulative attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerCumulativeAttributableBod { get { return CumulativeAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Display(AutoGenerateField = false)]
        [Description("Upper uncertainty bound cumulative  burden of disease attributable to part of population.")]
        [DisplayName("Cumulative attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperCumulativeAttributableBod { get { return CumulativeAttributableBods.Percentile(UncertaintyUpperBound); } }


        [Description("Standardized burden of disease attributable to part of population identified by exposure bin per 100.000.")]
        [DisplayName("Standardized attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardizedAttributableBod { get { return AttributableBod / PopulationSize * 1e5; } }

        [Description("Median standardized burden of disease attributable to part of population identified by exposure bin per 100.000.")]
        [DisplayName("Standardized attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianStandardizedAttributableBod { get { return AttributableBods.Any() ? AttributableBods.Percentile(50) / PopulationSize * 1e5 : double.NaN; } }

        [Description("Lower uncertainty bound standardized burden of disease attributable to part of population per 100.000.")]
        [DisplayName("Standardized attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerStandardizedAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound) / PopulationSize * 1e5; } }

        [Description("Upper uncertainty bound standardized burden of disease attributable to part of population per 100.000.")]
        [DisplayName("Standardized attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperStandardizedAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound) / PopulationSize * 1e5; } }

        [Description("Standardized exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Standardized exposed attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardizedExposedAttributableBod { get { return AttributableBod / PopulationSize / BinPercentage * 1e7; } }

        [Description("Median standardized exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Standardized exposed attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianStandardizedExposedAttributableBod { get { return AttributableBods.Any() ? AttributableBods.Percentile(50) / PopulationSize / BinPercentage * 1e7 : double.NaN; } }

        [Description("Lower uncertainty bound standardized exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Standardized exposed attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerStandardizedExposedAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound) / PopulationSize / BinPercentage * 1e7; } }

        [Description("Upper uncertainty bound standardized exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Standardized exposed attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperStandardizedExposedAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound) / PopulationSize / BinPercentage * 1e7; } }

        [Description("Cumulative standardized exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Cumulative standardized exposed attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:F!}")]
        public double CumulativeStandardizedExposedAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeStandardizedExposedAttributableBods { get; set; }

        [Description("Median cumulative standardized exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Cumulative standardized exposed attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeStandardizedExposedAttributableBod { get { return CumulativeStandardizedExposedAttributableBods.Any() ? CumulativeStandardizedExposedAttributableBods.Percentile(50) : double.NaN; } }
       
        [Display(AutoGenerateField = false)]
        [Description("Lower uncertainty bound cumulative standardized exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Cumulative standardized exposed attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerCumulativeStandardizedExposedAttributableBod { get { return CumulativeStandardizedExposedAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Display(AutoGenerateField = false)]
        [Description("Upper uncertainty bound cumulative standardized exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Cumulative standardized exposed attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperCumulativeStandardizedExposedAttributableBod { get { return CumulativeStandardizedExposedAttributableBods.Percentile(UncertaintyUpperBound); } }


    }
}
