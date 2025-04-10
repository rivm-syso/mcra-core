using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all populations.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Population> GetAllPopulations() {
            if (_data.AllPopulations == null) {
                LoadScope(SourceTableGroup.Populations);
                var allPopulations = new Dictionary<string, Population>(StringComparer.OrdinalIgnoreCase);
                var allPopulationIndividualProperties = new Dictionary<string, IndividualProperty>(StringComparer.OrdinalIgnoreCase);
                var emptyPropertyTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Populations);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawPopulations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idPopulation = r.GetString(RawPopulations.IdPopulation, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.Populations, idPopulation);
                                    if (valid) {
                                        var population = new Population {
                                            Code = idPopulation,
                                            Name = r.GetStringOrNull(RawPopulations.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawPopulations.Description, fieldMap),
                                            Location = r.GetStringOrNull(RawPopulations.Location, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawPopulations.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawPopulations.EndDate, fieldMap),
                                            NominalBodyWeight = r.GetDoubleOrNull(RawPopulations.NominalBodyWeight, fieldMap) ?? double.NaN,
                                            Size = r.GetDoubleOrNull(RawPopulations.Size, fieldMap) ?? double.NaN,
                                            PopulationIndividualPropertyValues = [],
                                        };
                                        allPopulations[population.Code] = population;

                                        getHardWiredProperties(
                                            allPopulations,
                                            allPopulationIndividualProperties,
                                            idPopulation,
                                            population
                                        );
                                    }
                                }
                            }
                        }

                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualProperties>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var propertyName = r.GetString(RawIndividualProperties.IdIndividualProperty, fieldMap);
                                    if (!allPopulationIndividualProperties.TryGetValue(propertyName, out IndividualProperty property)) {
                                        var name = r.GetStringOrNull(RawIndividualProperties.Name, fieldMap);
                                        if (r.IsDBNull(RawIndividualProperties.Type, fieldMap)) {
                                            emptyPropertyTypes.Add(propertyName);
                                        }
                                        allPopulationIndividualProperties[propertyName] = new IndividualProperty {
                                            Code = propertyName,
                                            Name = !string.IsNullOrEmpty(name) ? name : propertyName,
                                            Description = r.GetStringOrNull(RawIndividualProperties.Description, fieldMap),
                                            PropertyLevel = r.GetEnum<PropertyLevelType>(RawIndividualProperties.PropertyLevel, fieldMap),
                                            PropertyType = r.GetEnum<IndividualPropertyType>(RawIndividualProperties.Type, fieldMap),
                                        };
                                    }
                                }
                            }
                        }

                        HashSet<IndividualPropertyType> controlledPropertyTypes = [
                            IndividualPropertyType.Boolean,
                            IndividualPropertyType.Gender,
                            IndividualPropertyType.Month
                        ];

                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawPopulationIndividualPropertyValues>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idPopulation = r.GetString(RawPopulationIndividualPropertyValues.IdPopulation, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Populations, idPopulation);
                                    if (valid) {
                                        var individualProperty = r.GetString(RawPopulationIndividualPropertyValues.IdIndividualProperty, fieldMap);
                                        if (allPopulationIndividualProperties.TryGetValue(individualProperty, out IndividualProperty property)) {
                                            var value = r.GetStringOrNull(RawPopulationIndividualPropertyValues.Value, fieldMap);
                                            var propertyValue = new PopulationIndividualPropertyValue {
                                                IndividualProperty = property,
                                                Value = value,
                                                MinValue = r.GetDoubleOrNull(RawPopulationIndividualPropertyValues.MinValue, fieldMap),
                                                MaxValue = r.GetDoubleOrNull(RawPopulationIndividualPropertyValues.MaxValue, fieldMap),
                                                StartDate = r.GetDateTimeOrNull(RawPopulationIndividualPropertyValues.StartDate, fieldMap),
                                                EndDate = r.GetDateTimeOrNull(RawPopulationIndividualPropertyValues.EndDate, fieldMap)
                                            };
                                            //check controlled terminology for categorical levels that should be converted
                                            //from any alias
                                            if (!string.IsNullOrEmpty(value) && controlledPropertyTypes.Contains(property.PropertyType)) {
                                                var catLevels = propertyValue.CategoricalLevels
                                                    .Select(x => {
                                                        var cv = property.PropertyType switch {
                                                            IndividualPropertyType.Month => MonthTypeConverter.FromString(x).ToString(),
                                                            IndividualPropertyType.Gender => GenderTypeConverter.FromString(x).ToString(),
                                                            IndividualPropertyType.Boolean => BooleanTypeConverter.FromString(x).ToString(),
                                                            _ => x
                                                        };
                                                        return cv;
                                                    })
                                                    .ToHashSet();
                                                //replace with converted levels
                                                propertyValue.CategoricalLevels = catLevels;
                                            }

                                            if (!allPopulations[idPopulation].PopulationIndividualPropertyValues.ContainsKey(individualProperty)) {
                                                allPopulations[idPopulation].PopulationIndividualPropertyValues[individualProperty] = propertyValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Set property types (i.e., numeric/categorical) for properties without type
                        foreach (var property in allPopulationIndividualProperties) {
                            if (emptyPropertyTypes.Contains(property.Key)) {
                                var individualPropertyValues = allPopulations.Values
                                    .Where(r => r.PopulationIndividualPropertyValues.ContainsKey(property.Key))
                                    .Select(r => r.PopulationIndividualPropertyValues[property.Key])
                                    .ToList();
                                property.Value.PropertyType = individualPropertyValues.All(ipv => ipv.IsNumeric()) ? IndividualPropertyType.Numeric : IndividualPropertyType.Categorical;
                            }
                        }

                        // Read population characteristics
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawPopulationCharacteristics>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idPopulation = r.GetString(RawPopulationCharacteristics.IdPopulation, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.Populations, idPopulation);
                                        if (valid && allPopulations.TryGetValue(idPopulation, out var population)) {
                                            var record = new PopulationCharacteristic {
                                                idPopulation = idPopulation,
                                                Characteristic = r.GetEnum<PopulationCharacteristicType>(RawPopulationCharacteristics.Characteristic, fieldMap),
                                                Unit = r.GetStringOrNull(RawPopulationCharacteristics.Unit, fieldMap),
                                                Value = r.GetDouble(RawPopulationCharacteristics.Value, fieldMap),
                                                DistributionType = r.GetEnum(RawPopulationCharacteristics.PopulationCharacteristicDistributionType, fieldMap, PopulationCharacteristicDistributionType.Constant),
                                                CvVariability = r.GetDoubleOrNull(RawPopulationCharacteristics.CvVariability, fieldMap),
                                            };

                                            population.PopulationCharacteristics.Add(record);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Add items by code from the scope where no matched items were found in the source
                    foreach (var code in GetCodesInScope(ScopingType.Populations).Except(allPopulations.Keys, StringComparer.OrdinalIgnoreCase)) {
                        allPopulations[code] = new Population { Code = code };
                    }
                }
                _data.AllPopulations = allPopulations;
                _data.AllPopulationIndividualProperties = allPopulationIndividualProperties;
            }
            return _data.AllPopulations;
        }

        private static IndividualProperty _locationIndividualProperty = new() {
            Code = IndividualPropertyType.Location.ToString(),
            Name = IndividualPropertyType.Location.ToString(),
            Description = IndividualPropertyType.Location.ToString(),
            PropertyType = IndividualPropertyType.Location,
            PropertyLevel = PropertyLevelType.Individual
        };

        private static IndividualProperty _dateTimeIndividualProperty = new() {
            Code = IndividualPropertyType.DateTime.ToString(),
            Name = IndividualPropertyType.DateTime.ToString(),
            Description = IndividualPropertyType.DateTime.ToString(),
            PropertyType = IndividualPropertyType.DateTime,
            PropertyLevel = PropertyLevelType.IndividualDay
        };

        private static void getHardWiredProperties(
            Dictionary<string, Population> allPopulations,
            Dictionary<string, IndividualProperty> allPopulationIndividualProperties,
            string idPopulation,
            Population population
        ) {
            if (!string.IsNullOrEmpty(population.Location)) {
                var property = _locationIndividualProperty;
                allPopulationIndividualProperties[property.Code] = property;
                var populationIndividualPropertyValue = new PopulationIndividualPropertyValue {
                    IndividualProperty = property,
                    Value = population.Location,
                };
                allPopulations[idPopulation].PopulationIndividualPropertyValues[property.Code] = populationIndividualPropertyValue;
            }
            if (population.StartDate != null && population.EndDate != null) {
                var property = _dateTimeIndividualProperty;
                allPopulationIndividualProperties[property.Code] = property;
                var populationIndividualPropertyValue = new PopulationIndividualPropertyValue {
                    IndividualProperty = property,
                    StartDate = population.StartDate,
                    EndDate = population.EndDate,
                };
                allPopulations[idPopulation].PopulationIndividualPropertyValues[property.Code] = populationIndividualPropertyValue;
            }
        }

        /// <summary>
        /// ZIP file populationsIndividualPropertyValues IS NOT FINISHED (UNIT TEST CompiledDataManager_WriteShallowDataObjectsZipFileTest)
        /// </summary>
        /// <param name="tempFolder"></param>
        /// <param name="list"></param>
        private static void writePopulationDataToCsv(string tempFolder, IEnumerable<Population> list) {
            if (!list?.Any() ?? true) {
                return;
            }

            var tdp = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Populations);
            var dtp = tdp.CreateDataTable();

            var tdpipv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.PopulationIndividualPropertyValues);
            var dtpipv = tdpipv.CreateDataTable();

            foreach (var item in list) {
                var row = dtp.NewRow();

                row.WriteNonEmptyString(RawPopulations.IdPopulation, item.Code);
                row.WriteNonEmptyString(RawPopulations.Name, item.Name);
                row.WriteNonEmptyString(RawPopulations.Description, item.Description);
                row.WriteNonEmptyString(RawPopulations.Location, item.Location);
                row.WriteNonNullDateTime(RawPopulations.StartDate, item.StartDate);
                row.WriteNonNullDateTime(RawPopulations.EndDate, item.EndDate);
                row.WriteNonNullDouble(RawPopulations.NominalBodyWeight, item.NominalBodyWeight);
                row.WriteNonNullDouble(RawPopulations.Size, item.Size);

                dtp.Rows.Add(row);
                //population individual property values
                //TODO this is not finished
                if (item.PopulationIndividualPropertyValues != null) {

                    foreach (var prop in item.PopulationIndividualPropertyValues) {
                        //property values per individual
                        var rowpv = dtpipv.NewRow();
                        rowpv.WriteNonEmptyString(RawPopulationIndividualPropertyValues.IdIndividualProperty, prop.Key);
                        rowpv.WriteNonEmptyString(RawPopulationIndividualPropertyValues.IdPopulation, item.Code);
                        rowpv.WriteNonNullDouble(RawPopulationIndividualPropertyValues.MaxValue, prop.Value.MaxValue);
                        rowpv.WriteNonNullDouble(RawPopulationIndividualPropertyValues.MinValue, prop.Value.MinValue);
                        //rowpv.WriteNonNullDateTime(RawPopulationIndividualPropertyValues.StartDate, prop.Value.StartDate);
                        //rowpv.WriteNonNullDateTime(RawPopulationIndividualPropertyValues.EndDate, prop.Value.EndDate);
                        rowpv.WriteNonEmptyString(RawPopulationIndividualPropertyValues.Value, prop.Value.Value);
                        dtpipv.Rows.Add(rowpv);
                    }
                } else {
                    var rowpv = dtpipv.NewRow();
                    rowpv.WriteNonEmptyString(RawPopulations.IdPopulation, item.Code);
                    dtpipv.Rows.Add(rowpv);
                }
            }
            writeToCsv(tempFolder, tdp, dtp);
            writeToCsv(tempFolder, tdpipv, dtpipv);
        }
    }
}
