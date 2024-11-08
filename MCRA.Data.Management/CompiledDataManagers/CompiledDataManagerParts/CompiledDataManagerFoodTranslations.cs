using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Utils;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all foods from the compiled data source.
        /// </summary>
        /// <returns></returns>
        public IList<FoodTranslation> GetAllFoodTranslations() {
            if (_data.AllFoodTranslations == null) {
                LoadScope(SourceTableGroup.FoodTranslations);
                var allFoodTranslations = new List<FoodTranslation>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.FoodTranslations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        var foodEx2Foods = new List<Food>();
                        // Load food translations
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodTranslations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFoodFrom = r.GetString(RawFoodTranslations.IdFromFood, fieldMap);
                                    var idFoodTo = r.GetString(RawFoodTranslations.IdToFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFoodFrom)
                                              & CheckLinkSelected(ScopingType.Foods, idFoodTo);
                                    if (valid) {
                                        var foodFrom = getOrAddFood(idFoodFrom);
                                        var foodTo = getOrAddFood(idFoodTo);
                                        var foodTranslation = new FoodTranslation {
                                            FoodFrom = foodFrom,
                                            FoodTo = foodTo,
                                            Proportion = r.GetDouble(RawFoodTranslations.Proportion, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawFoodTranslations.IdPopulation, fieldMap)
                                        };
                                        if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(idFoodFrom)) {
                                            foodEx2Foods.Add(foodFrom);
                                        }
                                        if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(idFoodTo)) {
                                            foodEx2Foods.Add(foodTo);
                                        }
                                        allFoodTranslations.Add(foodTranslation);
                                    }
                                }
                            }
                        }
                        resolveFoodEx2Foods(_data.AllFoods, foodEx2Foods.ToArray());
                        
                        resolveProcessedFoods(_data.AllFoods);
                    }
                }
                _data.AllFoodTranslations = allFoodTranslations;
            }
            return _data.AllFoodTranslations;
        }


        private static void writeFoodTranslationDataToCsv(string tempFolder, IEnumerable<FoodTranslation> foodTranslations) {
            if (!foodTranslations?.Any() ?? true) {
                return;
            }

            var tdTrans = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodTranslations);
            var dtTrans = tdTrans.CreateDataTable();
            var ccrTrans = new int[Enum.GetNames(typeof(RawFoodTranslations)).Length];

            foreach (var t in foodTranslations) {
                var rTrans = dtTrans.NewRow();
                rTrans.WriteNonEmptyString(RawFoodTranslations.IdFromFood, t.FoodFrom.Code, ccrTrans);
                rTrans.WriteNonEmptyString(RawFoodTranslations.IdToFood, t.FoodTo.Code, ccrTrans);
                rTrans.WriteNonNaNDouble(RawFoodTranslations.Proportion, t.Proportion, ccrTrans);
                rTrans.WriteNonEmptyString(RawFoodTranslations.IdPopulation, t.IdPopulation, ccrTrans);
                dtTrans.Rows.Add(rTrans);
            }
            writeToCsv(tempFolder, tdTrans, dtTrans, ccrTrans);
        }
    }
}
