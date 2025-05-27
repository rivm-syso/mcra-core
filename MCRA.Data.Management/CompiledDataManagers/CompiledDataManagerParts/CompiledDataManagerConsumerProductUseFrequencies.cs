using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all consumer product surveys.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, ConsumerProductSurvey> GetAllConsumerProductSurveys() {
            if (_data.AllConsumerProductSurveys == null) {
                LoadScope(SourceTableGroup.ConsumerProductUseFrequencies);
                var allConsumerProductSurveys = new Dictionary<string, ConsumerProductSurvey>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductUseFrequencies);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawConsumerProductSurveys>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyId = r.GetString(RawConsumerProductSurveys.Id, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.ConsumerProductSurveys, surveyId);
                                    if (valid) {
                                        var survey = new ConsumerProductSurvey {
                                            Code = surveyId,
                                            Name = r.GetStringOrNull(RawConsumerProductSurveys.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawConsumerProductSurveys.Description, fieldMap),
                                            Country = r.GetStringOrNull(RawConsumerProductSurveys.Country, fieldMap),
                                            BodyWeightUnit = r.GetEnum(RawConsumerProductSurveys.BodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                            AgeUnitString = r.GetStringOrNull(RawConsumerProductSurveys.AgeUnit, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawConsumerProductSurveys.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawConsumerProductSurveys.EndDate, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawConsumerProductSurveys.IdPopulation, fieldMap),
                                        };
                                        allConsumerProductSurveys[survey.Code] = survey;
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllConsumerProductSurveys = allConsumerProductSurveys;
            }
            return _data.AllConsumerProductSurveys;
        }

        /// <summary>
        /// Gets all consumer product survey individuals.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Individual> GetAllConsumerProductIndividuals() {
            if (_data.AllConsumerProductIndividuals == null) {
                var allConsumerProductIndividuals = new Dictionary<string, Individual>(StringComparer.OrdinalIgnoreCase);
                var allIndividualProperties = new Dictionary<string, IndividualProperty>(StringComparer.OrdinalIgnoreCase);
                var emptyPropertyTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductUseFrequencies);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllConsumerProductSurveys();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read individuals
                        var id = 0;
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividuals>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyCode = r.GetString(RawIndividuals.IdFoodSurvey, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.ConsumerProductSurveys, surveyCode);
                                    if (valid) {
                                        var survey = _data.GetOrAddConsumerProductSurvey(surveyCode);
                                        var idIndividual = r.GetString(RawIndividuals.IdIndividual, fieldMap);
                                        var individual = new Individual(++id) {
                                            Code = idIndividual,
                                            BodyWeight = r.GetDoubleOrNull(RawIndividuals.BodyWeight, fieldMap) ?? double.NaN,
                                            SamplingWeight = r.GetDoubleOrNull(RawIndividuals.SamplingWeight, fieldMap) ?? 1D,
                                            CodeFoodSurvey = surveyCode,
                                        };
                                        allConsumerProductIndividuals.Add(individual.Code, individual);
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
                                    var valid = CheckLinkSelected(ScopingType.HumanMonitoringIndividuals, idIndividual);
                                    if (valid) {
                                        var propertyName = r.GetString(RawIndividualPropertyValues.PropertyName, fieldMap);
                                        var individual = allConsumerProductIndividuals[idIndividual];
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
                                var individualPropertyValues = allConsumerProductIndividuals.Values
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

                _data.AllConsumerProductIndividualProperties = allIndividualProperties;
                _data.AllConsumerProductIndividuals = allConsumerProductIndividuals;
            }
            return _data.AllConsumerProductIndividuals;
        }

        /// <summary>
        /// Gets all individual properties.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> GetAllConsumerProductIndividualProperties() {
            if (_data.AllConsumerProductIndividualProperties == null) {
                GetAllConsumerProductIndividuals();
            }
            return _data.AllConsumerProductIndividualProperties;
        }

        /// <summary>
        /// Gets all consumer product use frequencies.
        /// </summary>
        /// <returns></returns>
        public IList<IndividualConsumerProductUseFrequency> GetAllIndividualConsumerProductUseFrequencies() {
            if (_data.AllIndividualConsumerProductUseFrequencies == null) {
                var allIndividualConsumerProductUseFrequencies = new List<IndividualConsumerProductUseFrequency>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductUseFrequencies);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllConsumerProducts();
                    GetAllConsumerProductIndividuals();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        // Read samples
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualConsumerProductUseFrequencies>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idIndividual = r.GetString(RawIndividualConsumerProductUseFrequencies.IdIndividual, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.ConsumerProductIndividuals, idIndividual);
                                    if (valid) {
                                        var individual = _data.AllConsumerProductIndividuals[idIndividual];
                                        var product = r.GetString(RawIndividualConsumerProductUseFrequencies.IdProduct, fieldMap);
                                        var consumerProduct = _data.AllConsumerProducts[product];
                                        var individualConsumerProductUseFrequency = new IndividualConsumerProductUseFrequency() {
                                            Individual = individual,
                                            Product = consumerProduct,
                                            Frequency = r.GetDouble(RawIndividualConsumerProductUseFrequencies.Frequency, fieldMap),
                                        };
                                        allIndividualConsumerProductUseFrequencies.Add(individualConsumerProductUseFrequency);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllIndividualConsumerProductUseFrequencies = allIndividualConsumerProductUseFrequencies;
            }
            return _data.AllIndividualConsumerProductUseFrequencies;
        }

        private static void writeConsumerProductSurveyDataToCsv(string tempFolder, IEnumerable<ConsumerProductSurvey> surveys) {
            if (!surveys?.Any() ?? true) {
                return;
            }

            var tdsv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConsumerProductSurveys);
            var dtsv = tdsv.CreateDataTable();

            foreach (var survey in surveys) {
                var rowSv = dtsv.NewRow();
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.Id, survey.Code);
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.Name, survey.Name);
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.Description, survey.Description);
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.Country, survey.Country);
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.BodyWeightUnit, survey.BodyWeightUnit.ToString());
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.AgeUnit, survey.AgeUnitString);
                rowSv.WriteNonEmptyString(RawConsumerProductSurveys.IdPopulation, survey.IdPopulation);
                rowSv.WriteNonNullDateTime(RawConsumerProductSurveys.StartDate, survey.StartDate);
                rowSv.WriteNonNullDateTime(RawConsumerProductSurveys.EndDate, survey.EndDate);
                dtsv.Rows.Add(rowSv);
            }
            writeToCsv(tempFolder, tdsv, dtsv);
        }

        private static void writeIndividualConsumerProductUseFrequenciesToCsv(string tempFolder, IEnumerable<IndividualConsumerProductUseFrequency> frequencies) {
            if (!frequencies?.Any() ?? true) {
                return;
            }
            var tds = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IndividualConsumerProductUseFrequencies);
            var dts = tds.CreateDataTable();

            foreach (var freq in frequencies) {
                var rowSample = dts.NewRow();
                rowSample.WriteNonEmptyString(RawIndividualConsumerProductUseFrequencies.IdIndividual, freq.Individual.Code);
                rowSample.WriteNonEmptyString(RawIndividualConsumerProductUseFrequencies.IdProduct, freq.Product.Code);
                rowSample.WriteNonNullDouble(RawIndividualConsumerProductUseFrequencies.Frequency, freq.Frequency);
                dts.Rows.Add(rowSample);
            }
            writeToCsv(tempFolder, tds, dts);
        }
    }
}