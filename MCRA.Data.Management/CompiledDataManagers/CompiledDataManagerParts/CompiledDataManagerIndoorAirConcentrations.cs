using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>All indoor air concentrations.
        /// </summary>
        public IList<IndoorAirConcentration> GetAllIndoorAirConcentrations() {
            if (_data.AllIndoorAirConcentrations == null) {
                LoadScope(SourceTableGroup.IndoorAirConcentrations);
                var allIndoorAirConcentrations = new List<IndoorAirConcentration>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.IndoorAirConcentrations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndoorAirConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawIndoorAirConcentrations.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var indoorAirConcentration = new IndoorAirConcentration {
                                            idSample = r.GetStringOrNull(RawIndoorAirConcentrations.IdSample, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Location = r.GetStringOrNull(RawIndoorAirConcentrations.Location, fieldMap),
                                            Concentration = r.GetDouble(RawIndoorAirConcentrations.Concentration, fieldMap),
                                            AirConcentrationUnit = r.GetEnum(
                                                RawIndoorAirConcentrations.Unit,
                                                fieldMap,
                                                AirConcentrationUnit.ugPerm3
                                            )
                                        };
                                        allIndoorAirConcentrations.Add(indoorAirConcentration);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllIndoorAirConcentrations = allIndoorAirConcentrations;
            }
            return _data.AllIndoorAirConcentrations;
        }

        private static void writeIndoorAirConcentrationsToCsv(string tempFolder, IEnumerable<IndoorAirConcentration> indoorAirConcentrations) {
            if (!indoorAirConcentrations?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IndoorAirConcentrations);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawIndoorAirConcentrations)).Length];

            foreach (var indoorAirConcentration in indoorAirConcentrations) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawIndoorAirConcentrations.IdSample, indoorAirConcentration.idSample, ccr);
                row.WriteNonEmptyString(RawIndoorAirConcentrations.IdSubstance, indoorAirConcentration.Substance?.Code, ccr);
                row.WriteNonEmptyString(RawIndoorAirConcentrations.Location, indoorAirConcentration.Location, ccr);
                row.WriteNonNaNDouble(RawIndoorAirConcentrations.Concentration, indoorAirConcentration.Concentration, ccr);
                row.WriteNonEmptyString(RawIndoorAirConcentrations.Unit, indoorAirConcentration.AirConcentrationUnit.ToString(), ccr);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
