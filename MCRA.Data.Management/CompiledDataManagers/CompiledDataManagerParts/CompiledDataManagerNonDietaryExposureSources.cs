using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all non-dietary exposure sources.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, NonDietaryExposureSource> GetAllNonDietaryExposureSources() {
            if (_data.AllNonDietaryExposureSources == null) {
                LoadScope(SourceTableGroup.NonDietaryExposureSources);
                var allSources = new Dictionary<string, NonDietaryExposureSource>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.NonDietaryExposureSources);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        // Load non-dietary exposure sources
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawNonDietaryExposureSources>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSource = r.GetString(RawNonDietaryExposureSources.IdNonDietaryExposureSource, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.NonDietaryExposureSources, idSource);
                                    if (valid) {
                                        allSources.Add(idSource, new NonDietaryExposureSource {
                                            Code = idSource,
                                            Name = r.GetStringOrNull(RawNonDietaryExposureSources.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawNonDietaryExposureSources.Description, fieldMap)
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllNonDietaryExposureSources = allSources;
            }
            return _data.AllNonDietaryExposureSources;
        }
    }
}
