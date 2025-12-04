using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<AirIndoorFraction> GetAllAirIndoorFractions() {
            if (_data.AllAirIndoorFractions == null) {
                LoadScope(SourceTableGroup.AirExposureDeterminants);
                var allAirIndoorFractions = new List<AirIndoorFraction>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AirExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAirIndoorFractions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var airIndoorFraction = new AirIndoorFraction {
                                        idSubgroup = r.GetStringOrNull(RawAirIndoorFractions.IdSubgroup, fieldMap),
                                        AgeLower = r.GetDoubleOrNull(RawAirIndoorFractions.AgeLower, fieldMap),
                                        Fraction = r.GetDouble(RawAirIndoorFractions.Fraction, fieldMap),
                                    };
                                    allAirIndoorFractions.Add(airIndoorFraction);
                                }
                            }
                        }
                    }
                }
                _data.AllAirIndoorFractions = allAirIndoorFractions;
            }
            return _data.AllAirIndoorFractions;
        }

        public IList<AirVentilatoryFlowRate> GetAllAirVentilatoryFlowRates() {
            if (_data.AllAirVentilatoryFlowRates == null) {
                LoadScope(SourceTableGroup.AirExposureDeterminants);
                var allAirVentilatoryFlowRates = new List<AirVentilatoryFlowRate>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AirExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAirVentilatoryFlowRates>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var airVentilatoryFlowrate = new AirVentilatoryFlowRate {
                                        idSubgroup = r.GetStringOrNull(RawAirVentilatoryFlowRates.IdSubgroup, fieldMap),
                                        AgeLower = r.GetDoubleOrNull(RawAirVentilatoryFlowRates.AgeLower, fieldMap),
                                        Sex = r.GetEnum(RawAirVentilatoryFlowRates.Sex, fieldMap, GenderType.Undefined),
                                        Value = r.GetDouble(RawAirVentilatoryFlowRates.Value, fieldMap),
                                        DistributionType = r.GetEnum(RawAirVentilatoryFlowRates.DistributionType, fieldMap, VentilatoryFlowRateDistributionType.Constant),
                                        CvVariability = r.GetDoubleOrNull(RawAirVentilatoryFlowRates.CvVariability, fieldMap),
                                    };
                                    allAirVentilatoryFlowRates.Add(airVentilatoryFlowrate);
                                }
                            }
                        }
                    }
                }
                _data.AllAirVentilatoryFlowRates = allAirVentilatoryFlowRates;
            }
            return _data.AllAirVentilatoryFlowRates;
        }
    }
}
