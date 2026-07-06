using MCRA.Simulation.Calculators.Stratification;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmContributionsSectionBase<S, T1> : SummarySection
        where S : IHbmExposureContributorKey, new()
        where T1 : HbmContributionRecordBase<S>, new() {

        public List<T1> Records { get; set; } = [];

        protected static T1 createContributionRecord(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            IStratificationLevel stratificationLevel
        ) {
            if (stratificationLevel != null) {
                concentrations = concentrations
                    .Where(r => r.StratificationLevel == stratificationLevel)
                    .ToList();
            }
            var contribution = concentrations.Sum(c => c.TotalEndpointExposure)
                / concentrations.Sum(c => c.TotalExposure) * 100;
            var stratificationLevelParts = (stratificationLevel as CompositeStratificationLevel)?.Parts;
            var record = new T1() {
                Stratification = stratificationLevel?.Name,
                StratificationLevelCode = stratificationLevel?.Code,
                StratificationSubLevelCodes = stratificationLevelParts?.Select(c => c.Code).ToList() ?? [stratificationLevel?.Code],
                StratificationSubLevelNames = stratificationLevelParts?.Select(c => c.Name).ToList() ?? [stratificationLevel?.Name],
                Contribution = contribution,
                NumberOfConcentrations = concentrations.Count,
                Contributions = []
            };
            record.SetDescriptorValues(descriptor);
            return record;
        }

        protected static List<T1> createContributionRecords(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            bool stratify
        ) {
            var results = new List<T1>();
            var groups = concentrations
                .GroupBy(c => stratify ? c.StratificationLevel : null);

            foreach (var group in groups) {
                var record = createContributionRecord(
                    [.. group],
                    descriptor,
                    group.Key
                );
                results.Add(record);
            }
            return results;
        }

        protected void updateRecord(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            IStratificationLevel stratificationLevel,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            if (stratificationLevel != null) {
                concentrations = concentrations
                    .Where(r => r.StratificationLevel == stratificationLevel)
                    .ToList();
            }
            var contribution = concentrations.Sum(c => c.TotalEndpointExposure) 
                / concentrations.Sum(c => c.TotalExposure) * 100;

            var record = Records
                .SingleOrDefault(c => c.GetDescriptorKey() == descriptor.GetKey()
                    && c.StratificationLevelCode == stratificationLevel?.Code
                );

            //only update the record if it exists, so only summarize stratication levels that are in the nominal run
            if (record != null) {
                record.Contributions.Add(contribution);
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                record.UncertaintyCycles = uncertaintyCycles;
            } else {
                var newRecord = new T1();
                if (newRecord.Contributions == null) {
                    newRecord.Contributions = [];
                    newRecord.Contributions.Add(contribution);
                } else {
                    newRecord.Contributions.Add(contribution);
                }
                newRecord.Stratification = stratificationLevel?.Name;
                newRecord.StratificationLevelCode = stratificationLevel?.Code;
                newRecord.StratificationSubLevelCodes = (stratificationLevel as CompositeStratificationLevel)?.Parts.Select(c => c.Code).ToList() ?? [stratificationLevel?.Code];
                newRecord.StratificationSubLevelNames = (stratificationLevel as CompositeStratificationLevel)?.Parts.Select(c => c.Name).ToList() ?? [stratificationLevel?.Name];
                newRecord.Contribution = contribution;
                newRecord.NumberOfConcentrations = concentrations.Count;
                newRecord.UncertaintyLowerBound = lowerBound;
                newRecord.UncertaintyUpperBound = upperBound;
                newRecord.UncertaintyCycles = uncertaintyCycles;
                newRecord.SetDescriptorValues(descriptor);
                Records.Add(newRecord);
            }
        }

        protected void updateStratifiedRecords(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            bool stratify,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            var results = new List<T1>();
            var groups = concentrations
                .GroupBy(c => stratify ? c.StratificationLevel : null);

            foreach (var group in groups) {
                updateRecord(
                    [.. group],
                    descriptor,
                    group.Key,
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );
            }
        }
    }
}
