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
        public IList<OutdoorAirConcentration> GetAllOutdoorAirConcentrations() {
            if (_data.AllOutdoorAirConcentrations == null) {
                LoadScope(SourceTableGroup.OutdoorAirConcentrations);
                var allOutdoorAirConcentrations = new List<OutdoorAirConcentration>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.OutdoorAirConcentrations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOutdoorAirConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawOutdoorAirConcentrations.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var outdoorAirConcentration = new OutdoorAirConcentration {
                                            idSample = r.GetStringOrNull(RawOutdoorAirConcentrations.IdSample, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Location = r.GetStringOrNull(RawOutdoorAirConcentrations.Location, fieldMap),
                                            Concentration = r.GetDouble(RawOutdoorAirConcentrations.Concentration, fieldMap),
                                            Unit = r.GetEnum(
                                                RawOutdoorAirConcentrations.Unit,
                                                fieldMap,
                                                AirConcentrationUnit.ugPerm3
                                            )
                                        };
                                        allOutdoorAirConcentrations.Add(outdoorAirConcentration);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllOutdoorAirConcentrations = allOutdoorAirConcentrations;
            }
            return _data.AllOutdoorAirConcentrations;
        }

        private static void writeOutdoorAirConcentrationsToCsv(string tempFolder, IEnumerable<OutdoorAirConcentration> outdoorAirConcentrations) {
            if (!outdoorAirConcentrations?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.OutdoorAirConcentrations);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawOutdoorAirConcentrations)).Length];

            foreach (var concentration in outdoorAirConcentrations) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawOutdoorAirConcentrations.IdSample, concentration.idSample, ccr);
                row.WriteNonEmptyString(RawOutdoorAirConcentrations.IdSubstance, concentration.Substance?.Code, ccr);
                row.WriteNonEmptyString(RawOutdoorAirConcentrations.Location, concentration.Location, ccr);
                row.WriteNonNaNDouble(RawOutdoorAirConcentrations.Concentration, concentration.Concentration, ccr);
                row.WriteNonEmptyString(RawOutdoorAirConcentrations.Unit, concentration.Unit.ToString(), ccr);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
