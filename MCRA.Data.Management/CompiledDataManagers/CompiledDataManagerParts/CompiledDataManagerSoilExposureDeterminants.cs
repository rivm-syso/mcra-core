using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<SoilIngestion> GetAllSoilIngestions() {
            if (_data.AllSoilIngestions == null) {
                LoadScope(SourceTableGroup.SoilExposureDeterminants);
                var allSoilIngestions = new List<SoilIngestion>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SoilExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawSoilIngestions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var soilIngestion = new SoilIngestion {
                                        idSubgroup = r.GetStringOrNull(RawSoilIngestions.IdSubgroup, fieldMap),
                                        AgeLower = r.GetDoubleOrNull(RawSoilIngestions.AgeLower, fieldMap),
                                        Sex = r.GetEnum(RawSoilIngestions.Sex, fieldMap, GenderType.Undefined),
                                        Value = r.GetDouble(RawSoilIngestions.Value, fieldMap),
                                        ExposureUnit = r.GetEnum(RawSoilIngestions.ExposureUnit, fieldMap, ExternalExposureUnit.gPerDay),
                                        DistributionType = r.GetEnum(RawSoilIngestions.DistributionType, fieldMap, SoilIngestionDistributionType.Constant),
                                        CvVariability = r.GetDoubleOrNull(RawSoilIngestions.CvVariability, fieldMap)
                                    };
                                    allSoilIngestions.Add(soilIngestion);
                                }
                            }
                        }
                    }
                }
                _data.AllSoilIngestions = allSoilIngestions;
            }
            return _data.AllSoilIngestions;
        }
    }
}
