using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class AttributableBodSummaryRecord {
        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [Display(AutoGenerateField = false)]
        public int ExposureBinId { get; set; }

        [Description("Population.")]
        [DisplayName("Population")]
        public string Population { get; set; }

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
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double AttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AttributableBods { get; set; }

        [Description("Median Burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianAttributableBod { get { return AttributableBods.Any() ? AttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound burden of disease attributable to part of population.")]
        [DisplayName("Attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerAttributableBod { get { return AttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound  burden of disease attributable to part of population.")]
        [DisplayName("Attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperAttributableBod { get { return AttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Cumulative Burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double CumulativeAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> CumulativeAttributableBods { get; set; }

        [Description("Median cumulative Burden of disease attributable to part of population identified by exposure bin.")]
        [DisplayName("Cumulative attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MedianCumulativeAttributableBod { get { return CumulativeAttributableBods.Any() ? CumulativeAttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound cumulative burden of disease attributable to part of population.")]
        [DisplayName("Cumulative attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerCumulativeAttributableBod { get { return CumulativeAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound cumulative  burden of disease attributable to part of population.")]
        [DisplayName("Cumulative attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperCumulativeAttributableBod { get { return CumulativeAttributableBods.Percentile(UncertaintyUpperBound); } }
    }
}
