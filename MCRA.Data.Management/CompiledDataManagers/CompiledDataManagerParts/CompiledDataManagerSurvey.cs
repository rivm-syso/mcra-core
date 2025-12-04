using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Utils;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// The selected food survey.
        /// </summary>
        public IDictionary<string, FoodSurvey> GetAllFoodSurveys() {
            if (_data.AllFoodSurveys == null) {
                LoadScope(SourceTableGroup.Survey);
                var allFoodSurveys = new Dictionary<string, FoodSurvey>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Survey);
                if (rawDataSourceIds?.Count > 0) {
                    LoadScope(SourceTableGroup.Survey);
                    GetAllPopulations();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodSurveys>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSurvey = r.GetString(RawFoodSurveys.IdFoodSurvey, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.FoodSurveys, idSurvey);
                                    if (valid) {
                                        int.TryParse(r.GetStringOrNull(RawFoodSurveys.Year, fieldMap), out int year);
                                        var survey = new FoodSurvey {
                                            Code = idSurvey,
                                            Name = r.GetStringOrNull(RawFoodSurveys.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawFoodSurveys.Description, fieldMap),
                                            Location = r.GetStringOrNull(RawFoodSurveys.Location, fieldMap),
                                            BodyWeightUnit = r.GetEnum(RawFoodSurveys.BodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                            AgeUnitString = r.GetStringOrNull(RawFoodSurveys.AgeUnit, fieldMap),
                                            ConsumptionUnit = r.GetEnum(RawFoodSurveys.ConsumptionUnit, fieldMap, ConsumptionUnit.g),
                                            StartDate = r.GetDateTimeOrNull(RawFoodSurveys.StartDate, fieldMap) ?? (year > 0 ? new DateTime(year, 1, 1) : null),
                                            EndDate = r.GetDateTimeOrNull(RawFoodSurveys.EndDate, fieldMap) ?? (year > 0 ? new DateTime(year, 12, 31) : null),
                                            NumberOfSurveyDays = r.GetIntOrNull(RawFoodSurveys.NumberOfSurveyDays, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawFoodSurveys.IdPopulation, fieldMap),
                                        };
                                        allFoodSurveys[survey.Code] = survey;
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllFoodSurveys = allFoodSurveys;
            }

            return _data.AllFoodSurveys;
        }

        /// <summary>
        /// Gets all individuals of the compiled data source.
        /// </summary>
        public IDictionary<string, Individual> GetAllIndividuals() {
            if (_data.AllIndividuals == null) {
                GetAllFoodSurveys();
                var collectionNrOfDaysInSurvey = _data.AllFoodSurveys.Values
                    .ToDictionary(s => s.Code, s => s.NumberOfSurveyDays.GetValueOrDefault(), StringComparer.OrdinalIgnoreCase);
                _data.AllIndividuals = GetIndividuals(
                    SourceTableGroup.Survey,
                    ScopingType.FoodSurveys,
                    _data.GetOrAddFoodSurvey,
                    collectionNrOfDaysInSurvey
                    );
                _data.AllDietaryIndividualProperties = GetIndividualProperties(
                    SourceTableGroup.Survey,
                    ScopingType.DietaryIndividuals,
                    _data.AllIndividuals
                    );
            }
            return _data.AllIndividuals;
        }

        /// <summary>
        /// Gets all individual properties.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> GetAllIndividualProperties() {
            if (_data.AllDietaryIndividualProperties == null) {
                GetAllIndividuals();
            }
            return _data.AllDietaryIndividualProperties;
        }

        /// <summary>
        /// Get all food consumptions of the compiled data source.
        /// </summary>
        public IList<FoodConsumption> GetAllFoodConsumptions() {
            if (_data.AllFoodConsumptions == null) {
                var allFoodConsumptions = new List<FoodConsumption>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Survey);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    GetAllIndividuals();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawFoodConsumptions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    //IMPORTANT NOTE: don't process any lines where the consumed amount <= 0
                                    var consumedAmount = r.GetDouble(RawFoodConsumptions.Amount, fieldMap);
                                    if (consumedAmount <= 0D) {
                                        continue;
                                    }
                                    var idFood = r.GetString(RawFoodConsumptions.IdFood, fieldMap);
                                    var idIndividual = r.GetString(RawFoodConsumptions.IdIndividual, fieldMap);

                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & CheckLinkSelected(ScopingType.DietaryIndividuals, idIndividual);
                                    if (valid) {
                                        var food = getOrAddFood(idFood, idFood);
                                        var individual = _data.AllIndividuals[idIndividual];
                                        var idDay = r.GetString(RawFoodConsumptions.IdDay, fieldMap);

                                        if (!individual.IndividualDays.TryGetValue(idDay, out var individualDay)) {
                                            individualDay = new IndividualDay() {
                                                Individual = individual,
                                                IdDay = idDay,
                                                Date = r.GetDateTimeOrNull(RawFoodConsumptions.DateConsumed, fieldMap)
                                            };
                                            individual.IndividualDays[idDay] = individualDay;
                                        }

                                        var facetString = r.GetStringOrNull(RawFoodConsumptions.Facets, fieldMap);
                                        if (!string.IsNullOrEmpty(facetString) && FoodCodeUtilities.IsFoodEx2FacetString(facetString)) {
                                            var facetCodes = FoodCodeUtilities.ParseFacetString(facetString);
                                            if (facetCodes.Any()) {
                                                var facetFoodCode = $"{idFood}#{string.Join("$", facetCodes)}";
                                                var consumedFood = getOrAddFood(facetFoodCode, resolveFoodEx2: true);
                                                food = consumedFood;
                                            }
                                        }
                                        var consumption = new FoodConsumption {
                                            Food = food,
                                            IndividualDay = individualDay,
                                            idMeal = r.GetStringOrNull(RawFoodConsumptions.IdMeal, fieldMap),
                                            Amount = consumedAmount,
                                            DateConsumed = r.GetDateTimeOrNull(RawFoodConsumptions.DateConsumed, fieldMap)
                                        };

                                        //if applicable, link to the food consumption quantification data
                                        var idUnit = r.GetStringOrNull(RawFoodConsumptions.IdUnit, fieldMap);
                                        if (!string.IsNullOrEmpty(idUnit) &&
                                            food.FoodConsumptionQuantifications.TryGetValue(idUnit, out FoodConsumptionQuantification quantification)) {
                                            consumption.FoodConsumptionQuantification = quantification;
                                        }
                                        allFoodConsumptions.Add(consumption);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllFoodConsumptions = allFoodConsumptions;
            }
            return _data.AllFoodConsumptions;
        }
    }
}
