using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All read across food translations. The keys of the dictionary are the (data poor) foods for
        /// which there are read-across foods available. The values of the dictionary are the available
        /// read-across foods.
        /// </summary>
        public IDictionary<Food, ICollection<Food>> GetAllFoodExtrapolations() {
            if (_data.AllFoodExtrapolations == null) {
                LoadScope(SourceTableGroup.FoodExtrapolations);
                var foodExtrapolations = new Dictionary<Food, ICollection<Food>>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.FoodExtrapolations);
                if (rawDataSourceIds?.Count > 0) {
                    foreach (var rawDataSourceId in rawDataSourceIds) {
                        using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                            using (var r = rdm.OpenDataReader<RawReadAcrossFoodTranslations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idDataPoorFood = r.GetString(RawReadAcrossFoodTranslations.IdFromFood, fieldMap);
                                    var idReadAcrossFood = r.GetString(RawReadAcrossFoodTranslations.IdToFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idDataPoorFood)
                                              & CheckLinkSelected(ScopingType.Foods, idReadAcrossFood);
                                    if (valid) {
                                        var dataPoorFood = getOrAddFood(idDataPoorFood);
                                        var readAcrossFood = getOrAddFood(idReadAcrossFood);
                                        if (!foodExtrapolations.ContainsKey(dataPoorFood)) {
                                            foodExtrapolations[dataPoorFood] = new HashSet<Food>();
                                        }
                                        foodExtrapolations[dataPoorFood].Add(readAcrossFood);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllFoodExtrapolations = foodExtrapolations;
            }
            return _data.AllFoodExtrapolations;
        }
    }
}
