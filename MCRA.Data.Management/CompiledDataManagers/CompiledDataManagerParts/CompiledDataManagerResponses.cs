using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        /// <summary>
        /// GetAllResponses
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Response> GetAllResponses() {
            if (_data.AllResponses == null) {
                LoadScope(SourceTableGroup.Responses);
                var allResponses = new Dictionary<string, Response>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Responses);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllTestSystems();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawResponses>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idResponse = r.GetString(RawResponses.IdResponse, fieldMap);
                                    var idSystem = r.GetStringOrNull(RawResponses.IdSystem, fieldMap);
                                    var noSystem = string.IsNullOrEmpty(idSystem);
                                    var valid = IsCodeSelected(ScopingType.Responses, idResponse)
                                              & (noSystem || CheckLinkSelected(ScopingType.TestSystems, idSystem));
                                    if (valid) {
                                        //link to test system (after any test system filter)
                                        var testSystem = noSystem ? null : _data.GetOrAddTestSystem(idSystem);
                                        var response = new Response() {
                                            Code = idResponse,
                                            Name = r.GetStringOrNull(RawResponses.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawResponses.Description, fieldMap),
                                            TestSystem = testSystem,
                                            ResponseType = r.GetEnum<ResponseType>(RawResponses.ResponseType, fieldMap),
                                            ResponseUnit = r.GetStringOrNull(RawResponses.ResponseUnit, fieldMap),
                                            GuidelineMethod = r.GetStringOrNull(RawResponses.GuidelineMethod, fieldMap)
                                        };
                                        allResponses[response.Code] = response;
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllResponses = allResponses;
            }
            return _data.AllResponses;
        }
    }
}
