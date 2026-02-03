using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>All indoor air concentrations.
        /// </summary>
        public IList<AirConcentration> GetAllOutdoorAirConcentrations() {
            if (_data.AllOutdoorAirConcentrations == null) {
                LoadScope(SourceTableGroup.OutdoorAirConcentrations);
                var allOutdoorAirConcentrations = new List<AirConcentration>();
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
                                        var outdoorAirConcentration = new AirConcentration {
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
    }
}
