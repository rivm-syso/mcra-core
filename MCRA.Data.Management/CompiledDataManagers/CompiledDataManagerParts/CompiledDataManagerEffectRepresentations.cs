using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All effect representations.
        /// </summary>
        public IList<EffectRepresentation> GetAllEffectRepresentations() {
            if (_data.AllEffectRepresentations == null) {
                LoadScope(SourceTableGroup.EffectRepresentations);
                var effectRepresentations = new List<EffectRepresentation>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.EffectRepresentations);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllEffects();
                    GetAllResponses();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawEffectRepresentations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idEffect = r.GetString(RawEffectRepresentations.IdEffect, fieldMap);
                                    var idResponse = r.GetString(RawEffectRepresentations.IdResponse, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                              & CheckLinkSelected(ScopingType.Responses, idResponse);
                                    if (valid) {
                                        var record = new EffectRepresentation {
                                            Effect = _data.GetOrAddEffect(idEffect),
                                            Response = _data.AllResponses[idResponse],
                                            BenchmarkResponse = r.GetDoubleOrNull(RawEffectRepresentations.BenchmarkResponse, fieldMap),
                                            BenchmarkResponseTypeString = r.GetStringOrNull(RawEffectRepresentations.BenchmarkResponseType, fieldMap)
                                        };
                                        effectRepresentations.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllEffectRepresentations = effectRepresentations;
            }
            return _data.AllEffectRepresentations;
        }

        private static void writeEffectRepresentationsToCsv(string tempFolder, IEnumerable<EffectRepresentation> representations) {
            if (!representations?.Any() ?? true) {
                return;
            }

            var tde = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.EffectRepresentations);
            var dte = tde.CreateDataTable();

            foreach (var er in representations) {
                var rdm = dte.NewRow();
                rdm.WriteNonEmptyString(RawEffectRepresentations.IdEffect, er.Effect.Code);
                rdm.WriteNonEmptyString(RawEffectRepresentations.IdResponse, er.Response.Code);
                rdm.WriteNonNullDouble(RawEffectRepresentations.BenchmarkResponse, er.BenchmarkResponse);
                rdm.WriteNonEmptyString(RawEffectRepresentations.BenchmarkResponseType, er.BenchmarkResponseTypeString);

                dte.Rows.Add(rdm);
            }

            writeToCsv(tempFolder, tde, dte);
        }
    }
}
