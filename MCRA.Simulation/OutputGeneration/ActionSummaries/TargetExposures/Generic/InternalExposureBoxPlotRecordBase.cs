using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class InternalExposureBoxPlotRecordBase<T> : CategorizedBoxPlotChartRecord
        where T : IContributorKey, new() {

        public abstract string GetKey();

        public abstract void SetDescriptorValues(T descriptor);

    }
}
