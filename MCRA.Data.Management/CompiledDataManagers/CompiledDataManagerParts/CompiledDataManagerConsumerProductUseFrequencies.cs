using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all consumer product surveys.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, ConsumerProductSurvey> GetAllConsumerProductSurveys() {
            if (_data.AllConsumerProductSurveys == null) {
                var allSurveys = new Dictionary<int, string>();
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
                                            IdDataSource = rawDataSourceId,
                                            Name = r.GetStringOrNull(RawConsumerProductSurveys.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawConsumerProductSurveys.Description, fieldMap),
                                            Country = r.GetStringOrNull(RawConsumerProductSurveys.Country, fieldMap),
                                            BodyWeightUnit = r.GetEnum(RawConsumerProductSurveys.BodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                            StartDate = r.GetDateTimeOrNull(RawConsumerProductSurveys.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawConsumerProductSurveys.EndDate, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawConsumerProductSurveys.IdPopulation, fieldMap),
                                        };
                                        allConsumerProductSurveys[survey.Code] = survey;
                                        if (allSurveys.TryGetValue(rawDataSourceId, value: out var _)) {
                                            continue;
                                        }
                                        allSurveys[rawDataSourceId] = surveyId;
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
        public IList<Individual> GetAllConsumerProductIndividuals() {
            if (_data.AllConsumerProductIndividuals == null) {
                GetAllConsumerProductSurveys();
                (var individuals, var individualProperties) = GetIndividuals(
                    SourceTableGroup.ConsumerProductUseFrequencies,
                    ScopingType.ConsumerProductSurveys,
                    _data.GetOrAddConsumerProductSurvey
                );
                _data.AllConsumerProductIndividuals = individuals;
                _data.AllConsumerProductIndividualProperties = individualProperties;
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
                    var surveys = GetAllConsumerProductSurveys();
                    var individuals = GetAllConsumerProductIndividuals();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        // Read samples
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            var dataSourceSurveys = surveys.Values.Where(r => r.IdDataSource == rawDataSourceId);
                            var dataSourceIndividuals = dataSourceSurveys
                                .SelectMany(r => r.Individuals)
                                .ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);
                            using (var r = rdm.OpenDataReader<RawConsumerProductUseFrequencies>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idIndividual = r.GetString(RawConsumerProductUseFrequencies.IdIndividual, fieldMap);
                                    var idProduct = r.GetString(RawConsumerProductUseFrequencies.IdProduct, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.ConsumerProductIndividuals, idIndividual)
                                        && IsCodeSelected(ScopingType.ConsumerProducts, idProduct); ;
                                    if (valid) {
                                        if (!dataSourceIndividuals.TryGetValue(idIndividual, out var individual)) {
                                            continue;
                                        }
                                        var consumerProduct = _data.AllConsumerProducts[idProduct];
                                        var individualConsumerProductUseFrequency = new IndividualConsumerProductUseFrequency() {
                                            Individual = individual,
                                            Product = consumerProduct,
                                            Frequency = r.GetDouble(RawConsumerProductUseFrequencies.Frequency, fieldMap),
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
    }
}
