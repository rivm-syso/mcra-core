using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all air (indoor, outdoor) concentration distributions, within the scope.
        /// </summary>
        public IList<AirConcentrationDistribution> GetAllIndoorAirConcentrationDistributions() {
            if (_data.AllIndoorAirConcentrationDistributions == null) {
                LoadScope(SourceTableGroup.AirConcentrationDistributions);
                var allIndoorAirConcentrationDistributions = new List<AirConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AirConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndoorAirDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawIndoorAirDistributions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unit = r.GetEnum(
                                                RawIndoorAirDistributions.Unit,
                                                fieldMap,
                                                AirConcentrationUnit.ugPerm3
                                            );
                                        var airConcentrationDistribution = new AirConcentrationDistribution {
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Unit = unit,
                                            Mean = r.GetDouble(RawIndoorAirDistributions.Mean, fieldMap),
                                            DistributionType = r.GetEnum(RawIndoorAirDistributions.DistributionType, fieldMap, AirConcentrationDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawIndoorAirDistributions.CvVariability, fieldMap),
                                            OccurrencePercentage = r.GetDoubleOrNull(RawIndoorAirDistributions.OccurrencePercentage, fieldMap),
                                        };
                                        allIndoorAirConcentrationDistributions.Add(airConcentrationDistribution);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllIndoorAirConcentrationDistributions = allIndoorAirConcentrationDistributions;
            }
            return _data.AllIndoorAirConcentrationDistributions;
        }

        public IList<AirConcentrationDistribution> GetAllOutdoorAirConcentrationDistributions() {
            if (_data.AllOutdoorAirConcentrationDistributions == null) {
                LoadScope(SourceTableGroup.AirConcentrationDistributions);
                var allOutdoorAirConcentrationDistributions = new List<AirConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AirConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOutdoorAirDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawOutdoorAirDistributions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unit = r.GetEnum(
                                                RawOutdoorAirDistributions.Unit,
                                                fieldMap,
                                                AirConcentrationUnit.ugPerm3
                                            );
                                        var airConcentrationDistribution = new AirConcentrationDistribution {
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Unit = unit,
                                            Mean = r.GetDouble(RawOutdoorAirDistributions.Mean, fieldMap),
                                            DistributionType = r.GetEnum(RawOutdoorAirDistributions.DistributionType, fieldMap, AirConcentrationDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawOutdoorAirDistributions.CvVariability, fieldMap),
                                            OccurrencePercentage = r.GetDoubleOrNull(RawOutdoorAirDistributions.OccurrencePercentage, fieldMap),
                                        };
                                        allOutdoorAirConcentrationDistributions.Add(airConcentrationDistribution);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllOutdoorAirConcentrationDistributions = allOutdoorAirConcentrationDistributions;
            }
            return _data.AllOutdoorAirConcentrationDistributions;
        }
    }
}
