using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets alloccupational scenarios.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, OccupationalTask> GetAllOccupationalTasks() {
            if (_data.AllOccupationalTasks == null) {
                LoadScope(SourceTableGroup.OccupationalScenarios);
                var allOccupationalTasks = new Dictionary<string, OccupationalTask>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.OccupationalScenarios);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOccupationalTasks>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var OccupationalTask = new OccupationalTask {
                                        Code = r.GetString(RawOccupationalTasks.Id, fieldMap),
                                        Name = r.GetStringOrNull(RawOccupationalTasks.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawOccupationalTasks.Description, fieldMap),
                                    };
                                    allOccupationalTasks.Add(OccupationalTask.Code, OccupationalTask);
                                }
                            }
                        }
                    }
                }
                _data.AllOccupationalTasks = allOccupationalTasks;
            }
            return _data.AllOccupationalTasks;
        }

        public IDictionary<string, OccupationalScenario> GetAllOccupationalScenarios() {
            if (_data.AllOccupationalScenarios == null) {
                GetAllOccupationalTasks();
                LoadScope(SourceTableGroup.OccupationalScenarios);
                var allOccupationalscenarios = new Dictionary<string, OccupationalScenario>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.OccupationalScenarios);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOccupationalScenarios>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var occupationalScenario = new OccupationalScenario {
                                        Code = r.GetString(RawOccupationalScenarios.Id, fieldMap),
                                        Name = r.GetStringOrNull(RawOccupationalScenarios.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawOccupationalScenarios.Description, fieldMap)
                                    };
                                    allOccupationalscenarios.Add(occupationalScenario.Code, occupationalScenario);
                                }
                            }
                        }

                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOccupationalScenarioTasks>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idOccupationalScenario = r.GetString(RawOccupationalScenarioTasks.IdOccupationalScenario, fieldMap);
                                    if (allOccupationalscenarios.TryGetValue(idOccupationalScenario, out var occupationalScenario)) {
                                        var task = r.GetString(RawOccupationalScenarioTasks.IdOccupationalTask, fieldMap);
                                        var validTask = CheckLinkSelected(ScopingType.OccupationalTasks, task);
                                        if (validTask) {
                                            var occupationalScenarioTask = new OccupationalScenarioTask {
                                                OccupationalTask = _data.AllOccupationalTasks[task],
                                                RpeType = r.GetEnum(RawOccupationalScenarioTasks.RPEType, fieldMap, RPEType.Undefined),
                                                Duration = r.GetDouble(RawOccupationalScenarioTasks.Duration, fieldMap),
                                                Frequency = r.GetDouble(RawOccupationalScenarioTasks.Frequency, fieldMap),
                                                FrequencyResolution = r.GetEnum(RawOccupationalScenarioTasks.FrequencyResolution, fieldMap, FrequencyResolutionType.Undefined)
                                            };
                                            occupationalScenario.Tasks.Add(occupationalScenarioTask);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllOccupationalScenarios = allOccupationalscenarios;
            }
            return _data.AllOccupationalScenarios;
        }


        private static void writeOccupationalScenariosToCsv(string tempFolder, IDictionary<string, OccupationalScenario> occupationalScenarios) {
            if (!occupationalScenarios?.Any() ?? true) {
                return;
            }
            var tdsc = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.OccupationalScenarios);
            var dtsc = tdsc.CreateDataTable();
            var ccrsc = new int[Enum.GetNames<RawOccupationalScenarios>().Length];
            foreach (var scenario in occupationalScenarios.Values) {
                var r = dtsc.NewRow();
                r.WriteNonEmptyString(RawOccupationalScenarios.Id, scenario.Code, ccrsc);
                r.WriteNonEmptyString(RawOccupationalScenarios.Name, scenario.Name, ccrsc);
                r.WriteNonEmptyString(RawOccupationalScenarios.Description, scenario.Description, ccrsc);
                dtsc.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdsc, dtsc, ccrsc);

            var tdsa = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.OccupationalScenarioTasks);
            var dtsa = tdsa.CreateDataTable();
            var ccrsa = new int[Enum.GetNames<RawOccupationalScenarioTasks>().Length];
            foreach (var scenario in occupationalScenarios.Values) {
                foreach (var scenarioTask in scenario.Tasks) {
                    var r = dtsa.NewRow();
                    r.WriteNonEmptyString(RawOccupationalScenarioTasks.IdOccupationalScenario, scenario.Code, ccrsa);
                    r.WriteNonEmptyString(RawOccupationalScenarioTasks.IdOccupationalTask, scenarioTask.OccupationalTask.Code, ccrsa);
                    r.WriteNonEmptyString(RawOccupationalScenarioTasks.RPEType, scenarioTask.RpeType.ToString(), ccrsa);
                    r.WriteNonEmptyString(RawOccupationalScenarioTasks.FrequencyResolution, scenarioTask.FrequencyResolution.ToString(), ccrsa);
                    r.WriteNonNaNDouble(RawOccupationalScenarioTasks.Frequency, scenarioTask.Frequency, ccrsa);
                    r.WriteNonNaNDouble(RawOccupationalScenarioTasks.Duration, scenarioTask.Duration, ccrsa);
                    dtsa.Rows.Add(r);
                }
            }
            writeToCsv(tempFolder, tdsa, dtsa, ccrsa);
        }

        private static void writeOccupationalTasksToCsv(string tempFolder, IDictionary<string, OccupationalTask> OccupationalTask) {
            if (!OccupationalTask?.Any() ?? true) {
                return;
            }
            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.OccupationalTasks);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames<RawOccupationalTasks>().Length];
            foreach (var task in OccupationalTask.Values) {
                var r = dt.NewRow();
                r.WriteNonEmptyString(RawOccupationalTasks.Id, task.Code, ccr);
                r.WriteNonEmptyString(RawOccupationalTasks.Name, task.Name, ccr);
                r.WriteNonEmptyString(RawOccupationalTasks.Description, task.Description, ccr);
                dt.Rows.Add(r);
            }
            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
