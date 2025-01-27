using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All samples from the compiled data source.
        /// </summary>
        public IDictionary<string, FoodSample> GetAllFocalFoodSamples() {
            if (_data.AllFocalFoodSamples == null) {
                LoadScope(SourceTableGroup.FocalFoods);

                GetAllFoods();
                GetAllCompounds();
                loadFocalSampleProperties();

                var focalSampleProperties = _data.AllFocalSampleProperties;
                var allAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(StringComparer.OrdinalIgnoreCase);
                var foodSamples = new Dictionary<string, FoodSample>(StringComparer.OrdinalIgnoreCase);
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    fillAnalyticalMethods(rdm, allAnalyticalMethods, SourceTableGroup.FocalFoods, ScopingType.FocalFoodAnalyticalMethods);
                    fillSamples(
                        rdm,
                        SourceTableGroup.FocalFoods,
                        focalSampleProperties,
                        allAnalyticalMethods,
                        ScopingType.FocalFoodSamples,
                        ScopingType.FocalFoodSampleAnalyses,
                        ScopingType.FocalFoodAnalyticalMethods,
                        foodSamples
                    );
                }
                _data.AllFocalFoodSamples = foodSamples;
                _data.AllFocalFoodAnalyticalMethods = allAnalyticalMethods;
                _data.AllFocalSampleProperties = focalSampleProperties;
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

        private void loadFocalSampleProperties() {
            var allFocalSampleProperties = _data.AllFocalSampleProperties
                ?? new Dictionary<string, SampleProperty>(StringComparer.OrdinalIgnoreCase);
            using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                var ids = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.FocalFoods);

                if (ids?.Count > 0) {
                    foreach (var id in ids) {
                        // Read property values
                        using (var r = rdm.OpenDataReader<RawSamplePropertyValues>(id, out int[] fieldMap)) {
                            while (r?.Read() ?? false) {
                                var propertyName = r.GetString(RawSamplePropertyValues.PropertyName, fieldMap);
                                if (!allFocalSampleProperties.TryGetValue(propertyName, out var sampleProperty)) {
                                    allFocalSampleProperties[propertyName] = sampleProperty = new SampleProperty {
                                        Name = r.GetString(RawSamplePropertyValues.PropertyName, fieldMap),
                                    };
                                }
                                var propertyValue = new SamplePropertyValue {
                                    SampleProperty = sampleProperty,
                                    TextValue = r.GetStringOrNull(RawSamplePropertyValues.TextValue, fieldMap),
                                    DoubleValue = r.GetDoubleOrNull(RawSamplePropertyValues.DoubleValue, fieldMap)
                                };
                                sampleProperty.SamplePropertyValues.Add(propertyValue);
                            }
                        }
                    }
                }
            }
            _data.AllFocalSampleProperties = allFocalSampleProperties;
        }
    }
}
