using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureHazardRatioSectionBase<S, T1> : RiskCharacterisationRatioSectionBase<S, T1>
      where S : IExposureContributorKey, new()
      where T1 : RiskDistributionRecordBase<S>, new() {
        public override bool SaveTemporaryData => true;

        public List<(ExposureTarget Target, List<SubstanceRiskDistributionRecord> Records)> RiskRecords { get; set; } = [];
    }
}
