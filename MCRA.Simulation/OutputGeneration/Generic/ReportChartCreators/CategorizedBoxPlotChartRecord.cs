using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CategorizedBoxPlotChartRecord : BoxPlotChartRecord {

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

        [Display(AutoGenerateField = false)]
        public string StratificationLevelCode { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> StratificationSubLevelCodes { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> StratificationSubLevelNames { get; set; }
    }
}
