using System.Collections;
using DocumentFormat.OpenXml.InkML;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all PBK model definitions.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, PbkModelDefinition> GetAllPbkModelDefinitions() {
            if (_data.AllPbkModelDefinitions == null) {
                LoadScope(SourceTableGroup.PbkModelDefinitions);
                var allPbkModelDefinitions = new Dictionary<string, PbkModelDefinition>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.PbkModelDefinitions);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawPbkModelDefinitions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModelDefinition = r.GetString(RawPbkModelDefinitions.Id, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.KineticModelDefinitions, idModelDefinition);
                                    if (valid) {
                                        var sbmlFileName = $"{r.GetStringOrNull(RawPbkModelDefinitions.Name, fieldMap)}.sbml";
                                        var fileWithoutExtension = Path.GetFileNameWithoutExtension(sbmlFileName);
                                        if (!fileWithoutExtension.Equals(idModelDefinition, StringComparison.OrdinalIgnoreCase)) {
                                            throw new Exception($"The filename [{fileWithoutExtension}] should be equal to the id of the SBML model. " +
                                                $"Change filename to: {idModelDefinition}.sbml.");
                                        };
                                        var pmd = new PbkModelDefinition {
                                            IdModelDefinition = idModelDefinition,
                                            Name = r.GetStringOrNull(RawPbkModelDefinitions.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawPbkModelDefinitions.Description, fieldMap),
                                            FileName = sbmlFileName,
                                        };
                                        allPbkModelDefinitions.Add(idModelDefinition, pmd);
                                    }
                                }
                            }

                            foreach (var definition in allPbkModelDefinitions.Values) {
                                var sbmlFilePath = rdm.GetFileReference(rawDataSourceId, definition.FileName);
                                var kineticModelDefinition = MCRAKineticModelDefinitions.GetKineticModelDefinition(sbmlFilePath, definition.IdModelDefinition);
                                definition.KineticModelDefinition = kineticModelDefinition;
                            }
                        }
                    }
                }

                // Add hard-coded PBK model definitions
                foreach (var definition in MCRAKineticModelDefinitions.Definitions.Values) {
                    var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { definition.Id };
                    codes.UnionWith(definition.Aliases);
                    foreach (var code in codes) {
                        var valid = IsCodeSelected(ScopingType.KineticModelDefinitions, code);
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

        private static void writePbkModelDefinitionDataToCsv(string tempFolder, IEnumerable<PbkModelDefinition> definitions) {
            if (!definitions?.Any() ?? true) {
                return;
            }
            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.KineticModelDefinitions);
            var dt = td.CreateDataTable();
            foreach (var definition in definitions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawPbkModelDefinitions.Id, definition.IdModelDefinition);
                row.WriteNonEmptyString(RawPbkModelDefinitions.Name, definition.Name);
                row.WriteNonEmptyString(RawPbkModelDefinitions.Description, definition.Description);
                row.WriteNonEmptyString(RawPbkModelDefinitions.FilePath, definition.FileName);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt);
        }
    }
}
