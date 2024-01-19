using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        public IDictionary<string, TestSystem> GetAllTestSystems() {
            if (_data.AllTestSystems == null) {
                LoadScope(SourceTableGroup.TestSystems);
                var allTestSystems = new Dictionary<string, TestSystem>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.TestSystems);
                if (rawDataSourceIds?.Any() ?? false) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawTestSystems>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSystem = r.GetString(RawTestSystems.IdSystem, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.TestSystems, idSystem);
                                    var testSystemType = r.GetEnum(RawTestSystems.TestSystemType, fieldMap, TestSystemType.Undefined);
                                    var exposureRoute = r.GetEnum(
                                        RawTestSystems.ExposureRouteType,
                                        fieldMap,
                                        ExposureRoute.Undefined
                                    );
                                    if (valid) {
                                        var testSystem = new TestSystem() {
                                            Code = idSystem,
                                            Name = r.GetStringOrNull(RawTestSystems.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawTestSystems.Description, fieldMap),
                                            TestSystemType = testSystemType,
                                            Organ = r.GetStringOrNull(RawTestSystems.Organ, fieldMap),
                                            Species = r.GetStringOrNull(RawTestSystems.Species, fieldMap),
                                            Strain = r.GetStringOrNull(RawTestSystems.Strain, fieldMap),
                                            ExposureRoute = exposureRoute,
                                            Reference = r.GetStringOrNull(RawTestSystems.Reference, fieldMap),
                                        };
                                        allTestSystems[testSystem.Code] = testSystem;
                                    }
                                }
                            }
                        }
                    }
                }

                // Add items by code from the scope where no matched items were found in the source
                foreach (var code in GetCodesInScope(ScopingType.TestSystems).Except(allTestSystems.Keys, StringComparer.OrdinalIgnoreCase)) {
                    allTestSystems[code] = new TestSystem { Code = code };
                }
                _data.AllTestSystems = allTestSystems;
            }
            return _data.AllTestSystems;
        }

        private static void writeTestSystemsDataToCsv(string tempFolder, IEnumerable<TestSystem> testSystems) {
            if (!testSystems?.Any() ?? true) {
                return;
            }

            var tdt = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.TestSystems);
            var dtt = tdt.CreateDataTable();

            foreach (var ts in testSystems) {
                var rowts = dtt.NewRow();
                rowts.WriteNonEmptyString(RawTestSystems.IdSystem, ts.Code);
                rowts.WriteNonEmptyString(RawTestSystems.Name, ts.Name);
                rowts.WriteNonEmptyString(RawTestSystems.Description, ts.Description);
                rowts.WriteNonEmptyString(RawTestSystems.TestSystemType, ts.TestSystemType.ToString());
                rowts.WriteNonEmptyString(RawTestSystems.Organ, ts.Organ);
                rowts.WriteNonEmptyString(RawTestSystems.Species, ts.Species);
                rowts.WriteNonEmptyString(RawTestSystems.Strain, ts.Strain);
                rowts.WriteNonEmptyString(RawTestSystems.ExposureRouteType, ts.ExposureRoute.ToString());
                rowts.WriteNonEmptyString(RawTestSystems.Reference, ts.Reference);

                dtt.Rows.Add(rowts);
            }

            writeToCsv(tempFolder, tdt, dtt);
        }
    }
}
