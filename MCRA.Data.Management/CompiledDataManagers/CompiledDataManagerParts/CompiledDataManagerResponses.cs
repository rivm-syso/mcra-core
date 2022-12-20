using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;
using MCRA.General.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

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
                if (rawDataSourceIds?.Any() ?? false) {
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
                                            ResponseTypeString = r.GetStringOrNull(RawResponses.ResponseType, fieldMap),
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

        private static void writeResponsesDataToCsv(string tempFolder, IEnumerable<Response> responses) {
            if (!responses?.Any() ?? true) {
                return;
            }

            var tdr = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Responses);
            var dtr = tdr.CreateDataTable();

            foreach (var resp in responses) {
                var rowResp = dtr.NewRow();
                rowResp.WriteNonEmptyString(RawResponses.IdResponse, resp.Code);
                rowResp.WriteNonEmptyString(RawResponses.Name, resp.Name);
                rowResp.WriteNonEmptyString(RawResponses.Description, resp.Description);
                rowResp.WriteNonEmptyString(RawResponses.IdSystem, resp.TestSystem.Code);
                rowResp.WriteNonEmptyString(RawResponses.ResponseType, resp.ResponseTypeString);
                rowResp.WriteNonEmptyString(RawResponses.ResponseUnit, resp.ResponseUnit);
                rowResp.WriteNonEmptyString(RawResponses.GuidelineMethod, resp.GuidelineMethod);

                dtr.Rows.Add(rowResp);
            }

            writeToCsv(tempFolder, tdr, dtr);
        }
    }
}
