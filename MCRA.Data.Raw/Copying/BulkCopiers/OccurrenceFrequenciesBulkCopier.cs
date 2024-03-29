﻿using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class OccurrenceFrequenciesBulkCopier : RawDataSourceBulkCopierBase {

        public OccurrenceFrequenciesBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            progressState.Update($"Processing {SourceTableGroup.OccurrenceFrequencies.GetDisplayName()}");
            var hasMaximumConcentrationLimits = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OccurrenceFrequencies);
            if (hasMaximumConcentrationLimits) {
                registerTableGroup(SourceTableGroup.OccurrenceFrequencies);
            }
            progressState.Update(100);
        }
    }
}
