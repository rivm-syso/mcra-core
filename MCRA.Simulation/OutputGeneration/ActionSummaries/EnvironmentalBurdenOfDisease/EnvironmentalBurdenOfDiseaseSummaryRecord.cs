using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MathNet.Numerics.Statistics;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class EnvironmentalBurdenOfDiseaseSummaryRecord {

        [Display(AutoGenerateField = false)]
        public double PopulationSize { get; set; }

        [Display(AutoGenerateField = false)]
        public double StandardisedPopulationSize { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

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
        [DisplayName("BoD indicator")]
        public string BodIndicator { get; set; }

        [Description("Intermediate/source indicators from which this BoD was derived (using BoD indicator conversions).")]
        [DisplayName("Source indicator(s)")]
        public string SourceIndicators { get; set; }

        [Description("The code of the exposure response function.")]
        [DisplayName("ERF code")]
        public string ErfCode { get; set; }

        [Description("The name of the exposure response function.")]
        [DisplayName("ERF name")]
        public string ErfName { get; set; }

        [Description("Total attributable burden of disease for the whole population.")]
        [DisplayName("Total attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double TotalAttributableBod { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> TotalAttributableBods { get; set; }

        [Description("Median total attributable burden of disease for the whole population.")]
        [DisplayName("Total attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianTotalAttributableBod { get { return TotalAttributableBods.Any() ? TotalAttributableBods.Percentile(50) : double.NaN; } }

        [Description("Lower uncertainty bound total attributable burden of disease for the whole population.")]
        [DisplayName("Attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerTotalAttributableBod { get { return TotalAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound total attributable burden of disease for the whole population.")]
        [DisplayName("Attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperTotalAttributableBod { get { return TotalAttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Total population attributable fraction (%) (PAF).")]
        [DisplayName("Total population attributable fraction (%)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TotalPopulationAttributableFraction { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> TotalPopulationAttributableFractions { get; set; }

        [Description("Median total population attributable fraction (%) (PAF).")]
        [DisplayName("Total population attributable fraction (%) median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianTotalPopulationAttributableFraction { get { return TotalPopulationAttributableFractions.Percentile(50); } }

        [Description("Lower uncertainty bound total population attributable fraction (%) (PAF).")]
        [DisplayName("Population attributable fraction (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerTotalPopulationAttributableFraction { get { return TotalPopulationAttributableFractions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound total population attributable fraction (%) (PAF).")]
        [DisplayName("Population attributable fraction (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperTotalPopulationAttributableFraction { get { return TotalPopulationAttributableFractions.Percentile(UncertaintyUpperBound); } }

        [Description("Standardised total attributable burden of disease {EbdStandardisedPopulationSize}.")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize})")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double StandardisedTotalAttributableBod { get { return TotalAttributableBod / PopulationSize * StandardisedPopulationSize; } }

        [Description("Standardised median attributable burden of disease {EbdStandardisedPopulationSize}.")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianStandardisedTotalAttributableBod { get { return MedianTotalAttributableBod / PopulationSize * StandardisedPopulationSize; } }

        [Description("Standardised attributable burden of disease lower uncertainty bound {EbdStandardisedPopulationSize}.")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerStandardisedTotalAttributableBod { get { return LowerTotalAttributableBod / PopulationSize * StandardisedPopulationSize; } }

        [Description("Standardised attributable burden of disease upper uncertainty bound {EbdStandardisedPopulationSize}.")]
        [DisplayName("Standardised attr. BoD ({EbdStandardisedPopulationSize}) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperStandardisedTotalAttributableBod { get { return UpperTotalAttributableBod / PopulationSize * StandardisedPopulationSize; } }
    }
}
