using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        #region Methods

        /// <summary>
        /// All food samples from the compiled data source.
        /// </summary>
        public IDictionary<string, FoodSample> GetAllFoodSamples() {
            if (_data.AllFoodSamples == null) {
                LoadScope(SourceTableGroup.Concentrations);

                GetAllFoods();
                GetAllCompounds();
                loadAllSampleProperties();
                var allAdditionalSampleProperties = _data.AllAdditionalSampleProperties;
                var allAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(StringComparer.OrdinalIgnoreCase);
                var allFoodSamples = new Dictionary<string, FoodSample>(StringComparer.OrdinalIgnoreCase);
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    fillAnalyticalMethods(rdm, allAnalyticalMethods, SourceTableGroup.Concentrations, ScopingType.AnalyticalMethods);
                    fillSamples(
                        rdm,
                        SourceTableGroup.Concentrations,
                        allAdditionalSampleProperties,
                        allAnalyticalMethods,
                        ScopingType.FoodSamples,
                        ScopingType.SampleAnalyses,
                        ScopingType.AnalyticalMethods,
                        allFoodSamples
                    );
                }
                _data.AllFoodSamples = allFoodSamples;
                _data.AllAnalyticalMethods = allAnalyticalMethods;
                _data.AllAdditionalSampleProperties = allAdditionalSampleProperties;
            }
            return _data.AllFoodSamples;
        }

        /// <summary>
        /// Returns all other sample locations of the compiled datasource.
        /// </summary>
        public IDictionary<string, SampleProperty> GetAllAdditionalSampleProperties() {
            loadAllSampleProperties();
            return _data.AllAdditionalSampleProperties;
        }

        /// <summary>
        /// Returns all sample years.
        /// </summary>
        /// <returns></returns>
        public ICollection<int> GetAllSampleYears() {
            loadAllSampleProperties();
            return _data.AllSampleYears;
        }

        /// <summary>
        /// Returns all sample locations.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSampleLocations() {
            loadAllSampleProperties();
            return _data.AllSampleLocations;
        }

        /// <summary>
        /// Returns all sample regions.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSampleRegions() {
            loadAllSampleProperties();
            return _data.AllSampleRegions;
        }

        /// <summary>
        /// Returns all sample production methods.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSampleProductionMethods() {
            loadAllSampleProperties();
            return _data.AllSampleProductionMethods;
        }

        /// <summary>
        /// Returns all distinct analytical methods of all samples of the compiled data source.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> GetAllAnalyticalMethods() {
            if (_data.AllAnalyticalMethods == null) {
                GetAllFoodSamples();
            }
            return _data.AllAnalyticalMethods;
        }

        private void fillSamples(
            IRawDataManager rdm,
            SourceTableGroup tableGroup,
            IDictionary<string, SampleProperty> additionalFoodSampleProperties,
            IDictionary<string, AnalyticalMethod> allAnalyticalMethods,
            ScopingType scopingTypeFoodSamples,
            ScopingType scopingTypeSampleAnalyses,
            ScopingType scopingTypeAnalyticalMethods,
            IDictionary<string, FoodSample> foodSamples
        ) {
            var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(tableGroup);
            if (rawDataSourceIds?.Any() ?? false) {
                var amSampleCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var sampleAnalyses = new Dictionary<string, SampleAnalysis>(StringComparer.OrdinalIgnoreCase);

                // Read food samples
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawFoodSamples>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idFood = r.GetString(RawFoodSamples.IdFood, fieldMap);
                            var idFoodSample = r.GetString(RawFoodSamples.IdFoodSample, fieldMap);
                            var valid = CheckLinkSelected(ScopingType.Foods, idFood);
                            if (valid) {
                                var food = getOrAddFood(idFood);
                                foodSamples.Add(
                                    r.GetString(RawFoodSamples.IdFoodSample, fieldMap),
                                    new FoodSample {
                                        Code = idFoodSample,
                                        Food = food,
                                        Location = r.GetStringOrNull(RawFoodSamples.Location, fieldMap),
                                        Region = r.GetStringOrNull(RawFoodSamples.Region, fieldMap),
                                        DateSampling = r.GetDateTimeOrNull(RawFoodSamples.DateSampling, fieldMap),
                                        ProductionMethod = r.GetStringOrNull(RawFoodSamples.ProductionMethod, fieldMap),
                                        Name = r.GetStringOrNull(RawFoodSamples.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawFoodSamples.Description, fieldMap),
                                        SampleAnalyses = [],
                                    }
                                );
                            }
                        }
                    }
                }

                // Read additional sample properties
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawSamplePropertyValues>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idFoodSample = r.GetString(RawSamplePropertyValues.IdSample, fieldMap);
                            var valid = CheckLinkSelected(ScopingType.FoodSamples, idFoodSample);
                            if (foodSamples.TryGetValue(idFoodSample, out var foodSample)) {
                                var propertyName = r.GetString(RawSamplePropertyValues.PropertyName, fieldMap);
                                if (!additionalFoodSampleProperties.TryGetValue(propertyName, out var sampleProperty)) {
                                    additionalFoodSampleProperties[propertyName] = sampleProperty = new SampleProperty {
                                        Name = r.GetString(RawSamplePropertyValues.PropertyName, fieldMap),
                                        SamplePropertyValues = []
                                    };
                                }
                                var propertyValue = new SamplePropertyValue {
                                    SampleProperty = sampleProperty,
                                    TextValue = r.GetStringOrNull(RawSamplePropertyValues.TextValue, fieldMap),
                                    DoubleValue = r.GetDoubleOrNull(RawSamplePropertyValues.DoubleValue, fieldMap)
                                };
                                sampleProperty.SamplePropertyValues.Add(propertyValue);
                                foodSample.SampleProperties.Add(sampleProperty, propertyValue);
                            }
                        }
                    }
                }

                // Read sample analyses, combine with just read food samples to fill AllSamples
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawAnalysisSamples>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idFoodSample = r.GetString(RawAnalysisSamples.IdFoodSample, fieldMap);
                            var idMethod = r.GetString(RawAnalysisSamples.IdAnalyticalMethod, fieldMap);
                            var valid = CheckLinkSelected(scopingTypeFoodSamples, idFoodSample)
                                      & CheckLinkSelected(scopingTypeAnalyticalMethods, idMethod);
                            if (valid) {
                                var idAnalysisSample = r.GetString(RawAnalysisSamples.IdAnalysisSample, fieldMap);
                                var foodSample = foodSamples[idFoodSample];
                                var food = getOrAddFood(foodSample.Food.Code);
                                var analyticalMethod = getOrAddAnalyticalMethod(allAnalyticalMethods, idMethod);
                                var sampleAnalysis = new SampleAnalysis {
                                    Code = idAnalysisSample,
                                    AnalyticalMethod = analyticalMethod,
                                    AnalysisDate = r.GetDateTimeOrNull(RawAnalysisSamples.DateAnalysis, fieldMap),
                                    Name = r.GetStringOrNull(RawAnalysisSamples.Name, fieldMap),
                                    Description = r.GetStringOrNull(RawAnalysisSamples.Description, fieldMap),
                                };

                                if (sampleAnalyses.ContainsKey(sampleAnalysis.Code)) {
                                    var msg = $"Duplicate key '{sampleAnalysis.Code}' found in the table 'AnalysisSamples', " +
                                               "the 'idAnalysisSample' column should contain unique values. " +
                                               "Please correct the table in the original data file and upload it again.";
                                    throw new Exception(msg);
                                }

                                sampleAnalyses.Add(sampleAnalysis.Code, sampleAnalysis);
                                foodSample.SampleAnalyses.Add(sampleAnalysis);
                                if (!amSampleCounts.ContainsKey(analyticalMethod.Code)) {
                                    amSampleCounts[analyticalMethod.Code] = 1;
                                } else {
                                    amSampleCounts[analyticalMethod.Code]++;
                                }
                            }
                        }
                        foreach (var n in amSampleCounts) {
                            allAnalyticalMethods[n.Key].SampleCount += n.Value;
                        }
                    }
                }

                // Read sample analysis concentrations
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawConcentrationsPerSample>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idSample = r.GetString(RawConcentrationsPerSample.IdAnalysisSample, fieldMap);
                            var idSubstance = r.GetString(RawConcentrationsPerSample.IdCompound, fieldMap);
                            var valid = CheckLinkSelected(scopingTypeSampleAnalyses, idSample)
                                      & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                            if (valid) {
                                var sample = sampleAnalyses[idSample];
                                var substance = _data.GetOrAddSubstance(idSubstance);
                                var resTypeString = r.GetStringOrNull(RawConcentrationsPerSample.ResType, fieldMap);
                                var concentration = r.GetDoubleOrNull(RawConcentrationsPerSample.Concentration, fieldMap);
                                var isMissing = !concentration.HasValue || double.IsNaN(concentration.Value);
                                var resType = ResTypeConverter.FromString(
                                    resTypeString,
                                    isMissing ? ResType.MV : ResType.VAL
                                );

                                // Check for substance in analyticalmethod compounds
                                if (sample.AnalyticalMethod.AnalyticalMethodCompounds.ContainsKey(substance)) {
                                    var c = new ConcentrationPerSample {
                                        Sample = sample,
                                        Compound = substance,
                                        Concentration = concentration,
                                        ResType = resType
                                    };
                                    sample.Concentrations[substance] = c;
                                } else {
                                    var msg = $"Substance '{substance.Code}' for sample '{sample.Code}' not found " +
                                              $"in the analytical method substances table for analytical method '{sample.AnalyticalMethod.Code}'. " +
                                               "Please correct the table in the original data file and upload it again.";
                                    throw new KeyNotFoundException(msg);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void fillAnalyticalMethods(
            IRawDataManager rdm,
            IDictionary<string, AnalyticalMethod> analyticalMethods,
            SourceTableGroup tableGroup,
            ScopingType scopingTypeAnalyticalMethods
        ) {
            var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(tableGroup);
            if (rawDataSourceIds?.Any() ?? false) {
                // Read analytical methods
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawAnalyticalMethods>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idAnalyticalMethod = r.GetString(RawAnalyticalMethods.IdAnalyticalMethod, fieldMap);
                            var valid = IsCodeSelected(scopingTypeAnalyticalMethods, idAnalyticalMethod);
                            if (valid) {
                                var am = new AnalyticalMethod {
                                    Code = idAnalyticalMethod,
                                    Description = r.GetStringOrNull(RawAnalyticalMethods.Description, fieldMap),
                                    Name = r.GetStringOrNull(RawAnalyticalMethods.Name, fieldMap)
                                };
                                analyticalMethods.Add(am.Code, am);
                            }
                        }
                    }
                }

                // Add items by code from the scope where no matched items were found in the source
                var readingScope = GetCodesInScope(scopingTypeAnalyticalMethods);
                foreach (var code in readingScope.Except(analyticalMethods.Keys)) {
                    analyticalMethods[code] = new AnalyticalMethod { Code = code };
                }

                // Read analytical method substances
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawAnalyticalMethodCompounds>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var amCode = r.GetString(RawAnalyticalMethodCompounds.IdAnalyticalMethod, fieldMap);
                            var idSubstance = r.GetString(RawAnalyticalMethodCompounds.IdCompound, fieldMap);
                            var valid = CheckLinkSelected(scopingTypeAnalyticalMethods, amCode)
                                      & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                            if (valid) {
                                var method = getOrAddAnalyticalMethod(analyticalMethods, amCode);
                                var substance = _data.GetOrAddSubstance(idSubstance);
                                var lod = r.GetDoubleOrNull(RawAnalyticalMethodCompounds.LOD, fieldMap);
                                var loq = r.GetDoubleOrNull(RawAnalyticalMethodCompounds.LOQ, fieldMap);
                                var unitString = r.GetStringOrNull(RawAnalyticalMethodCompounds.ConcentrationUnit, fieldMap);
                                var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.mgPerKg);
                                var amc = new AnalyticalMethodCompound {
                                    AnalyticalMethod = method,
                                    Compound = substance,
                                    LOD = lod ?? double.NaN,
                                    LOQ = loq ?? double.NaN,
                                    ConcentrationUnit = unit
                                };
                                method.AnalyticalMethodCompounds[substance] = amc;
                            }
                        }
                    }
                }
            }
        }

        private static AnalyticalMethod getOrAddAnalyticalMethod(IDictionary<string, AnalyticalMethod> analyticalMethods, string id, string description = null) {
            if (!analyticalMethods.TryGetValue(id, out AnalyticalMethod item)) {
                item = new AnalyticalMethod { Code = id, Description = description };
                analyticalMethods.Add(id, item);
            }
            return item;
        }

        private void loadAllSampleProperties() {
            if (_data.AllSampleYears == null
                || _data.AllSampleLocations == null
                || _data.AllSampleRegions == null
                || _data.AllSampleProductionMethods == null
            ) {
                var allSampleYears = new HashSet<int>();
                var allSampleLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var allSampleRegions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var allSampleProductionMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var allAdditionalSampleProperties = _data.AllAdditionalSampleProperties
                    ?? new Dictionary<string, SampleProperty>(StringComparer.OrdinalIgnoreCase);
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    //create function for reading sample locations and years
                    Func<ICollection<int>, bool> readSamplesFunction = (ids) => {
                        if (ids?.Any() ?? false) {
                            foreach (var id in ids) {

                                // Read sample years
                                using (var r = rdm.OpenDataReader<RawSampleYears>(id, out int[] fieldMap)) {
                                    while (r?.Read() ?? false) {
                                        var sampleYear = r.GetIntOrNull(RawSampleYears.Year, fieldMap);
                                        if (sampleYear.HasValue && !allSampleYears.Contains(sampleYear.Value)) {
                                            allSampleYears.Add(sampleYear.Value);
                                        }
                                    }
                                }

                                // Read sample locations
                                using (var r = rdm.OpenDataReader<RawSampleLocations>(id, out int[] fieldMap)) {
                                    while (r?.Read() ?? false) {
                                        var location = r.GetString(RawSampleLocations.Location, fieldMap);
                                        if (!string.IsNullOrWhiteSpace(location) && !allSampleLocations.Contains(location)) {
                                            allSampleLocations.Add(location);
                                        }
                                    }
                                }

                                // Read sample regions
                                using (var r = rdm.OpenDataReader<RawSampleRegions>(id, out int[] fieldMap)) {
                                    while (r?.Read() ?? false) {
                                        var region = r.GetString(RawSampleRegions.Region, fieldMap);
                                        if (!string.IsNullOrWhiteSpace(region) && !allSampleRegions.Contains(region)) {
                                            allSampleRegions.Add(region);
                                        }
                                    }
                                }

                                // Read production methods
                                using (var r = rdm.OpenDataReader<RawSampleProductionMethods>(id, out int[] fieldMap)) {
                                    while (r?.Read() ?? false) {
                                        var productionMethods = r.GetString(RawSampleProductionMethods.ProductionMethod, fieldMap);
                                        if (!string.IsNullOrWhiteSpace(productionMethods) && !allSampleProductionMethods.Contains(productionMethods)) {
                                            allSampleProductionMethods.Add(productionMethods);
                                        }
                                    }
                                }

                                // Read additional sample properties
                                using (var r = rdm.OpenDataReader<RawSamplePropertyValues>(id, out int[] fieldMap)) {
                                    while (r?.Read() ?? false) {
                                        var propertyName = r.GetString(RawSamplePropertyValues.PropertyName, fieldMap);
                                        if (!allAdditionalSampleProperties.TryGetValue(propertyName, out var sampleProperty)) {
                                            allAdditionalSampleProperties[propertyName] = sampleProperty = new SampleProperty {
                                                Name = r.GetString(RawSamplePropertyValues.PropertyName, fieldMap),
                                                SamplePropertyValues = new HashSet<SamplePropertyValue>()
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
                            return true;
                        }
                        return false;
                    };
                    readSamplesFunction.Invoke(_rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Concentrations));
                }
                _data.AllSampleYears = allSampleYears;
                _data.AllSampleLocations = allSampleLocations;
                _data.AllSampleRegions = allSampleRegions;
                _data.AllSampleProductionMethods = allSampleProductionMethods;
                _data.AllAdditionalSampleProperties = allAdditionalSampleProperties;
            }
        }

        private static void writeAnalyticalMethodsToCsv(string tempFolder, IEnumerable<AnalyticalMethod> analyticalMethods) {
            if (!analyticalMethods?.Any() ?? true) {
                return;
            }

            var tdam = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AnalyticalMethods);
            var dtam = tdam.CreateDataTable();
            var tdmc = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AnalyticalMethodCompounds);
            var dtmc = tdmc.CreateDataTable();

            var ccram = new int[Enum.GetNames(typeof(RawAnalyticalMethods)).Length];
            var ccrmc = new int[Enum.GetNames(typeof(RawAnalyticalMethodCompounds)).Length];

            foreach (var am in analyticalMethods) {
                var rowam = dtam.NewRow();
                rowam.WriteNonEmptyString(RawAnalyticalMethods.IdAnalyticalMethod, am.Code, ccram);
                rowam.WriteNonEmptyString(RawAnalyticalMethods.Description, am.Description, ccram);

                dtam.Rows.Add(rowam);

                foreach (var amc in am.AnalyticalMethodCompounds.Values) {
                    var rowmc = dtmc.NewRow();
                    rowmc.WriteNonEmptyString(RawAnalyticalMethodCompounds.IdAnalyticalMethod, am.Code, ccrmc);
                    rowmc.WriteNonEmptyString(RawAnalyticalMethodCompounds.IdCompound, amc.Compound.Code, ccrmc);
                    rowmc.WriteNonNaNDouble(RawAnalyticalMethodCompounds.LOD, amc.LOD, ccrmc);
                    rowmc.WriteNonNaNDouble(RawAnalyticalMethodCompounds.LOQ, amc.LOQ, ccrmc);
                    rowmc.WriteNonEmptyString(RawAnalyticalMethodCompounds.ConcentrationUnit, amc.ConcentrationUnit.ToString(), ccrmc);
                    dtmc.Rows.Add(rowmc);
                }
            }

            writeToCsv(tempFolder, tdam, dtam, ccram);
            writeToCsv(tempFolder, tdmc, dtmc, ccrmc);
        }

        private static void writeAdditionalSamplePropertiesToCsv(string tempFolder, IEnumerable<SampleProperty> additionalSampleProperties) {
            if (!additionalSampleProperties?.Any() ?? true) {
                return;
            }

            var tdsp = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.SampleProperties);
            var dtsp = tdsp.CreateDataTable();
            foreach (var sp in additionalSampleProperties) {
                foreach (var sampleProperty in additionalSampleProperties) {
                    var row = dtsp.NewRow();
                    row.WriteNonEmptyString(RawSampleProperties.Name, sampleProperty.Name);
                    row.WriteNonEmptyString(RawSampleProperties.Description, sampleProperty.Description);
                }
            }
            writeToCsv(tempFolder, tdsp, dtsp);
        }

        private static void writeFoodSamplesToCsv(string tempFolder, IEnumerable<FoodSample> foodSamples) {
            if (!foodSamples?.Any() ?? true) {
                return;
            }

            var tdfs = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodSamples);
            var dtfs = tdfs.CreateDataTable();
            var tdas = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AnalysisSamples);
            var dtas = tdas.CreateDataTable();
            var tdspv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.SamplePropertyValues);
            var dtspv = tdspv.CreateDataTable();

            var tdc = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConcentrationsPerSample);
            var dtc = tdc.CreateDataTable();

            var ccrfs = new int[Enum.GetNames(typeof(RawFoodSamples)).Length];
            var ccras = new int[Enum.GetNames(typeof(RawAnalysisSamples)).Length];

            //keep foodsamples in separately keyed dictionary
            var foodSampleCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            //all samples go into analysis samples table
            foreach (var s in foodSamples) {
                //Add the food samples based on the IdLargeSample
                //or code of the sample itself if it is null
                var codeFoodSample = s.Code;
                if (!foodSampleCodes.Contains(codeFoodSample)) {
                    var rowfs = dtfs.NewRow();
                    rowfs.WriteNonEmptyString(RawFoodSamples.IdFoodSample, codeFoodSample, ccrfs);
                    rowfs.WriteNonEmptyString(RawFoodSamples.IdFood, s.Food.Code, ccrfs);
                    dtfs.Rows.Add(rowfs);
                    foodSampleCodes.Add(codeFoodSample);
                }

                foreach (var samplePropertyValue in s.SampleProperties) {
                    var rowspv = dtspv.NewRow();
                    rowspv.WriteNonEmptyString(RawSamplePropertyValues.IdSample, codeFoodSample);
                    rowspv.WriteNonEmptyString(RawSamplePropertyValues.PropertyName, samplePropertyValue.Key.Name);
                    rowspv.WriteNonNullDouble(RawSamplePropertyValues.DoubleValue, samplePropertyValue.Value.DoubleValue);
                    rowspv.WriteNonEmptyString(RawSamplePropertyValues.TextValue, samplePropertyValue.Value.TextValue);
                }

                foreach (var sampleAnalysis in s.SampleAnalyses) {
                    var row = dtas.NewRow();
                    row.WriteNonEmptyString(RawAnalysisSamples.IdAnalysisSample, sampleAnalysis.Code, ccras);
                    row.WriteNonEmptyString(RawAnalysisSamples.IdFoodSample, codeFoodSample, ccras);
                    row.WriteNonEmptyString(RawAnalysisSamples.IdAnalyticalMethod, sampleAnalysis.AnalyticalMethod?.Code, ccras);
                    row.WriteNonNullDateTime(RawAnalysisSamples.DateAnalysis, sampleAnalysis.AnalysisDate, ccras);
                    dtas.Rows.Add(row);

                    foreach (var conc in sampleAnalysis.Concentrations) {
                        var rowc = dtc.NewRow();
                        rowc.WriteNonEmptyString(RawConcentrationsPerSample.IdAnalysisSample, sampleAnalysis.Code);
                        rowc.WriteNonEmptyString(RawConcentrationsPerSample.IdCompound, conc.Key.Code);
                        rowc.WriteNonNullDouble(RawConcentrationsPerSample.Concentration, conc.Value.Concentration);
                        rowc.WriteNonEmptyString(RawConcentrationsPerSample.ResType, conc.Value.ResType.ToString());
                        dtc.Rows.Add(rowc);
                    }
                }
            }
            writeToCsv(tempFolder, tdfs, dtfs, ccrfs);
            writeToCsv(tempFolder, tdas, dtas, ccras);
            writeToCsv(tempFolder, tdc, dtc);
            writeToCsv(tempFolder, tdspv, dtspv);
        }
    }

    #endregion
}

