using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseModelSubstanceSummaryRecord {

        [Description("Experiment code")]
        [DisplayName("Experiment code")]
        public string CodeExperiment { get; set; }

        [Description("Response code")]
        [DisplayName("Response code")]
        public string CodeResponse { get; set; }

        [Description("Response code")]
        [DisplayName("Response code")]
        public string CodeCompound { get; set; }

        [Description("Covariate level")]
        [DisplayName("Covariate level")]
        public string CovariateLevel { get; set; }

        [Description("Benchmark dose")]
        [DisplayName("BMD")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDose { get; set; } = double.NaN;

        [Description("Benchmark dose lower")]
        [DisplayName("BMDL")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDoseLower { get; set; } = double.NaN;

        [Description("Benchmark dose upper")]
        [DisplayName("BMDU")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkDoseUpper { get; set; } = double.NaN;

        [Display(AutoGenerateField = false)]
        public bool IsTested { get; set; }

        [Display(AutoGenerateField = false)]
        public bool Converged { get; set; }

    }
}
