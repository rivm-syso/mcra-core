using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class FoodsBulkCopier : RawDataSourceBulkCopierBase {

        public FoodsBulkCopier(
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
            progressState.Update("Processing Foods");
            var hasFoods = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Foods);
            if (hasFoods) {
                registerTableGroup(SourceTableGroup.Foods);

                progressState.Update("Processing types", 10);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ProcessingTypes);

                progressState.Update("Processing Food Properties", 20);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodProperties);

                progressState.Update("Processing Food Unit Weights", 30);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodUnitWeights);

                progressState.Update("Processing Food Consumption Quantifications", 40);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodConsumptionQuantifications);

                progressState.Update("Processing Food Origins", 60);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodOrigins);

                progressState.Update("Processing Food Hierarchies", 70);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodHierarchies);

                progressState.Update("Processing Food Facets", 80);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Facets);

                progressState.Update("Processing Food Facet Descriptors", 90);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FacetDescriptors);
            }

            progressState.MarkCompleted();
        }
    }
}
