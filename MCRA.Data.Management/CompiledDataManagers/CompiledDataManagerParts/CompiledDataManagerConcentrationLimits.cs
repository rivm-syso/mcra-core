using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All maximum residue limits.
        /// </summary>
        public IList<ConcentrationLimit> GetAllMaximumConcentrationLimits() {
            if (_data.AllMaximumConcentrationLimits == null) {
                LoadScope(SourceTableGroup.MaximumResidueLimits);
                var allMaximumConcentrationLimits = new List<ConcentrationLimit>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.MaximumResidueLimits);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoods();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawMaximumResidueLimits>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawMaximumResidueLimits.IdFood, fieldMap);
                                    var idSubstance = r.GetString(RawMaximumResidueLimits.IdCompound, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var limit = new ConcentrationLimit {
                                            Food = getOrAddFood(idFood),
                                            Compound = _data.GetOrAddSubstance(idSubstance),
                                            Limit = r.GetDouble(RawMaximumResidueLimits.Limit, fieldMap),
                                            ConcentrationUnit = r.GetEnum(RawMaximumResidueLimits.ConcentrationUnit, fieldMap, ConcentrationUnit.mgPerKg),
                                            ValueType = r.GetEnum(RawMaximumResidueLimits.ValueType, fieldMap, ConcentrationLimitValueType.MaximumResidueLimit),
                                            StartDate = r.GetDateTimeOrNull(RawMaximumResidueLimits.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawMaximumResidueLimits.EndDate, fieldMap),
                                            Reference = r.GetStringOrNull(RawMaximumResidueLimits.Reference, fieldMap)
                                        };
                                        allMaximumConcentrationLimits.Add(limit);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllMaximumConcentrationLimits = allMaximumConcentrationLimits;
            }
            return _data.AllMaximumConcentrationLimits;
        }

        private static void writeMaximumConcentrationLimitDataToCsv(string tempFolder, IEnumerable<ConcentrationLimit> limits) {
            if (!limits?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.MaximumResidueLimits);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawMaximumResidueLimits)).Length];

            foreach (var limit in limits) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawMaximumResidueLimits.IdCompound, limit.Compound?.Code, ccr);
                row.WriteNonEmptyString(RawMaximumResidueLimits.IdFood, limit.Food?.Code, ccr);
                row.WriteNonNaNDouble(RawMaximumResidueLimits.Limit, limit.Limit, ccr);
                row.WriteNonEmptyString(RawMaximumResidueLimits.ConcentrationUnit, limit.ConcentrationUnit.ToString());
                row.WriteNonEmptyString(RawMaximumResidueLimits.ValueType, limit.ValueType.ToString());
                row.WriteNonNullDateTime(RawMaximumResidueLimits.StartDate, limit.StartDate, ccr);
                row.WriteNonNullDateTime(RawMaximumResidueLimits.EndDate, limit.EndDate, ccr);
                row.WriteNonEmptyString(RawMaximumResidueLimits.Reference, limit.Reference);

                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
