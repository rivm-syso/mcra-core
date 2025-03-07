using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all baseline bod indicators of the compiled datasource.
        /// </summary>
        public IList<BaselineBodIndicator> GetAllBaselineBodIndicators() {
            if (_data.AllBaselineBodIndicators == null) {
                LoadScope(SourceTableGroup.BaselineBodIndicators);
                GetAllEffects();
                var allBaselineBodIndicators = new List<BaselineBodIndicator>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.BaselineBodIndicators);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawBaselineBodIndicators>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idEffect = r.GetString(RawBaselineBodIndicators.IdEffect, fieldMap);

                                    var record = new BaselineBodIndicator() {
                                        Population = r.GetStringOrNull(RawBaselineBodIndicators.Population, fieldMap),
                                        Effect = _data.GetOrAddEffect(idEffect),
                                        BodIndicator = r.GetEnum<BodIndicator>(RawBaselineBodIndicators.BodIndicator, fieldMap),
                                        Value = r.GetDouble(RawBaselineBodIndicators.Value, fieldMap)
                                    };
                                    allBaselineBodIndicators.Add(record);

                                }
                            }
                        }
                    }
                }
                _data.AllBaselineBodIndicators = allBaselineBodIndicators;
            }
            return _data.AllBaselineBodIndicators;
        }

        private static void writeBaselineBodIndicatorsDataToCsv(string tempFolder, IEnumerable<BaselineBodIndicator> baselineBodIndicators) {
            if (!baselineBodIndicators?.Any() ?? true) {
                return;
            }

            var tdBaselineBodIndicators = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.BaselineBodIndicators);
            var dtABaselineBodIndicators = tdBaselineBodIndicators.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawBaselineBodIndicators)).Length];
            foreach (var bodi in baselineBodIndicators) {
                var r = dtABaselineBodIndicators.NewRow();
                r.WriteNonEmptyString(RawBaselineBodIndicators.Population, bodi.Population, ccr);
                r.WriteNonEmptyString(RawBaselineBodIndicators.IdEffect, bodi.Effect?.Code, ccr);
                r.WriteNonEmptyString(RawBaselineBodIndicators.BodIndicator, bodi.BodIndicator.ToString(), ccr);
                r.WriteNonEmptyString(RawBaselineBodIndicators.Value, bodi.Value.ToString(), ccr);
                dtABaselineBodIndicators.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdBaselineBodIndicators, dtABaselineBodIndicators);
        }
    }
}
