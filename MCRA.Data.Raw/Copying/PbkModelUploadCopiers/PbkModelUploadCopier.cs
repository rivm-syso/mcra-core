using MCRA.General;
using MCRA.General.Sbml;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.SBML;

namespace MCRA.Data.Raw.Copying.PbkUploadCopiers {

    public class PbkModelUploadCopier : RawDataSourceBulkCopierBase {

        public PbkModelUploadCopier(
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
            progressState.Update("Processing PBK modeldefinitions");
            if (dataSourceReader is SbmlDataSourceReader) {
                var fileName = (dataSourceReader as SbmlDataSourceReader).GetFileReference();
                var reader = new SbmlFileReader();
                var sbmlModel = reader.LoadModel(fileName);
                var converter = new SbmlToPbkModelDefinitionConverter();
                var modelDefinition = converter.Convert(sbmlModel);
                var modelDefinitions = new List<RawPbkModelDefinition> { 
                    new() {
                        Id = modelDefinition.Id,
                        Name = modelDefinition.Name,
                        Description = modelDefinition.Description,
                        FileName = fileName
                    }
                };
                if (tryCopyDataTable(modelDefinitions.ToDataTable(), RawDataSourceTableID.KineticModelDefinitions)) {
                    registerTableGroup(SourceTableGroup.PbkModelDefinitions);
                }
            } else {
                throw new NotImplementedException();
                // TODO: read model definition records from table
                // get sbml files and somehow write these files
            }
        }
    }
}
