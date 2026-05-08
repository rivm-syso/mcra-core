using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmBoxPlotRecordBase<T> : BoxPlotChartRecord 
        where T : IExposureContributorKey, new() {

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

        public abstract string GetDescriptorKey();

        public abstract void SetDescriptorValues(T descriptor);

    }
}
