using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All single value concentrations from the compiled data source.
        /// </summary>
        public IList<ConcentrationSingleValue> GetAllConcentrationSingleValues() {
            if (_data.AllConcentrationSingleValues == null) {
                var allConcentrationSingleValues = new List<ConcentrationSingleValue>();
                GetAllFoods();
                GetAllCompounds();
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    fillConcentrationSingleValues(rdm, SourceTableGroup.SingleValueConcentrations, allConcentrationSingleValues);
                }
                _data.AllConcentrationSingleValues = allConcentrationSingleValues;
            }
            return _data.AllConcentrationSingleValues;
        }

        private void fillConcentrationSingleValues(
            IRawDataManager rdm,
            SourceTableGroup tableGroup,
            List<ConcentrationSingleValue> concentrationSingleValues
        ) {
            var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(tableGroup);
            if (rawDataSourceIds?.Count > 0) {
                // Read raw food samples from tabulated concentrations
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawConcentrationSingleValues>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idFood = r.GetString(RawConcentrationSingleValues.IdFood, fieldMap);
                            var idSubstance = r.GetString(RawConcentrationSingleValues.IdSubstance, fieldMap);
                            var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                            if (valid) {
                                var record = new ConcentrationSingleValue {
                                    Food = getOrAddFood(idFood),
                                    Substance = _data.GetOrAddSubstance(idSubstance),
                                    Value = r.GetDouble(RawConcentrationSingleValues.Value, fieldMap),
                                    ConcentrationUnit = r.GetEnum(RawConcentrationSingleValues.ConcentrationUnit, fieldMap, ConcentrationUnit.mgPerKg),
                                    Percentile = r.GetDoubleOrNull(RawConcentrationSingleValues.Percentile, fieldMap),
                                    ValueType = r.GetEnum(RawConcentrationSingleValues.ValueType, fieldMap, ConcentrationValueType.MeanConcentration),
                                    Reference = r.GetStringOrNull(RawConcentrationSingleValues.Reference, fieldMap)
                                };
                                concentrationSingleValues.Add(record);
                            }
                        }
                    }
                }
            }
        }

        private static void writeConcentrationSingleValuesToCsv(string tempFolder, IEnumerable<ConcentrationSingleValue> records) {
            if (!records?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConcentrationSingleValues);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawConcentrationSingleValues)).Length];

            foreach (var record in records) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawConcentrationSingleValues.IdSubstance, record.Substance?.Code, ccr);
                row.WriteNonEmptyString(RawConcentrationSingleValues.IdFood, record.Food?.Code, ccr);
                row.WriteNonNaNDouble(RawConcentrationSingleValues.Value, record.Value, ccr);
                row.WriteNonEmptyString(RawConcentrationSingleValues.ValueType, record.ValueType.ToString());
                row.WriteNonNullDouble(RawConcentrationSingleValues.Percentile, record.Percentile);
                row.WriteNonEmptyString(RawConcentrationSingleValues.ConcentrationUnit, record.ConcentrationUnit.ToString());
                row.WriteNonEmptyString(RawConcentrationSingleValues.Reference, record.Reference);

                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
