using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Utils;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System.Data;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all foods from the compiled data source.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Food> GetAllFoods() {
            if (_data.AllFoods == null) {
                LoadScope(SourceTableGroup.Foods);
                var allFoods = new Dictionary<string, Food>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Foods);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFacets();
                    GetAllProcessingTypes();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // While creating foods, check for FoodEx2 codes
                        var foodEx2Foods = new List<Food>();

                        // Load raw foods and directly process the Foodex2 facets, if any are encountered
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoods>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawFoods.IdFood, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = new Food {
                                            Code = idFood,
                                            Name = r.GetStringOrNull(RawFoods.Name, fieldMap),
                                            AlternativeName = r.GetStringOrNull(RawFoods.AlternativeName, fieldMap),
                                            Description = r.GetStringOrNull(RawFoods.Description, fieldMap)
                                        };
                                        if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(idFood)) {
                                            foodEx2Foods.Add(food);
                                        }
                                        allFoods[idFood] = food;
                                    }
                                }
                            }
                        }

                        // Add foods from scope
                        var readingScope = GetCodesInScope(ScopingType.Foods);
                        foreach (var code in readingScope.Except(allFoods.Keys, StringComparer.OrdinalIgnoreCase)) {
                            var food = new Food { Code = code };
                            if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(code)) {
                                foodEx2Foods.Add(food);
                            }
                            allFoods[code] = food;
                        }

                        // Load food unit weights
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodUnitWeights>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawFoodUnitWeights.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = getOrAddFood(allFoods, idFood);
                                        var location = r.GetStringOrNull(RawFoodUnitWeights.Location, fieldMap);
                                        var record = new FoodUnitWeight {
                                            Food = food,
                                            Location = location,
                                            Qualifier= r.GetEnum(RawFoodUnitWeights.Qualifier, fieldMap, ValueQualifier.Equals),
                                            Value = r.GetDouble(RawFoodUnitWeights.Value, fieldMap),
                                            ValueType= r.GetEnum(RawFoodUnitWeights.ValueType, fieldMap, UnitWeightValueType.UnitWeightRac),
                                            Reference = r.GetStringOrNull(RawFoodUnitWeights.Reference, fieldMap),
                                        };
                                        if (!string.IsNullOrEmpty(location)) {
                                            food.FoodUnitWeights.Add(record);
                                        } else {
                                            if (record.ValueType == UnitWeightValueType.UnitWeightEp) {
                                                food.DefaultUnitWeightEp = record;
                                            } else {
                                                food.DefaultUnitWeightRac = record;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Load food consumption quantifications and uncertainties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodConsumptionQuantifications>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawFoodConsumptionQuantifications.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = getOrAddFood(allFoods, idFood);
                                        var idUnit = r.GetString(RawFoodConsumptionQuantifications.IdUnit, fieldMap);
                                        var fcq = new FoodConsumptionQuantification {
                                            Food = food,
                                            UnitCode = idUnit,
                                            UnitWeight = r.GetDouble(RawFoodConsumptionQuantifications.UnitWeight, fieldMap),
                                            AmountUncertainty = r.GetDoubleOrNull(RawFoodConsumptionQuantifications.AmountUncertainty, fieldMap),
                                            UnitWeightUncertainty = r.GetDoubleOrNull(RawFoodConsumptionQuantifications.UnitWeightUncertainty, fieldMap)
                                        };
                                        food.FoodConsumptionQuantifications[idUnit] = fcq;
                                    }
                                }
                            }
                        }

                        // Load food properties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodProperties>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawFoodProperties.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = getOrAddFood(allFoods, idFood);
                                        var fp = new FoodProperty {
                                            LargePortion = r.GetDoubleOrNull(RawFoodProperties.LargePortion, fieldMap),
                                            LargePortionChildren = r.GetDoubleOrNull(RawFoodProperties.LargePortionChildren, fieldMap),
                                            LargePortionBabies = r.GetDoubleOrNull(RawFoodProperties.LargePortionBabies, fieldMap),
                                            UnitWeight = r.GetDoubleOrNull(RawFoodProperties.UnitWeight, fieldMap),
                                        };
                                        if (fp.LargePortion.HasValue || fp.UnitWeight.HasValue) {
                                            food.Properties = fp;
                                        }
                                    }
                                }
                            }
                        }

                        // Load FoodOrigins
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodOrigins>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawFoodOrigins.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = getOrAddFood(allFoods, idFood);
                                        var fo = new FoodOrigin {
                                            Food = food,
                                            Percentage = r.GetDouble(RawFoodOrigins.Percentage, fieldMap),
                                            MarketLocation = r.GetStringOrNull(RawFoodOrigins.MarketLocation, fieldMap),
                                            OriginLocation = r.GetStringOrNull(RawFoodOrigins.OriginLocation, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawFoodOrigins.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawFoodOrigins.EndDate, fieldMap),
                                        };
                                        food.FoodOrigins.Add(fo);
                                    }
                                }
                            }
                        }

                        // Load FoodHierarchies
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodHierarchies>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idParent = r.GetString(RawFoodHierarchies.IdParent, fieldMap);
                                    var idFood = r.GetString(RawFoodHierarchies.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idParent)
                                              & CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        Food parent = getOrAddFood(allFoods, idParent);
                                        Food food = getOrAddFood(allFoods, idFood);
                                        food.Parent = parent;
                                        parent.Children.Add(food);
                                    }
                                }
                            }
                        }

                        // Add items by code from the scope where no matched items were found in the source
                        foreach (var code in GetCodesInScope(ScopingType.Foods).Except(allFoods.Keys, StringComparer.OrdinalIgnoreCase)) {
                            getOrAddFood(allFoods, code);
                        }

                        resolveFoodEx2Foods(allFoods, foodEx2Foods.ToArray());
                        resolveProcessedFoods(allFoods);
                    }
                }

                _data.AllFoods = allFoods;
            }
            return _data.AllFoods;
        }

        /// <summary>
        /// All processingtypes form of the compiled data source.
        /// </summary>
        public IDictionary<string, ProcessingType> GetAllProcessingTypes() {
            if (_data.AllProcessingTypes == null) {
                LoadScope(SourceTableGroup.Foods);
                var allProcessingTypes = new Dictionary<string, ProcessingType>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Foods);
                if (rawDataSourceIds?.Any() ?? false) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawProcessingTypes>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var processingTypeCode = r.GetString(RawProcessingTypes.IdProcessingType, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.ProcessingTypes, processingTypeCode);
                                    if (valid) {
                                        var pt = new ProcessingType {
                                            Code = processingTypeCode,
                                            Name = r.GetStringOrNull(RawProcessingTypes.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawProcessingTypes.Description, fieldMap),
                                            IsBulkingBlending = r.GetBoolean(RawProcessingTypes.BulkingBlending, fieldMap),
                                            DistributionType = r.GetEnum(RawProcessingTypes.DistributionType, fieldMap, ProcessingDistributionType.LogisticNormal)
                                        };
                                        allProcessingTypes[pt.Code] = pt;
                                    }
                                }
                            }
                        }

                        foreach (var code in GetCodesInScope(ScopingType.ProcessingTypes).Except(allProcessingTypes.Keys, StringComparer.OrdinalIgnoreCase)) {
                            allProcessingTypes[code] = new ProcessingType { Code = code };
                        }
                    }
                }
                _data.AllProcessingTypes = allProcessingTypes;
            }
            return _data.AllProcessingTypes;
        }

        /// <summary>
        /// Gets all FoodEx2 facets.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Facet> GetAllFacets() {
            if (_data.AllFacets == null) {
                var allFoodFacets = new Dictionary<string, FoodFacet>(StringComparer.OrdinalIgnoreCase);
                var allFacets = new Dictionary<string, Facet>(StringComparer.OrdinalIgnoreCase);
                var allFacetDescriptors = new Dictionary<string, FacetDescriptor>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Foods);
                if (rawDataSourceIds?.Any() ?? false) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        // Load Facets
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFacets>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var key = r.GetString(RawFacets.IdFacet, fieldMap);
                                    allFacets.Add(key, new Facet {
                                        Code = key,
                                        Name = r.GetStringOrNull(RawFacets.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawFacets.Description, fieldMap)
                                    });
                                }
                            }
                        }

                        // Load FacetDescriptors
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFacetDescriptors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFacetDescriptor = r.GetString(RawFacetDescriptors.IdFacetDescriptor, fieldMap);
                                    var facetDescriptor = new FacetDescriptor {
                                        Code = idFacetDescriptor,
                                        Name = r.GetStringOrNull(RawFacetDescriptors.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawFacetDescriptors.Description, fieldMap),
                                    };
                                    allFacetDescriptors.Add(idFacetDescriptor, facetDescriptor);
                                }
                            }
                        }
                    }
                }
                _data.AllFacets = allFacets;
                _data.AllFacetDescriptors = allFacetDescriptors;
                _data.AllFoodFacets = allFoodFacets;
            }
            return _data.AllFacets;
        }

        private Food getOrAddFood(string id, string name = null, bool resolveFoodEx2 = true) {
            return getOrAddFood(_data.AllFoods, id, name, resolveFoodEx2);
        }

        private Food getOrAddFood(IDictionary<string, Food> foods, string id, string name = null, bool resolveFoodEx2 = true) {
            if (string.IsNullOrWhiteSpace(id)) {
                return null;
            }
            if (!foods.TryGetValue(id, out Food item)) {
                item = new Food { Code = id, Name = name ?? id };
                foods.Add(id, item);
                if (resolveFoodEx2) {
                    resolveFoodEx2Foods(foods, item);
                }
            }
            return item;
        }

        private FoodFacet getOrAddFoodFacet(string code) {
            if (string.IsNullOrWhiteSpace(code)) {
                return null;
            }
            if (!_data.AllFoodFacets.TryGetValue(code, out var foodFacet)) {
                (var facetCode, var descriptorCode) = FoodCodeUtilities.SplitFoodEx2FoodFacetCode(code);
                if (!string.IsNullOrEmpty(facetCode) && !string.IsNullOrEmpty(descriptorCode)) {
                    if (!_data.AllFacets.TryGetValue(facetCode, out var facet)) {
                        facet = new Facet() { Code = facetCode };
                        _data.AllFacets.Add(facetCode, facet);
                    }
                    if (!_data.AllFacetDescriptors.TryGetValue(descriptorCode, out var facetDescriptor)) {
                        facetDescriptor = new FacetDescriptor() {
                            Code = descriptorCode,
                            Name = _data.AllProcessingTypes.TryGetValue(code, out var t) ? t.Name : null
                        };
                        _data.AllFacetDescriptors.Add(descriptorCode, facetDescriptor);
                    }
                    foodFacet = new FoodFacet() {
                        Facet = facet,
                        FacetDescriptor = facetDescriptor,
                        Name = facetDescriptor.Name
                    };
                    _data.AllFoodFacets.Add(code, foodFacet);
                }
            }
            return foodFacet;
        }

        private static void writeFoodsDataToCsv(string tempFolder, IEnumerable<Food> foods) {
            if (!foods?.Any() ?? true) {
                return;
            }

            var tdFood = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Foods);
            var dtFood = tdFood.CreateDataTable();
            var tdTrans = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodTranslations);
            var dtTrans = tdTrans.CreateDataTable();
            var tdQuant = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodConsumptionQuantifications);
            var dtQuant = tdQuant.CreateDataTable();
            var tdProps = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodProperties);
            var dtProps = tdProps.CreateDataTable();
            var tdOrigin = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodOrigins);
            var dtOrigin = tdOrigin.CreateDataTable();
            var tdHier = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodHierarchies);
            var dtHier = tdHier.CreateDataTable();
            var tdFacet = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Facets);
            var dtFacet = tdFacet.CreateDataTable();
            var tdUnitWts = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodUnitWeights);
            var dtUnitWts = tdUnitWts.CreateDataTable();

            var facetIds = new HashSet<string>();

            var ccrFoods = new int[Enum.GetNames(typeof(RawFoods)).Length];
            var ccrTrans = new int[Enum.GetNames(typeof(RawFoodTranslations)).Length];
            var ccrQuant = new int[Enum.GetNames(typeof(RawFoodConsumptionQuantifications)).Length];
            var ccrProp = new int[Enum.GetNames(typeof(RawFoodProperties)).Length];
            var ccrOrig = new int[Enum.GetNames(typeof(RawFoodOrigins)).Length];
            var ccrUnitWt = new int[Enum.GetNames(typeof(RawFoodUnitWeights)).Length];

            foreach (var f in foods) {
                var rFood = dtFood.NewRow();
                rFood.WriteNonEmptyString(RawFoods.IdFood, f.Code, ccrFoods);
                rFood.WriteNonEmptyString(RawFoods.Name, f.Name, ccrFoods);
                rFood.WriteNonEmptyString(RawFoods.AlternativeName, f.AlternativeName, ccrFoods);
                rFood.WriteNonEmptyString(RawFoods.Description, f.Description, ccrFoods);
                dtFood.Rows.Add(rFood);

                //food consumption quantifications
                foreach (var q in f.FoodConsumptionQuantifications.Values) {
                    var rQuant = dtQuant.NewRow();
                    rQuant.WriteNonEmptyString(RawFoodConsumptionQuantifications.IdFood, q.Food.Code, ccrQuant);
                    rQuant.WriteNonEmptyString(RawFoodConsumptionQuantifications.IdUnit, q.UnitCode, ccrQuant);
                    rQuant.WriteNonNaNDouble(RawFoodConsumptionQuantifications.UnitWeight, q.UnitWeight, ccrQuant);
                    rQuant.WriteNonNullDouble(RawFoodConsumptionQuantifications.AmountUncertainty, q.AmountUncertainty, ccrQuant);
                    rQuant.WriteNonNullDouble(RawFoodConsumptionQuantifications.UnitWeightUncertainty, q.UnitWeightUncertainty, ccrQuant);
                    dtQuant.Rows.Add(rQuant);
                }

                //food properties
                if (f.Properties != null) {
                    var rProp = dtProps.NewRow();
                    rProp.WriteNonEmptyString(RawFoodProperties.IdFood, f.Code, ccrProp);
                    rProp.WriteNonNullDouble(RawFoodProperties.LargePortion, f.Properties.LargePortion, ccrProp);
                    rProp.WriteNonNullDouble(RawFoodProperties.LargePortionBabies, f.Properties.LargePortionBabies, ccrProp);
                    rProp.WriteNonNullDouble(RawFoodProperties.LargePortionChildren, f.Properties.LargePortionChildren, ccrProp);
                    rProp.WriteNonNullDouble(RawFoodProperties.UnitWeight, f.Properties.UnitWeight, ccrProp);
                    dtProps.Rows.Add(rProp);
                }

                //food origins
                foreach (var orig in f.FoodOrigins) {
                    var rOrig = dtOrigin.NewRow();
                    rOrig.WriteNonEmptyString(RawFoodOrigins.IdFood, f.Code, ccrOrig);
                    rOrig.WriteNonEmptyString(RawFoodOrigins.MarketLocation, orig.MarketLocation, ccrOrig);
                    rOrig.WriteNonEmptyString(RawFoodOrigins.OriginLocation, orig.OriginLocation, ccrOrig);
                    rOrig.WriteNonNaNDouble(RawFoodOrigins.Percentage, orig.Percentage, ccrOrig);
                    rOrig.WriteNonNullDateTime(RawFoodOrigins.StartDate, orig.StartDate, ccrOrig);
                    rOrig.WriteNonNullDateTime(RawFoodOrigins.EndDate, orig.EndDate, ccrOrig);
                    dtOrigin.Rows.Add(rOrig);
                }

                //food hierarchies: check parents only
                if (f.Parent != null) {
                    var rHier = dtHier.NewRow();
                    rHier.WriteNonEmptyString(RawFoodHierarchies.IdFood, f.Code);
                    rHier.WriteNonEmptyString(RawFoodHierarchies.IdParent, f.Parent.Code);
                    dtHier.Rows.Add(rHier);
                }

                foreach (var facet in f.FoodFacets) {
                    if (!facetIds.Contains(facet.Facet.Code)) {
                        var rFacet = dtFacet.NewRow();
                        rFacet.WriteNonEmptyString(RawFacets.IdFacet, facet.Facet.Code);
                        rFacet.WriteNonEmptyString(RawFacets.Name, facet.Facet.Name);
                        dtFacet.Rows.Add(rFacet);
                    }
                }

                //Unit weights
                var uwtList = new List<FoodUnitWeight>();
                uwtList.AddRange(f.FoodUnitWeights);
                if (f.DefaultUnitWeightEp != null) {
                    uwtList.Add(f.DefaultUnitWeightEp);
                }
                if (f.DefaultUnitWeightRac != null) {
                    uwtList.Add(f.DefaultUnitWeightRac);
                }
                foreach (var uwt in uwtList) {
                    var rUwt = dtUnitWts.NewRow();
                    rUwt.WriteNonEmptyString(RawFoodUnitWeights.IdFood, f.Code, ccrUnitWt);
                    rUwt.WriteNonEmptyString(RawFoodUnitWeights.Location, uwt.Location, ccrUnitWt);
                    rUwt.WriteNonEmptyString(RawFoodUnitWeights.Qualifier, uwt.Qualifier.ToString(), ccrUnitWt);
                    rUwt.WriteNonEmptyString(RawFoodUnitWeights.ValueType, uwt.ValueType.ToString(), ccrUnitWt);
                    rUwt.WriteNonNaNDouble(RawFoodUnitWeights.Value, uwt.Value, ccrUnitWt);
                    rUwt.WriteNonEmptyString(RawFoodUnitWeights.Reference, uwt.Reference, ccrUnitWt);
                    dtUnitWts.Rows.Add(rUwt);
                }
            }

            writeToCsv(tempFolder, tdFood, dtFood, ccrFoods);
            writeToCsv(tempFolder, tdTrans, dtTrans, ccrTrans);
            writeToCsv(tempFolder, tdQuant, dtQuant, ccrQuant);
            writeToCsv(tempFolder, tdProps, dtProps, ccrProp);
            writeToCsv(tempFolder, tdOrigin, dtOrigin, ccrOrig);
            writeToCsv(tempFolder, tdHier, dtHier);
            writeToCsv(tempFolder, tdFacet, dtFacet);
            writeToCsv(tempFolder, tdUnitWts, dtUnitWts);
        }

        private static void writeFacetDescriptorsDataToCsv(string tempFolder, IDictionary<string, FacetDescriptor> facetDescriptors) {
            if (!facetDescriptors?.Any() ?? true) {
                return;
            }
            var tdDescriptors = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FacetDescriptors);
            var dtDescriptors = tdDescriptors.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawFacetDescriptors)).Length];
            foreach (var kvp in facetDescriptors) {
                var r = dtDescriptors.NewRow();
                r.WriteNonEmptyString(RawFacetDescriptors.IdFacetDescriptor, kvp.Key, ccr);
                r.WriteNonEmptyString(RawFacetDescriptors.Name, kvp.Value.Name, ccr);
                r.WriteNonEmptyString(RawFacetDescriptors.Description, kvp.Value.Description, ccr);
                dtDescriptors.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdDescriptors, dtDescriptors);
        }

        private void resolveProcessedFoods(IDictionary<string, Food> foods) {
            foreach (var food in foods.Values) {
                if (FoodCodeUtilities.IsProcessedFood(food.Code)) {
                    var baseCode = FoodCodeUtilities.GetProcessedFoodBaseCode(food.Code);
                    var processingParts = FoodCodeUtilities.GetFoodProcessingParts(food.Code);
                    if (foods.TryGetValue(baseCode, out var result)) {
                        food.BaseFood = getOrAddFood(foods, baseCode);
                        foreach (var processingCode in processingParts) {
                            if (_data.AllProcessingTypes.TryGetValue(processingCode, out ProcessingType item)) {
                                food.ProcessingTypes.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private void resolveFoodEx2Foods(IDictionary<string, Food> foods, params Food[] foodEx2Foods) {
            // parse all foods: look for foodex2 codes
            // create a list of foodEx2 foods from _data.AllFoods
            foreach (var food in foodEx2Foods) {
                if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(food.Code)) {
                    var foodCodePart = FoodCodeUtilities.GetFoodEx2BaseCode(food.Code);
                    if (!string.IsNullOrEmpty(foodCodePart)) {
                        var baseFood = getOrAddFood(foods, foodCodePart, null, false);
                        food.BaseFood = baseFood;
                        food.Parent = baseFood;
                        var facetCodes = FoodCodeUtilities.GetFoodEx2FacetCodes(food.Code);
                        food.FoodFacets = facetCodes.Select(getOrAddFoodFacet).ToList();
                        food.Name = food.Parent.Name + " - " + string.Join(" - ", food.FoodFacets.Select(f => f.Name));
                    }
                }
            }
        }
    }
}
