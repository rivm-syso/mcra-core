using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CategorizedBoxPlotChartRecord : BoxPlotChartRecord {

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

    }
}
