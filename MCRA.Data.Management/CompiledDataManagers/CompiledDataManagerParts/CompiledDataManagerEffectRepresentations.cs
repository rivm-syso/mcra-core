using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

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
                if (rawDataSourceIds?.Count > 0) {
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
                                            BenchmarkResponseType = r.GetEnum(RawEffectRepresentations.BenchmarkResponseType, fieldMap, BenchmarkResponseType.Undefined)
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
    }
}
