using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

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
                                            Reference = r.GetStringOrNull(RawOccurrenceFrequencies.Reference,fieldMap)
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

        private static void writeOccurrenceFrequenciesDataToCsv(string tempFolder, IEnumerable<OccurrenceFrequency> limits) {
            if (!limits?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.OccurrenceFrequencies);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawOccurrenceFrequencies)).Length];

            foreach (var limit in limits) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawOccurrenceFrequencies.IdFood, limit.Food?.Code, ccr);
                row.WriteNonEmptyString(RawOccurrenceFrequencies.IdSubstance, limit.Substance?.Code, ccr);
                row.WriteNonNaNDouble(RawOccurrenceFrequencies.Percentage, limit.Percentage, ccr);
                row.WriteNonEmptyString(RawOccurrenceFrequencies.Reference, limit.Reference);

                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
