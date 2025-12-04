using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All TDS food sample compositions
        /// </summary>
        public IList<TDSFoodSampleComposition> GetAllTDSFoodSampleCompositions() {
            if (_data.AllTDSFoodSampleCompositions == null) {
                LoadScope(SourceTableGroup.TotalDietStudy);
                _data.AllTDSFoodSampleCompositions = [];
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.TotalDietStudy);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawTDSFoodSampleCompositions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idTdsFood = r.GetString(RawTDSFoodSampleCompositions.IdTDSFood, fieldMap);
                                    var idFood = r.GetString(RawTDSFoodSampleCompositions.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idTdsFood)
                                              & CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var tdsFood = getOrAddFood(idTdsFood);
                                        var food = getOrAddFood(idFood);
                                        var fsc = new TDSFoodSampleComposition {
                                            Food = food,
                                            TDSFood = tdsFood,
                                            PooledAmount = r.GetDouble(RawTDSFoodSampleCompositions.PooledAmount, fieldMap),
                                            Regionality = r.GetStringOrNull(RawTDSFoodSampleCompositions.Regionality, fieldMap),
                                            Seasonality = r.GetStringOrNull(RawTDSFoodSampleCompositions.Seasonality, fieldMap),
                                            Description = r.GetStringOrNull(RawTDSFoodSampleCompositions.Description, fieldMap)
                                        };
                                        _data.AllTDSFoodSampleCompositions.Add(fsc);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _data.AllTDSFoodSampleCompositions;
        }
    }
}
