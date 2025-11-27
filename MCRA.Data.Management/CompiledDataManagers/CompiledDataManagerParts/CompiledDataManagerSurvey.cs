using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Data.Compiled.Utils;

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

        private static void writeFoodSurveyDataToCsv(string tempFolder, IEnumerable<FoodSurvey> surveys) {
            if (!surveys?.Any() ?? true) {
                return;
            }

            var tdsv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodSurveys);
            var dtsv = tdsv.CreateDataTable();

            var ccr = new int[Enum.GetNames(typeof(RawFoodSurveys)).Length];

            foreach (var survey in surveys) {
                var rowsv = dtsv.NewRow();

                rowsv.WriteNonEmptyString(RawFoodSurveys.IdFoodSurvey, survey.Code, ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.Description, survey.Description, ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.Location, survey.Location, ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.BodyWeightUnit, survey.BodyWeightUnit.ToString(), ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.AgeUnit, survey.AgeUnitString, ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.ConsumptionUnit, survey.ConsumptionUnit.ToString(), ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.IdPopulation, survey.IdPopulation, ccr);
                rowsv.WriteNonNullInt32(RawFoodSurveys.NumberOfSurveyDays, survey.NumberOfSurveyDays, ccr);
                rowsv.WriteNonNullDateTime(RawFoodSurveys.StartDate, survey.StartDate, ccr);
                rowsv.WriteNonNullDateTime(RawFoodSurveys.EndDate, survey.EndDate, ccr);

                dtsv.Rows.Add(rowsv);
            }

            writeToCsv(tempFolder, tdsv, dtsv, ccr);
        }

        private static void writeIndividualsDataToCsv(string tempFolder, IEnumerable<Individual> individuals) {
            if (!individuals?.Any() ?? true) {
                return;
            }

            var tdi = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Individuals);
            var dti = tdi.CreateDataTable();
            var tdid = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IndividualDays);
            var dtid = tdid.CreateDataTable();
            var tdp = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IndividualProperties);
            var dtp = tdp.CreateDataTable();
            var tdpv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IndividualPropertyValues);
            var dtpv = tdpv.CreateDataTable();

            var ccri = new int[Enum.GetNames(typeof(RawIndividuals)).Length];
            var ccrid = new int[Enum.GetNames(typeof(RawIndividualDays)).Length];
            var ccrpv = new int[Enum.GetNames(typeof(RawIndividualPropertyValues)).Length];

            var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var individual in individuals) {
                var rowi = dti.NewRow();

                rowi.WriteNonEmptyString(RawIndividuals.IdIndividual, individual.Code, ccri);
                rowi.WriteNonEmptyString(RawIndividuals.IdFoodSurvey, individual.CodeFoodSurvey, ccri);
                rowi.WriteNonNullInt32(RawIndividuals.NumberOfSurveyDays, individual.NumberOfDaysInSurvey, ccri);
                rowi.WriteNonNaNDouble(RawIndividuals.BodyWeight, individual.BodyWeight, ccri);
                rowi.WriteNonNaNDouble(RawIndividuals.SamplingWeight, individual.SamplingWeight, ccri);

                dti.Rows.Add(rowi);

                //individual properties and values
                if (individual.IndividualPropertyValues.Any()) {
                    foreach (var prop in individual.IndividualPropertyValues) {
                        if (!properties.Contains(prop.IndividualProperty.Name)) {
                            //unique property names
                            var rowp = dtp.NewRow();
                            rowp.WriteNonEmptyString(RawIndividualProperties.Name, prop.IndividualProperty.Name);
                            dtp.Rows.Add(rowp);

                            properties.Add(prop.IndividualProperty.Name);
                        }

                        //property values per individual
                        var rowpv = dtpv.NewRow();
                        rowpv.WriteNonEmptyString(RawIndividualPropertyValues.IdIndividual, individual.Code, ccrpv);
                        rowpv.WriteNonEmptyString(RawIndividualPropertyValues.PropertyName, prop.IndividualProperty.Name, ccrpv);
                        rowpv.WriteNonNullDouble(RawIndividualPropertyValues.DoubleValue, prop.DoubleValue, ccrpv);
                        rowpv.WriteNonEmptyString(RawIndividualPropertyValues.TextValue, prop.TextValue, ccrpv);
                        dtpv.Rows.Add(rowpv);
                    }
                } else {
                    var rowp = dtp.NewRow();
                    rowp.WriteNonEmptyString(RawIndividuals.IdIndividual, individual.Code);
                    dtp.Rows.Add(rowp);
                }

                //individual days
                foreach (var id in individual.IndividualDays) {
                    //property values per individual
                    var row = dtid.NewRow();
                    row.WriteNonEmptyString(RawIndividualDays.IdIndividual, individual.Code, ccrpv);
                    row.WriteNonEmptyString(RawIndividualDays.IdDay, id.Value.IdDay, ccrpv);
                    row.WriteNonNullDateTime(RawIndividualDays.SamplingDate, id.Value.Date);
                    dtid.Rows.Add(row);
                }
            }

            writeToCsv(tempFolder, tdi, dti, ccri);
            writeToCsv(tempFolder, tdid, dtid, ccrid);
            writeToCsv(tempFolder, tdp, dtp);
            writeToCsv(tempFolder, tdpv, dtpv, ccrpv);
        }

        private static void writeConsumptionDataToCsv(string tempFolder, IEnumerable<FoodConsumption> consumptions) {
            if (!consumptions?.Any() ?? true) {
                return;
            }

            var tdc = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Consumptions);
            var dtc = tdc.CreateDataTable();

            var ccr = new int[Enum.GetNames(typeof(RawFoodConsumptions)).Length];

            foreach (var consumption in consumptions) {
                var rowc = dtc.NewRow();

                rowc.WriteNonEmptyString(RawFoodConsumptions.IdIndividual, consumption.Individual.Code, ccr);
                rowc.WriteNonEmptyString(RawFoodConsumptions.IdFood, consumption.Food.Code, ccr);
                rowc.WriteNonEmptyString(RawFoodConsumptions.IdDay, consumption.idDay, ccr);
                rowc.WriteNonEmptyString(RawFoodConsumptions.IdMeal, consumption.idMeal, ccr);
                rowc.WriteNonEmptyString(RawFoodConsumptions.IdUnit, consumption.FoodConsumptionQuantification?.UnitCode, ccr);
                rowc.WriteNonNaNDouble(RawFoodConsumptions.Amount, consumption.Amount, ccr);

                dtc.Rows.Add(rowc);
            }

            writeToCsv(tempFolder, tdc, dtc, ccr);
        }
    }
}
