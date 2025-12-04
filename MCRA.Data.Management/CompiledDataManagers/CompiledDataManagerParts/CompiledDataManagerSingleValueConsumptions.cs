using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all deterministic substance conversion factors.
        /// </summary>
        public IList<PopulationConsumptionSingleValue> GetAllPopulationConsumptionSingleValues() {
            if (_data.AllPopulationConsumptionSingleValues == null) {
                LoadScope(SourceTableGroup.SingleValueConsumptions);
                var allValues = new List<PopulationConsumptionSingleValue>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SingleValueConsumptions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllPopulations();
                    GetAllFoods();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawPopulationConsumptionSingleValues>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idPopulation = r.GetString(RawPopulationConsumptionSingleValues.IdPopulation, fieldMap);
                                    var idFood = r.GetString(RawPopulationConsumptionSingleValues.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Populations, idPopulation)
                                              & CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = getOrAddFood(idFood, idFood);
                                        var record = new PopulationConsumptionSingleValue {
                                            Population = _data.GetOrAddPopulation(idPopulation),
                                            Food = food,
                                            ValueType = r.GetEnum(RawPopulationConsumptionSingleValues.ValueType, fieldMap, ConsumptionValueType.Undefined),
                                            Percentile = r.GetDoubleOrNull(RawPopulationConsumptionSingleValues.Percentile, fieldMap),
                                            ConsumptionAmount = r.GetDouble(RawPopulationConsumptionSingleValues.ConsumptionAmount, fieldMap),
                                            ConsumptionUnit = r.GetEnum(RawPopulationConsumptionSingleValues.ConsumptionUnit, fieldMap, ConsumptionIntakeUnit.gPerKgBWPerDay),
                                            Reference = r.GetStringOrNull(RawPopulationConsumptionSingleValues.Reference, fieldMap),
                                        };
                                        allValues.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllPopulationConsumptionSingleValues = allValues;
            }
            return _data.AllPopulationConsumptionSingleValues;
        }
    }
}
