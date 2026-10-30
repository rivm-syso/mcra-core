using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        /// <summary>
        /// Returns a list of food-specific conversion factors.
        /// </summary>
        public ICollection<DeterministicSubstanceConversionFactor> GetAllFoodSubstanceConversionFactors() {
            if (_data.AllFoodSubstanceConversionFactors == null) {
                LoadScope(SourceTableGroup.FoodSubstanceConversionFactors);
                var allFoodSubstanceConversionFactors = new List<DeterministicSubstanceConversionFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.FoodSubstanceConversionFactors);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    GetAllFoods();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDeterministicSubstanceConversionFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idMeasuredSubstance = r.GetString(RawDeterministicSubstanceConversionFactors.IdMeasuredSubstance, fieldMap);
                                    var idActiveSubstance = r.GetString(RawDeterministicSubstanceConversionFactors.IdActiveSubstance, fieldMap);
                                    var idFood = r.GetString(RawDeterministicSubstanceConversionFactors.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idMeasuredSubstance)
                                              & CheckLinkSelected(ScopingType.Compounds, idActiveSubstance)
                                              & CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var record = new DeterministicSubstanceConversionFactor {
                                            MeasuredSubstance = _data.GetOrAddSubstance(idMeasuredSubstance),
                                            ActiveSubstance = _data.GetOrAddSubstance(idActiveSubstance),
                                            ConversionFactor = r.GetDouble(RawDeterministicSubstanceConversionFactors.ConversionFactor, fieldMap),
                                            Reference = r.GetStringOrNull(RawDeterministicSubstanceConversionFactors.Reference, fieldMap),
                                            Food = _data.GetOrAddFood(idFood),
                                        };
                                        allFoodSubstanceConversionFactors.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllFoodSubstanceConversionFactors = allFoodSubstanceConversionFactors;
            }
            return _data.AllFoodSubstanceConversionFactors;
        }
    }
}
