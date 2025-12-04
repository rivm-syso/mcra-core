using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

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
                                            Unit = r.GetEnum(
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
    }
}
