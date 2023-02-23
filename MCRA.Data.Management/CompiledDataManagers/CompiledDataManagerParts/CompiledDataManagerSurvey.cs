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
                if (rawDataSourceIds?.Any() ?? false) {
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
                                            BodyWeightUnitString = r.GetStringOrNull(RawFoodSurveys.BodyWeightUnit, fieldMap),
                                            AgeUnitString = r.GetStringOrNull(RawFoodSurveys.AgeUnit, fieldMap),
                                            ConsumptionUnitString = r.GetStringOrNull(RawFoodSurveys.ConsumptionUnit, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawFoodSurveys.StartDate, fieldMap) ?? (year > 0 ? new DateTime(year, 1, 1) : (DateTime?)null),
                                            EndDate = r.GetDateTimeOrNull(RawFoodSurveys.EndDate, fieldMap) ?? (year > 0 ? new DateTime(year, 12, 31) : (DateTime?)null),
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
                var allIndividuals = new Dictionary<string, Individual>(StringComparer.OrdinalIgnoreCase);
                var allDietaryIndividualProperties = new Dictionary<string, IndividualProperty>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Survey);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoodSurveys();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        var id = 0;
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividuals>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyCode = r.GetString(RawIndividuals.IdFoodSurvey, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.FoodSurveys, surveyCode);
                                    if (valid) {
                                        var survey = _data.GetOrAddFoodSurvey(surveyCode);
                                        var idIndividual = r.GetString(RawIndividuals.IdIndividual, fieldMap);
                                        var idv = new Individual(++id) {
                                            Code = idIndividual,
                                            BodyWeight = r.GetDouble(RawIndividuals.BodyWeight, fieldMap),
                                            SamplingWeight = r.GetDoubleOrNull(RawIndividuals.SamplingWeight, fieldMap) ?? 1D,
                                            NumberOfDaysInSurvey = r.GetIntOrNull(RawIndividuals.NumberOfSurveyDays, fieldMap)
                                                ?? survey.NumberOfSurveyDays ?? 1,
                                            IndividualDays = new Dictionary<string, IndividualDay>(StringComparer.OrdinalIgnoreCase),
                                            Name = r.GetStringOrNull(RawIndividuals.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawIndividuals.Description, fieldMap),
                                        };
                                        idv.CodeFoodSurvey = surveyCode;
                                        allIndividuals.Add(idv.Code, idv);
                                        survey.Individuals.Add(idv);
                                    }
                                }
                            }
                        }

                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualDays>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idIndividual = r.GetString(RawIndividualDays.IdIndividual, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.DietaryIndividuals, idIndividual);
                                    if (valid) {
                                        var individual = allIndividuals[idIndividual];
                                        var idDay = r.GetString(RawIndividualDays.IdDay, fieldMap);
                                        var idv = new IndividualDay() {
                                            Individual = individual,
                                            IdDay = idDay,
                                            Date = r.GetDateTimeOrNull(RawIndividualDays.SamplingDate, fieldMap)
                                        };
                                        individual.IndividualDays[idDay] = idv;
                                    }
                                }
                            }
                        }

                        // Read individual properties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualProperties>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var propertyName = r.GetString(RawIndividualProperties.IdIndividualProperty, fieldMap);
                                    if (!allDietaryIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
                                        var name = r.GetStringOrNull(RawIndividualProperties.Name, fieldMap);
                                        allDietaryIndividualProperties[propertyName] = new IndividualProperty {
                                            Code = propertyName,
                                            Name = !string.IsNullOrEmpty(name) ? name : propertyName,
                                            Description = r.GetStringOrNull(RawIndividualProperties.Description, fieldMap),
                                            PropertyLevelString = r.GetStringOrNull(RawIndividualProperties.PropertyLevel, fieldMap),
                                            PropertyTypeString = r.GetStringOrNull(RawIndividualProperties.Type, fieldMap),
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
                                    var valid = CheckLinkSelected(ScopingType.DietaryIndividuals, idIndividual);
                                    if (valid) {
                                        var propertyName = r.GetString(RawIndividualPropertyValues.PropertyName, fieldMap);
                                        var individual = allIndividuals[idIndividual];
                                        if (allDietaryIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
                                            var propertyValue = new IndividualPropertyValue {
                                                IndividualProperty = individualProperty,
                                                TextValue = r.GetStringOrNull(RawIndividualPropertyValues.TextValue, fieldMap),
                                                DoubleValue = r.GetDoubleOrNull(RawIndividualPropertyValues.DoubleValue, fieldMap)
                                            };
                                            individual.IndividualPropertyValues.Add(propertyValue);
                                        }
                                    }
                                }
                            }
                        }

                        // Set property types (i.e., numeric/categorical) for properties without type
                        foreach (var property in allDietaryIndividualProperties) {
                            if (string.IsNullOrEmpty(property.Value.PropertyTypeString)) {
                                var individualPropertyValues = allIndividuals.Values
                                    .Where(r => r.IndividualPropertyValues.Any(c => c.IndividualProperty == property.Value))
                                    .Select(r => r.IndividualPropertyValues.First(c => c.IndividualProperty == property.Value))
                                    .ToList();
                                property.Value.PropertyTypeString = individualPropertyValues.All(ipv => ipv.IsNumeric()) ? IndividualPropertyType.Numeric.ToString() : IndividualPropertyType.Categorical.ToString();
                            }
                        }
                    }
                }

                _data.AllDietaryIndividualProperties = allDietaryIndividualProperties;
                _data.AllIndividuals = allIndividuals;
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
                if (rawDataSourceIds?.Any() ?? false) {
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
                rowsv.WriteNonEmptyString(RawFoodSurveys.BodyWeightUnit, survey.BodyWeightUnitString, ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.AgeUnit, survey.AgeUnitString, ccr);
                rowsv.WriteNonEmptyString(RawFoodSurveys.ConsumptionUnit, survey.ConsumptionUnitString, ccr);
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
                if (individual.IndividualPropertyValues.Count > 0) {
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
