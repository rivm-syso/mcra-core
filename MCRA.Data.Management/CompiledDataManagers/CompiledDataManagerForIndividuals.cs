using MCRA.Data.Compiled.Interfaces;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    /// <summary>
    /// Implements common logic for retrieving individuals and individual properties. Used by several 
    /// compiled data manager parts.
    /// </summary>
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all individual set individuals.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Individual> GetIndividuals<T>(
            SourceTableGroup sourceTableGroup,
            ScopingType linkedScopingType,
            Func<string, string, T> getOrAddIndividualCollection,
            Dictionary<string, int> collectionNumberOfDaysInSurveys = null
        ) where T : IIndividualCollection {
            var allIndividuals = new Dictionary<string, Individual>(StringComparer.OrdinalIgnoreCase);
            var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(sourceTableGroup);
            if (rawDataSourceIds?.Count > 0) {
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    var id = 0;
                    foreach (var rawDataSourceId in rawDataSourceIds) {
                        using (var r = rdm.OpenDataReader<RawIndividuals>(rawDataSourceId, out int[] fieldMap)) {
                            while (r?.Read() ?? false) {
                                var collectionCode = r.GetString(RawIndividuals.IdFoodSurvey, fieldMap);
                                var valid = CheckLinkSelected(linkedScopingType, collectionCode);
                                if (valid) {
                                    var collection = getOrAddIndividualCollection(collectionCode, "");
                                    var idIndividual = r.GetString(RawIndividuals.IdIndividual, fieldMap);
                                    var collectionNumberOfDays = collectionNumberOfDaysInSurveys != null && 
                                        collectionNumberOfDaysInSurveys.TryGetValue(collectionCode, out int defDays)
                                        ? defDays
                                        : 1;
                                    var individual = new Individual(++id) {
                                        Code = idIndividual,
                                        BodyWeight = r.GetDoubleOrNull(RawIndividuals.BodyWeight, fieldMap) ?? double.NaN,
                                        SamplingWeight = r.GetDoubleOrNull(RawIndividuals.SamplingWeight, fieldMap) ?? 1D,
                                        NumberOfDaysInSurvey = r.GetIntOrNull(RawIndividuals.NumberOfSurveyDays, fieldMap) 
                                            ?? collectionNumberOfDays,
                                        CodeFoodSurvey = collectionCode,
                                    };
                                    allIndividuals.Add(individual.Code, individual);
                                    collection.Individuals.Add(individual);
                                }
                            }
                        }
                    }
                }
            }
            return allIndividuals;
        }

        /// <summary>
        /// Gets all individual properties.
        /// </summary>
        public IDictionary<string, IndividualProperty> GetIndividualProperties(
            SourceTableGroup sourceTableGroup,
            ScopingType linkedScopingTypeIndividuals,
            IDictionary<string, Individual> allIndividuals
        ) {
            var allIndividualProperties = new Dictionary<string, IndividualProperty>(StringComparer.OrdinalIgnoreCase);
            var emptyPropertyTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(sourceTableGroup);
            if (rawDataSourceIds?.Count > 0) {
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    // Read individual properties
                    foreach (var rawDataSourceId in rawDataSourceIds) {
                        using (var r = rdm.OpenDataReader<RawIndividualProperties>(rawDataSourceId, out int[] fieldMap)) {
                            while (r?.Read() ?? false) {
                                var propertyName = r.GetString(RawIndividualProperties.IdIndividualProperty, fieldMap);
                                if (!allIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
                                    var name = r.GetStringOrNull(RawIndividualProperties.Name, fieldMap);
                                    if (r.IsDBNull(RawIndividualProperties.Type, fieldMap)) {
                                        emptyPropertyTypes.Add(propertyName);
                                    }
                                    allIndividualProperties[propertyName] = new IndividualProperty {
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
                                var valid = CheckLinkSelected(linkedScopingTypeIndividuals, idIndividual);
                                if (valid) {
                                    var propertyName = r.GetString(RawIndividualPropertyValues.PropertyName, fieldMap);
                                    var individual = allIndividuals[idIndividual];
                                    if (allIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
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
                    foreach (var property in allIndividualProperties) {
                        if (emptyPropertyTypes.Contains(property.Key)) {
                            var individualPropertyValues = allIndividuals.Values
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
            return allIndividualProperties;
        }
    }
}
