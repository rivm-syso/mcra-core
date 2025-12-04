using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All maximum residue limits.
        /// </summary>
        public ICollection<OccurrenceFrequency> GetAllOccurrenceFrequencies() {
            if (_data.AllOccurrenceFrequencies == null) {
                LoadScope(SourceTableGroup.OccurrenceFrequencies);
                var allOccurrenceFrequencies = new List<OccurrenceFrequency>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.OccurrenceFrequencies);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOccurrenceFrequencies>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawOccurrenceFrequencies.IdFood, fieldMap);
                                    var idSubstance = r.GetString(RawOccurrenceFrequencies.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var limit = new OccurrenceFrequency {
                                            Food = getOrAddFood(idFood),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Percentage = r.GetDouble(RawOccurrenceFrequencies.Percentage, fieldMap),
                                            Reference = r.GetStringOrNull(RawOccurrenceFrequencies.Reference, fieldMap)
                                        };
                                        allOccurrenceFrequencies.Add(limit);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllOccurrenceFrequencies = allOccurrenceFrequencies;
            }
            return _data.AllOccurrenceFrequencies;
        }
    }
}
