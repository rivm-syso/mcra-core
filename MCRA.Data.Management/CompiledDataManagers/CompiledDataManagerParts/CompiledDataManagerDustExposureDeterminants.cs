using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
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
    }
}
