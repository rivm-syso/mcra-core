using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all PBK model definitions.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, PbkModelDefinition> GetAllPbkModelDefinitions(string dataFilePath = null) {
            if (_data.AllPbkModelDefinitions == null) {
                LoadScope(SourceTableGroup.PbkModelDefinitions);
                var allPbkModelDefinitions = new Dictionary<string, PbkModelDefinition>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.PbkModelDefinitions);
                var hasRawDataSource = rawDataSourceIds?.Count > 0;
                if (hasRawDataSource) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawPbkModelDefinitions>(rawDataSourceId, out int[] fieldMap, true)) {
                                while (r?.Read() ?? false) {
                                    var idModelDefinition = r.GetString(RawPbkModelDefinitions.Id, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.KineticModelDefinitions, idModelDefinition);
                                    if (valid) {
                                        var sbmlFileName = r.GetStringOrNull(RawPbkModelDefinitions.FilePath, fieldMap);
                                        var pmd = new PbkModelDefinition {
                                            IdModelDefinition = idModelDefinition,
                                            Name = r.GetStringOrNull(RawPbkModelDefinitions.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawPbkModelDefinitions.Description, fieldMap),
                                            FileName = sbmlFileName,
                                        };
                                        //Copy the extracted SBML model file to temp location in parameter
                                        var sbmlFilePath = rdm.GetFileReference(rawDataSourceId, sbmlFileName);
                                        if (dataFilePath != null && Path.Exists(dataFilePath)) {
                                            //move sbml file to provided temp location if present
                                            var newSbmlFilePath = Path.Combine(dataFilePath, Path.GetFileName(sbmlFilePath));
                                            File.Move(sbmlFilePath, newSbmlFilePath);
                                            //point smblFilePath to new location
                                            sbmlFilePath = newSbmlFilePath;
                                        }

                                        var kineticModelDefinition = SbmlPbkModelSpecificationBuilder.CreateFromSbmlFile(sbmlFilePath);
                                        pmd.KineticModelDefinition = kineticModelDefinition;

                                        allPbkModelDefinitions.Add(idModelDefinition, pmd);
                                    }
                                }
                            }
                        }
                    }
                }

                // Add hard-coded PBK model definitions
                foreach (var definition in McraEmbeddedPbkModelDefinitions.Definitions.Values) {
                    var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { definition.Id };
                    codes.UnionWith(definition.Aliases);
                    foreach (var code in codes) {
                        var valid = !hasRawDataSource || IsCodeSelected(ScopingType.KineticModelDefinitions, code);
                        if (valid && !allPbkModelDefinitions.TryGetValue(code, out _)) {
                            var pmd = new PbkModelDefinition {
                                IdModelDefinition = code,
                                Name = definition.Name,
                                Description = definition.Description,
                                KineticModelDefinition = definition
                            };
                            allPbkModelDefinitions.Add(code, pmd);
                        }
                    }
                }

                _data.AllPbkModelDefinitions = allPbkModelDefinitions;
            }
            return _data.AllPbkModelDefinitions;
        }
    }
}
