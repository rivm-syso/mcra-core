using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all individual sets.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualSet> GetAllIndividualSets() {
            if (_data.AllIndividualSets == null) {
                LoadScope(SourceTableGroup.Individuals);
                var allIndividualSets = new Dictionary<string, IndividualSet>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Individuals);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualSets>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var setId = r.GetString(RawIndividualSets.IdSet, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.IndividualSets, setId);
                                    if (valid) {
                                        var set = new IndividualSet {
                                            Code = setId,
                                            Name = r.GetStringOrNull(RawIndividualSets.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawIndividualSets.Description, fieldMap),
                                            BodyWeightUnit = r.GetEnum(RawIndividualSets.BodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                            AgeUnitString = r.GetStringOrNull(RawIndividualSets.AgeUnit, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawIndividualSets.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawIndividualSets.EndDate, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawIndividualSets.IdPopulation, fieldMap),
                                        };
                                        allIndividualSets[set.Code] = set;
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllIndividualSets = allIndividualSets;
            }
            return _data.AllIndividualSets;
        }

        /// <summary>
        /// Gets all individual set individuals.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Individual> GetAllIndividualSetIndividuals() {
            if (_data.AllIndividualSetIndividuals == null) {
                var allIndividualSetIndividuals = new Dictionary<string, Individual>(StringComparer.OrdinalIgnoreCase);
                var allIndividualSetIndividualProperties = new Dictionary<string, IndividualProperty>(StringComparer.OrdinalIgnoreCase);
                var emptyPropertyTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Individuals);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllIndividualSets();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read individuals
                        var id = 0;
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividuals>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyCode = r.GetString(RawIndividuals.IdFoodSurvey, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.IndividualSets, surveyCode);
                                    if (valid) {
                                        var survey = _data.GetOrAddIndividualSet(surveyCode);
                                        var idIndividual = r.GetString(RawIndividuals.IdIndividual, fieldMap);
                                        var individual = new Individual(++id) {
                                            Code = idIndividual,
                                            BodyWeight = r.GetDoubleOrNull(RawIndividuals.BodyWeight, fieldMap) ?? double.NaN,
                                            SamplingWeight = r.GetDoubleOrNull(RawIndividuals.SamplingWeight, fieldMap) ?? 1D,
                                            NumberOfDaysInSurvey = r.GetIntOrNull(RawIndividuals.NumberOfSurveyDays, fieldMap) ?? 0,
                                            CodeFoodSurvey = surveyCode,
                                        };
                                        allIndividualSetIndividuals.Add(individual.Code, individual);
                                        survey.Individuals.Add(individual);
                                    }
                                }
                            }
                        }

                        // Read individual properties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualProperties>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var propertyName = r.GetString(RawIndividualProperties.IdIndividualProperty, fieldMap);
                                    if (!allIndividualSetIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
                                        var name = r.GetStringOrNull(RawIndividualProperties.Name, fieldMap);
                                        if (r.IsDBNull(RawIndividualProperties.Type, fieldMap)) {
                                            emptyPropertyTypes.Add(propertyName);
                                        }
                                        allIndividualSetIndividualProperties[propertyName] = new IndividualProperty {
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

                        // Read individual property values
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualPropertyValues>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idIndividual = r.GetString(RawIndividualPropertyValues.IdIndividual, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.IndividualSetIndividuals, idIndividual);
                                    if (valid) {
                                        var propertyName = r.GetString(RawIndividualPropertyValues.PropertyName, fieldMap);
                                        var individual = allIndividualSetIndividuals[idIndividual];
                                        if (allIndividualSetIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
                                            individual.SetPropertyValue(
                                                individualProperty,
                                                r.GetStringOrNull(RawIndividualPropertyValues.TextValue, fieldMap),
                                                r.GetDoubleOrNull(RawIndividualPropertyValues.DoubleValue, fieldMap)
                                            );
                                        }
                                    }
                                }
                            }
                        }

                        // Set property types (i.e., numeric/categorical) for properties without type
                        foreach (var property in allIndividualSetIndividualProperties) {
                            if (emptyPropertyTypes.Contains(property.Key)) {
                                var individualPropertyValues = allIndividualSetIndividuals.Values
                                    .Where(r => r.IndividualPropertyValues.Any(c => c.IndividualProperty == property.Value))
                                    .Select(r => r.IndividualPropertyValues.First(c => c.IndividualProperty == property.Value))
                                    .ToList();
                                property.Value.PropertyType = individualPropertyValues.All(ipv => ipv.IsNumeric())
                                    ? IndividualPropertyType.Numeric
                                    : IndividualPropertyType.Categorical;
                            }
                        }
                    }
                }

                _data.AllIndividualSetIndividuals = allIndividualSetIndividuals;
                _data.AllIndividualSetIndividualProperties = allIndividualSetIndividualProperties;
            }
            return _data.AllIndividualSetIndividuals;
        }

        /// <summary>
        /// Gets all individual properties.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> GetAllIndividualSetIndividualProperties() {
            if (_data.AllIndividualSetIndividualProperties == null) {
                GetAllIndividualSetIndividuals();
            }
            return _data.AllIndividualSetIndividualProperties;
        }

    }
}
