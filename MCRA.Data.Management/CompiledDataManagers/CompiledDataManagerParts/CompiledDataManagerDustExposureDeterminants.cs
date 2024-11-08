using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<DustIngestion> GetAllDustIngestions() {
            if (_data.AllDustIngestions == null) {
                LoadScope(SourceTableGroup.DustExposureDeterminants);
                var allDustIngestions = new List<DustIngestion>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustIngestions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var dustIngestion = new DustIngestion {
                                        idSubgroup = r.GetStringOrNull(RawDustIngestions.IdSubgroup, fieldMap),
                                        AgeLower = r.GetDoubleOrNull(RawDustIngestions.AgeLower, fieldMap),
                                        Sex = r.GetEnum(RawDustIngestions.Sex, fieldMap, GenderType.Undefined),
                                        Value = r.GetDouble(RawDustIngestions.Value, fieldMap),
                                        ExposureUnit = r.GetEnum(RawDustIngestions.ExposureUnit, fieldMap, ExternalExposureUnit.gPerDay),
                                        DistributionType = r.GetEnum(RawDustIngestions.DistributionType, fieldMap, DustIngestionDistributionType.Constant),
                                        CvVariability = r.GetDoubleOrNull(RawDustIngestions.CvVariability, fieldMap)
                                    };
                                    allDustIngestions.Add(dustIngestion);
                                }
                            }
                        }
                    }
                }
                _data.AllDustIngestions = allDustIngestions;
            }
            return _data.AllDustIngestions;
        }

        public IList<DustBodyExposureFraction> GetAllDustBodyExposureFractions() {
            if (_data.AllDustBodyExposureFractions == null) {
                LoadScope(SourceTableGroup.DustExposureDeterminants);
                var allDustBodyExposureFractions = new List<DustBodyExposureFraction>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustBodyExposureFractions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var dustBodyExposureFraction = new DustBodyExposureFraction {
                                        idSubgroup = r.GetStringOrNull(RawDustBodyExposureFractions.IdSubgroup, fieldMap),
                                        AgeLower = r.GetDoubleOrNull(RawDustBodyExposureFractions.AgeLower, fieldMap),
                                        Sex = r.GetEnum(RawDustBodyExposureFractions.Sex, fieldMap, GenderType.Undefined),
                                        Value = r.GetDouble(RawDustBodyExposureFractions.Value, fieldMap),
                                        DistributionType = r.GetEnum(RawDustBodyExposureFractions.DistributionType, fieldMap, DustBodyExposureFractionDistributionType.Constant),
                                        CvVariability = r.GetDoubleOrNull(RawDustBodyExposureFractions.CvVariability, fieldMap)
                                    };
                                    allDustBodyExposureFractions.Add(dustBodyExposureFraction);
                                }
                            }
                        }
                    }
                }
                _data.AllDustBodyExposureFractions = allDustBodyExposureFractions;
            }
            return _data.AllDustBodyExposureFractions;
        }

        public IList<DustAdherenceAmount> GetAllDustAdherenceAmounts() {
            if (_data.AllDustAdherenceAmounts == null) {
                LoadScope(SourceTableGroup.DustExposureDeterminants);
                var allDustAdherenceAmounts = new List<DustAdherenceAmount>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustAdherenceAmounts>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var dustAdherenceAmount = new DustAdherenceAmount {
                                        idSubgroup = r.GetStringOrNull(RawDustAdherenceAmounts.IdSubgroup, fieldMap),
                                        AgeLower = r.GetDoubleOrNull(RawDustAdherenceAmounts.AgeLower, fieldMap),
                                        Sex = r.GetEnum(RawDustAdherenceAmounts.Sex, fieldMap, GenderType.Undefined),
                                        Value = r.GetDouble(RawDustAdherenceAmounts.Value, fieldMap),
                                        DistributionType = r.GetEnum(RawDustAdherenceAmounts.DistributionType, fieldMap, DustAdherenceAmountDistributionType.Constant),
                                        CvVariability = r.GetDoubleOrNull(RawDustAdherenceAmounts.CvVariability, fieldMap)
                                    };
                                    allDustAdherenceAmounts.Add(dustAdherenceAmount);
                                }
                            }
                        }
                    }
                }
                _data.AllDustAdherenceAmounts = allDustAdherenceAmounts;
            }
            return _data.AllDustAdherenceAmounts;
        }

        public IList<DustAvailabilityFraction> GetAllDustAvailabilityFractions() {
            if (_data.AllDustAvailabilityFractions == null) {
                LoadScope(SourceTableGroup.DustExposureDeterminants);
                var allDustAvailabilityFractions = new List<DustAvailabilityFraction>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustAvailabilityFractions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawDustAvailabilityFractions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var dustAvailabilityFraction = new DustAvailabilityFraction {
                                            idSubgroup = r.GetStringOrNull(RawDustAvailabilityFractions.IdSubgroup, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            AgeLower = r.GetDoubleOrNull(RawDustAvailabilityFractions.AgeLower, fieldMap),
                                            Sex = r.GetEnum(RawDustAvailabilityFractions.Sex, fieldMap, GenderType.Undefined),
                                            Value = r.GetDouble(RawDustAvailabilityFractions.Value, fieldMap),
                                            DistributionType = r.GetEnum(RawDustAvailabilityFractions.DistributionType, fieldMap, DustAvailabilityFractionDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawDustAvailabilityFractions.CvVariability, fieldMap)
                                        };
                                        allDustAvailabilityFractions.Add(dustAvailabilityFraction);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllDustAvailabilityFractions = allDustAvailabilityFractions;
            }
            return _data.AllDustAvailabilityFractions;
        }

        private static void writeDustIngestionsToCsv(string tempFolder, IEnumerable<DustIngestion> dustIngestions) {
            if (!dustIngestions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DustIngestions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawDustIngestions)).Length];

            foreach (var dustIngestion in dustIngestions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawDustIngestions.IdSubgroup, dustIngestion.idSubgroup, ccr);
                row.WriteNonNullDouble(RawDustIngestions.AgeLower, dustIngestion.AgeLower, ccr);
                row.WriteNonEmptyString(RawDustIngestions.Sex, dustIngestion.Sex.ToString(), ccr);
                row.WriteNonNullDouble(RawDustIngestions.Value, dustIngestion.Value, ccr);
                row.WriteNonEmptyString(RawDustIngestions.ExposureUnit, dustIngestion.ExposureUnit.ToString(), ccr);
                row.WriteNonEmptyString(RawDustIngestions.DistributionType, dustIngestion.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawDustIngestions.CvVariability, dustIngestion.CvVariability, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }

        private static void writeDustBodyExposureFractionsToCsv(
            string tempFolder,
            IEnumerable<DustBodyExposureFraction> dustBodyExposureFractions
        ) {
            if (!dustBodyExposureFractions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DustBodyExposureFractions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawDustBodyExposureFractions)).Length];

            foreach (var dustBodyExposureFraction in dustBodyExposureFractions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawDustBodyExposureFractions.IdSubgroup, dustBodyExposureFraction.idSubgroup, ccr);
                row.WriteNonNullDouble(RawDustBodyExposureFractions.AgeLower, dustBodyExposureFraction.AgeLower, ccr);
                row.WriteNonEmptyString(RawDustBodyExposureFractions.Sex, dustBodyExposureFraction.Sex.ToString(), ccr);
                row.WriteNonNullDouble(RawDustBodyExposureFractions.Value, dustBodyExposureFraction.Value, ccr);
                row.WriteNonEmptyString(RawDustBodyExposureFractions.DistributionType, dustBodyExposureFraction.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawDustBodyExposureFractions.CvVariability, dustBodyExposureFraction.CvVariability, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }

        private static void writeDustAdherenceAmountsToCsv(
            string tempFolder,
            IEnumerable<DustAdherenceAmount> dustAdherenceAmounts
        ) {
            if (!dustAdherenceAmounts?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DustAdherenceAmounts);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawDustAdherenceAmounts)).Length];

            foreach (var dustAdherenceAmount in dustAdherenceAmounts) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawDustAdherenceAmounts.IdSubgroup, dustAdherenceAmount.idSubgroup, ccr);
                row.WriteNonNullDouble(RawDustAdherenceAmounts.AgeLower, dustAdherenceAmount.AgeLower, ccr);
                row.WriteNonEmptyString(RawDustAdherenceAmounts.Sex, dustAdherenceAmount.Sex.ToString(), ccr);
                row.WriteNonNullDouble(RawDustAdherenceAmounts.Value, dustAdherenceAmount.Value, ccr);
                row.WriteNonEmptyString(RawDustAdherenceAmounts.DistributionType, dustAdherenceAmount.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawDustAdherenceAmounts.CvVariability, dustAdherenceAmount.CvVariability, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }

        private static void writeDustAvailabilityFractionsToCsv(string tempFolder, IEnumerable<DustAvailabilityFraction> dustAvailabilityFractions) {
            if (!dustAvailabilityFractions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DustAvailabilityFractions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawDustAvailabilityFractions)).Length];

            foreach (var dustAvailabilityFraction in dustAvailabilityFractions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawDustAvailabilityFractions.IdSubgroup, dustAvailabilityFraction.idSubgroup, ccr);
                row.WriteNonEmptyString(RawDustAvailabilityFractions.IdSubstance, dustAvailabilityFraction.Substance?.Code, ccr);
                row.WriteNonNullDouble(RawDustAvailabilityFractions.AgeLower, dustAvailabilityFraction.AgeLower, ccr);
                row.WriteNonEmptyString(RawDustAvailabilityFractions.Sex, dustAvailabilityFraction.Sex.ToString(), ccr);
                row.WriteNonNullDouble(RawDustAvailabilityFractions.Value, dustAvailabilityFraction.Value, ccr);
                row.WriteNonEmptyString(RawDustAvailabilityFractions.DistributionType, dustAvailabilityFraction.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawDustAvailabilityFractions.CvVariability, dustAvailabilityFraction.CvVariability, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
