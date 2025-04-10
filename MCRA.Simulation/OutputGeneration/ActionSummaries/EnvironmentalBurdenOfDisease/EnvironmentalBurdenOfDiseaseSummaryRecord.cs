using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MathNet.Numerics.Statistics;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class EnvironmentalBurdenOfDiseaseSummaryRecord {

        [Display(AutoGenerateField = false)]
        public double PopulationSize { get; set; }

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

        [Description("Burden of disease indicator.")]
        [DisplayName("BoD indicator")]
        public string BodIndicator { get; set; }

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

        [Display(AutoGenerateField = false)]
        public List<(int Id, double Bod)> AttributableBodPerBin { get; set; }
        [Display(AutoGenerateField = false)]

        public List<(int Id, List<double> Bods)> AttributableBodPerBinList { get; set; }

        [Description("Median total attributable burden of disease for the whole population.")]
        [DisplayName("Total attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianTotalAttributableBod { get { return AttributableBodPerBinList.Any() ? AttributableBodPerBinList.Sum(c => c.Bods.Percentile(50)) : double.NaN; } }

        [Description("Lower uncertainty bound total attributable burden of disease for the whole population.")]
        [DisplayName("Attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerTotalAttributableBod { get { return TotalAttributableBods.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound total attributable burden of disease for the whole population.")]
        [DisplayName("Attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperTotalAttributableBod { get { return TotalAttributableBods.Percentile(UncertaintyUpperBound); } }

        [Description("Standardized total attributable burden of disease for the whole population (AttrBoD / Population size * 100.000) .")]
        [DisplayName("Standardized total attributable BoD per 100.000")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double StandardizedTotalAttributableBod { get { return TotalAttributableBod / PopulationSize * 1e5; } }

        [Description("Median standardized total attributable burden of disease for the whole population.")]
        [DisplayName("Standardized total attributable BoD median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianStandardizedTotalAttributableBod { get { return MedianTotalAttributableBod / PopulationSize * 1e5; } }

        [Description("Lower uncertainty bound standardized total attributable burden of disease for the whole population.")]
        [DisplayName("Standardized total attributable BoD lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerStandardizedTotalAttributableBod { get { return LowerTotalAttributableBod / PopulationSize * 1e5; } }

        [Description("Upper uncertainty bound standardized total attributable burden of disease for the whole population.")]
        [DisplayName("Standardized total attributable BoD upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperStandardizedTotalAttributableBod { get { return UpperTotalAttributableBod / PopulationSize * 1e5; } }
    }
}
