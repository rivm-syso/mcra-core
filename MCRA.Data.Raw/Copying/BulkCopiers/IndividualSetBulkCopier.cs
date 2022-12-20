using MCRA.Utils.DataFileReading;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public abstract class IndividualSetBulkCopier : RawDataSourceBulkCopierBase {

        public IndividualSetBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        protected bool tryCopyIndividuals(IDataSourceReader dataSourceReader) {
            if (!_parsedDataTables.Contains(RawDataSourceTableID.Individuals)) {
                var hasPropertiesInTables = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndividualProperties);
                if (hasPropertiesInTables) {
                    var hasIndividualPropertyValues = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndividualPropertyValues);
                    if (hasIndividualPropertyValues) {
                        //Three tables Individuals, IndividualProperties, IndividualPropertyValues, not recommended
                        var hasIndividuals = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Individuals);
                        return hasIndividuals;
                    } else {
                        //Two table Individuals, IndividualProperties, recommended
                        tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Individuals);
                        return tryDoBulkCopyWithDynamicPropertyValues(
                            dataSourceReader,
                            RawDataSourceTableID.Individuals,
                            RawDataSourceTableID.IndividualPropertyValues
                        );
                    }
                } else {
                    //One table Individuals not recommended
                    return tryDoBulkCopyWithDynamicProperties(
                        dataSourceReader,
                        RawDataSourceTableID.Individuals,
                        RawDataSourceTableID.IndividualProperties,
                        RawDataSourceTableID.IndividualPropertyValues
                    );
                }
            }
            return true; // Individuals are already copied
        }
    }
}
