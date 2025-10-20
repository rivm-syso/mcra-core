using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class AttributableBodSummaryRecord {

        [Display(AutoGenerateField = false)]
        public double PopulationSize { get; set; }

        [Display(AutoGenerateField = false)]
        public double StandardisedPopulationSize { get; set; }

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

        [Description("The code of the substance.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The name of the substance.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("The code of the effect.")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("The name of the effect.")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("Burden of disease indicator.")]
        [DisplayName("Bod indicator")]
        [Display(AutoGenerateField = false)]
        public string BodIndicator { get; set; }

        [Description("Intermediate/source indicators from which this BoD was derived (using BoD indicator conversions).")]
        [DisplayName("Source indicator(s)")]
        public string SourceIndicators {
            get {
                if (SourceIndicatorList?.Count > 0) {
                    return string.Join(", ", SourceIndicatorList);
                } else {
                    return string.Empty;
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public List<string> SourceIndicatorList { get; set; }

        [Description("The code of the exposure response function.")]
        [DisplayName("ERF Code")]
        public string ErfCode { get; set; }

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

        [Description("Exposure level considered for the bin in the EBD calculations.")]
        [DisplayName("Exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Exposures { get; set; }

        [Description("Uncertainty median bin exposure level.")]
        [DisplayName("Exposure median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianExposure { get { return Exposures.Count != 0 ? Exposures.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound bin exposure level.")]
        [DisplayName("Exposure lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerBoundExposure { get { return Exposures.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound bin exposure level.")]
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

        [Display(AutoGenerateField = false)]
        public List<double> ResponseValues { get; set; }

        [Description("Median percentile specific response value ({EffectMetric}).")]
        [DisplayName("Percentile specific {EffectMetric} median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianResponseValue { get { return ResponseValues.Count != 0 ? ResponseValues.Percentile(50) : double.NaN; } }

        [Description("Percentile specific attributable fraction.")]
        [DisplayName("Percentile specific AF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AttributableFraction { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AttributableFractions { get; set; }

        [Description("Median percentile specific attributable fraction.")]
        [DisplayName("Percentile specific AF median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAttributableFraction { get { return AttributableFractions.Count != 0 ? AttributableFractions.Percentile(50) : double.NaN; } }

        [Description("Total burden of disease ({BodIndicator}).")]
        [DisplayName("Total BoD ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double TotalBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> TotalBods { get; set; }

        [Description("Median total burden of disease ({BodIndicator}).")]
        [DisplayName("Total BoD median ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianTotalBod { get { return TotalBods.Count != 0 ? TotalBods.Percentile(50) : double.NaN; } }

        [Description("Burden of disease attributable to part of population identified by exposure bin ({BodIndicator}).")]
        [DisplayName("Attributable BoD ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AttributableBods { get; set; }

        [Description("Median burden of disease attributable to part of population identified by exposure bin ({BodIndicator}).")]
        [DisplayName("Attributable BoD median ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAttributableBod { get { return AttributableBods.Count != 0 ? AttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound burden of disease attributable to part of population ({BodIndicator}).")]
        [DisplayName("Attributable BoD lower bound (LowerBound) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound  burden of disease attributable to part of population ({BodIndicator}).")]
        [DisplayName("Attributable BoD upper bound (UpperBound) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Cumulative percentage burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double CumulativeAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeAttributableBods { get; set; }

        [Description("Median cumulative burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD median (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeAttributableBod { get { return CumulativeAttributableBods.Count != 0 ? CumulativeAttributableBods.Percentile(50) : double.NaN; } }

        [Display(AutoGenerateField = false)]
        [Description("Lower uncertainty bound cumulative burden of disease attributable to part of population.")]
        [DisplayName("Cumulative attributable BoD lower bound (LowerBound) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerCumulativeAttributableBod { get { return CumulativeAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Display(AutoGenerateField = false)]
        [Description("Upper uncertainty bound cumulative  burden of disease attributable to part of population.")]
        [DisplayName("Cumulative attributable BoD upper bound (UpperBound) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperCumulativeAttributableBod { get { return CumulativeAttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Standardised burden of disease attributable to part of population identified by exposure bin, {EbdStandardisedPopulationSize} ({BodIndicator}).")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardisedAttributableBod { get { return AttributableBod / PopulationSize * StandardisedPopulationSize; } }

        [Description("Median standardised burden of disease attributable to part of population identified by exposure bin, {EbdStandardisedPopulationSize} ({BodIndicator}).")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) median ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianStandardisedAttributableBod { get { return AttributableBods.Count != 0 ? AttributableBods.Percentile(50) / PopulationSize * StandardisedPopulationSize : double.NaN; } }

        [Description("Lower uncertainty bound standardised burden of disease attributable to part of population, {EbdStandardisedPopulationSize} ({BodIndicator}).")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) lower bound (LowerBound) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerStandardisedAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound) / PopulationSize * StandardisedPopulationSize; } }

        [Description("Upper uncertainty bound standardised burden of disease attributable to part of population, {EbdStandardisedPopulationSize} ({BodIndicator}).")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) upper bound (UpperBound) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperStandardisedAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound) / PopulationSize * StandardisedPopulationSize; } }

        [Description("Standardised exposed burden of disease attributable to part of population identified by exposure bin, {EbdStandardisedPopulationSize} exposed ({BodIndicator}).")]
        [DisplayName("Standardised exposed attr. BoD ({EbdStandardisedPopulationSize}) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StandardisedExposedAttributableBod { get { return AttributableBod / PopulationSize / BinPercentage * StandardisedPopulationSize * 100; } }

        [Display(AutoGenerateField = false)]
        public List<double> StandardisedExposedAttributableBods { get; set; }

        [Description("Median standardised exposed burden of disease attributable to part of population identified by exposure bin, {EbdStandardisedPopulationSize} exposed ({BodIndicator}).")]
        [DisplayName("Standardised exposed attr. BoD ({EbdStandardisedPopulationSize}) median ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianStandardisedExposedAttributableBod { get { return StandardisedExposedAttributableBods.Count != 0 ? StandardisedExposedAttributableBods.Percentile(50) / PopulationSize * StandardisedPopulationSize : double.NaN; } }

        [Description("Lower uncertainty bound standardised exposed burden of disease attributable to part of population, {EbdStandardisedPopulationSize} exposed ({BodIndicator}).")]
        [DisplayName("Standardised exposed attr. BoD ({EbdStandardisedPopulationSize}) lower bound (LowerBound) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerStandardisedExposedAttributableBod { get { return StandardisedExposedAttributableBods.Percentile(UncertaintyLowerBound) / PopulationSize * StandardisedPopulationSize * 100; } }

        [Description("Upper uncertainty bound standardised exposed burden of disease attributable to part of population, {EbdStandardisedPopulationSize} exposed ({BodIndicator}).")]
        [DisplayName("Standardised exposed attr. BoD ({EbdStandardisedPopulationSize}) upper bound (UpperBound) ({BodIndicator})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperStandardisedExposedAttributableBod { get { return StandardisedExposedAttributableBods.Percentile(UncertaintyUpperBound) / PopulationSize * StandardisedPopulationSize * 100; } }

        [Description("Cumulative percentage standardised exposed burden of disease attributable to part of population identified by exposure bin, {EbdStandardisedPopulationSize} exposed.")]
        [DisplayName("Cumulative standardised exposed attr. BoD ({EbdStandardisedPopulationSize}) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double CumulativeStandardisedExposedAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeStandardisedExposedAttributableBods { get; set; }

        [Description("Median cumulative standardised exposed burden of disease attributable to part of population identified by exposure bin, {EbdStandardisedPopulationSize} exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD median (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeStandardisedExposedAttributableBod { get { return CumulativeStandardisedExposedAttributableBods.Count != 0 ? CumulativeStandardisedExposedAttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound cumulative standardised exposed burden of disease attributable to part of population, {EbdStandardisedPopulationSize} exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD lower bound (LowerBound) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerCumulativeStandardisedExposedAttributableBod { get { return CumulativeStandardisedExposedAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound cumulative standardised exposed burden of disease attributable to part of population, {EbdStandardisedPopulationSize} exposed.")]
        [DisplayName("Cumulative standardised exposed attributable BoD upper bound (UpperBound) (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperCumulativeStandardisedExposedAttributableBod { get { return CumulativeStandardisedExposedAttributableBods.Percentile(UncertaintyUpperBound); } }

        public string GetGroupKey() {
            if (SourceIndicatorList?.Count > 0) {
                var sourceKey = string.Join("_", SourceIndicatorList);
                return $"{PopulationCode}_{BodIndicator}_{sourceKey}_{ErfCode}";
            } else {
                return $"{PopulationCode}_{BodIndicator}_{ErfCode}";
            }
        }

        public string GetGroupDisplayName() {
            if (SourceIndicatorList?.Count > 0) {
                return $"{EffectName} {SubstanceName} {PopulationName} ({BodIndicator} from {SourceIndicators})";
            } else {
                return $"{EffectName} {SubstanceName} {PopulationName} ({BodIndicator})";
            }
        }
    }
}
