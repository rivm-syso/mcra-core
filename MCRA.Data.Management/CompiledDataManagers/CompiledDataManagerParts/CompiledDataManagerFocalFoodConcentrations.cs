using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All samples from the compiled data source.
        /// </summary>
        public IDictionary<string, FoodSample> GetAllFocalFoodSamples() {
            if (_data.AllFocalFoodSamples == null) {
                LoadScope(SourceTableGroup.FocalFoods);
                var allAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(StringComparer.OrdinalIgnoreCase);
                var allSampleProperties = new Dictionary<string, SampleProperty>(StringComparer.OrdinalIgnoreCase);
                var foodSamples = new Dictionary<string, FoodSample>(StringComparer.OrdinalIgnoreCase);

                GetAllFoods();
                GetAllCompounds();
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    fillAnalyticalMethods(rdm, allAnalyticalMethods, SourceTableGroup.FocalFoods, ScopingType.FocalFoodAnalyticalMethods);
                    fillSamples(
                        rdm,
                        SourceTableGroup.FocalFoods,
                        allSampleProperties,
                        allAnalyticalMethods,
                        ScopingType.FocalFoodSamples,
                        ScopingType.FocalFoodSampleAnalyses,
                        ScopingType.FocalFoodAnalyticalMethods,
                        foodSamples
                    );
                }
                _data.AllFocalFoodSamples = foodSamples;
                _data.AllFocalFoodAnalyticalMethods = allAnalyticalMethods;
            }
            return _data.AllFocalFoodSamples;
        }


        /// <summary>
        /// Returns all distinct analytical methods of all samples of the compiled data source.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> GetAllFocalFoodAnalyticalMethods() {
            if (_data.AllFocalFoodAnalyticalMethods == null) {
                GetAllFocalFoodSamples();
            }
            return _data.AllFocalFoodAnalyticalMethods;
        }

        /// <summary>
        /// All foods for focal commodities of the compiled data source.
        /// </summary>
        public IDictionary<string, Food> GetAllFocalCommodityFoods() {
            if (_data.AllFocalCommodityFoods == null) {
                GetAllFoods();
                GetAllFocalFoodSamples();
                _data.AllFocalCommodityFoods = _data.AllFocalFoodSamples.Values
                    .Select(r => r.Food)
                    .Distinct()
                    .ToDictionary(r => r.Code);
            }
            return _data.AllFocalCommodityFoods;
        }
    }
}
