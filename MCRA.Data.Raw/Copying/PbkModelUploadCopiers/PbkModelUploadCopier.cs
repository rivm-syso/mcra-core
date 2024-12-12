using MCRA.General;
using MCRA.General.KineticModelDefinitions.SbmlPbkUtils;
using MCRA.General.Sbml;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.SBML;

namespace MCRA.Data.Raw.Copying.PbkModelUploadCopiers {

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
            var tableNames = dataSourceReader.GetTableNames().ToHashSet();
            if (dataSourceReader is SbmlDataSourceReader sbmlDsReader) {
                var filePath = sbmlDsReader.GetFileReference();
                var reader = new SbmlFileReader();
                try {
                    var sbmlModel = reader.LoadModel(filePath);
                    var converter = new SbmlToPbkModelDefinitionConverter();
                    var modelDefinition = converter.Convert(sbmlModel);
                    var modelDefinitions = new List<RawPbkModelDefinition> {
                        new() {
                            Id = modelDefinition.Id,
                            Name = modelDefinition.Name,
                            Description = modelDefinition.Description,
                            FilePath = filePath,
                        }
                    };
                    if (tryCopyDataTable(modelDefinitions.ToDataTable(), RawDataSourceTableID.KineticModelDefinitions)) {
                        registerTableGroup(SourceTableGroup.PbkModelDefinitions);
                    }
                } catch (PbkModelException ) {
                    throw;
                } catch (Exception ex) {
                    throw new Exception($"SBML model definition syntax, fatal error. {ex.Message}");
                }
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
