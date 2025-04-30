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

        [Display(AutoGenerateField = false)]
        public List<double> BinPercentages { get; set; }

        [Description("Median percentage of population.")]
        [DisplayName("Percentage of population median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianBinPercentage { get { return BinPercentages.Count != 0 ? BinPercentages.Percentile(50) : double.NaN; } }

        [Description("Exposure.")]
        [DisplayName("Exposure")]
        public double Exposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Exposures { get; set; }

        [Description("Median exposure.")]
        [DisplayName("Exposure median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianExposure { get { return Exposures.Count != 0 ? Exposures.Percentile(50) : double.NaN; } }

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

        [Display(AutoGenerateField = false)]
        public List<double> TotalBods { get; set; }

        [Description("Median total burden of disease.")]
        [DisplayName("Total BoD median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianTotalBod { get { return TotalBods.Count != 0 ? TotalBods.Percentile(50) : double.NaN; } }

        [Description("Burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AttributableBods { get; set; }

        [Description("Median burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAttributableBod { get { return AttributableBods.Count != 0 ? AttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound burden of disease attributable to part of population.")]
        [DisplayName("Attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound  burden of disease attributable to part of population.")]
        [DisplayName("Attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Cumulative percentage burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double CumulativeAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeAttributableBods { get; set; }

        [Description("Median cumulative burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD median (%")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeAttributableBod { get { return CumulativeAttributableBods.Count != 0 ? CumulativeAttributableBods.Percentile(50) : double.NaN; } }

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

        [Description("Standardised burden of disease attributable to part of population identified by exposure bin per 100.000.")]
        [DisplayName("Standardised attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardisedAttributableBod { get { return AttributableBod / PopulationSize * 1e5; } }

        [Description("Median standardised burden of disease attributable to part of population identified by exposure bin per 100.000.")]
        [DisplayName("Standardised attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianStandardisedAttributableBod { get { return AttributableBods.Count != 0 ? AttributableBods.Percentile(50) / PopulationSize * 1e5 : double.NaN; } }

        [Description("Lower uncertainty bound standardised burden of disease attributable to part of population per 100.000.")]
        [DisplayName("Standardised attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerStandardisedAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound) / PopulationSize * 1e5; } }

        [Description("Upper uncertainty bound standardised burden of disease attributable to part of population per 100.000.")]
        [DisplayName("Standardised attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperStandardisedAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound) / PopulationSize * 1e5; } }

        [Description("Standardised exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Standardised exposed attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardisedExposedAttributableBod { get { return AttributableBod / PopulationSize / BinPercentage * 1e7; } }

        [Display(AutoGenerateField = false)]
        public List<double> StandardisedExposedAttributableBods { get; set; }

        [Description("Median standardised exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Standardised exposed attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianStandardisedExposedAttributableBod { get { return StandardisedExposedAttributableBods.Count != 0 ? StandardisedExposedAttributableBods.Percentile(50) / PopulationSize * 1e7 : double.NaN; } }
        
        [Description("Lower uncertainty bound standardised exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Standardised exposed attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerStandardisedExposedAttributableBod { get { return StandardisedExposedAttributableBods.Percentile(UncertaintyLowerBound) / PopulationSize * 1e7; } }
        
        [Description("Upper uncertainty bound standardised exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Standardised exposed attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperStandardisedExposedAttributableBod { get { return StandardisedExposedAttributableBods.Percentile(UncertaintyUpperBound) / PopulationSize * 1e7; } }

        [Description("Cumulative percentage standardised exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double CumulativeStandardisedExposedAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeStandardisedExposedAttributableBods { get; set; }

        [Description("Median cumulative standardised exposed burden of disease attributable to part of population identified by exposure bin per 100.000 exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD median (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeStandardisedExposedAttributableBod { get { return CumulativeStandardisedExposedAttributableBods.Count != 0 ? CumulativeStandardisedExposedAttributableBods.Percentile(50) : double.NaN; } }
       
        [Description("Lower uncertainty bound cumulative standardised exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerCumulativeStandardisedExposedAttributableBod { get { return CumulativeStandardisedExposedAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound cumulative standardised exposed burden of disease attributable to part of population per 100.000 exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperCumulativeStandardisedExposedAttributableBod { get { return CumulativeStandardisedExposedAttributableBods.Percentile(UncertaintyUpperBound); } }

    }
}
