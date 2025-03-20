using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
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




        private static void writeAirIndoorFractionsToCsv(
            string tempFolder,
            IEnumerable<AirIndoorFraction> airIndoorFractions
        ) {
            if (!airIndoorFractions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AirIndoorFractions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawAirIndoorFractions)).Length];

            foreach (var airIndoorFraction in airIndoorFractions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawAirIndoorFractions.IdSubgroup, airIndoorFraction.idSubgroup, ccr);
                row.WriteNonNullDouble(RawAirIndoorFractions.AgeLower, airIndoorFraction.AgeLower, ccr);
                row.WriteNonNullDouble(RawAirIndoorFractions.Fraction, airIndoorFraction.Fraction, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }

        private static void writeAirVentilatoryFlowRatesToCsv(string tempFolder, IEnumerable<AirVentilatoryFlowRate> airVentilatoryFlowRates) {
            if (!airVentilatoryFlowRates?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AirVentilatoryFlowRates);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawAirVentilatoryFlowRates)).Length];

            foreach (var airVentilatoryFlowRate in airVentilatoryFlowRates) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawAirVentilatoryFlowRates.IdSubgroup, airVentilatoryFlowRate.idSubgroup, ccr);
                row.WriteNonNullDouble(RawAirVentilatoryFlowRates.AgeLower, airVentilatoryFlowRate.AgeLower, ccr);
                row.WriteNonEmptyString(RawAirVentilatoryFlowRates.Sex, airVentilatoryFlowRate.Sex.ToString(), ccr);
                row.WriteNonNullDouble(RawAirVentilatoryFlowRates.Value, airVentilatoryFlowRate.Value, ccr);
                row.WriteNonEmptyString(RawAirVentilatoryFlowRates.DistributionType, airVentilatoryFlowRate.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawAirVentilatoryFlowRates.CvVariability, airVentilatoryFlowRate.CvVariability, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
