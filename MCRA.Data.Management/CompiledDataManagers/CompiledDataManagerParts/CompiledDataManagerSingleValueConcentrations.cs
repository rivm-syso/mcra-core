using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

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
    }
}
