using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmBoxPlotRecordBase<T> : CategorizedBoxPlotChartRecord
        where T : IContributorKey, new() {

        public abstract string GetDescriptorKey();

        public abstract void SetDescriptorValues(T descriptor);

    }
}
