using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all human monitoring surveys.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, HumanMonitoringSurvey> GetAllHumanMonitoringSurveys() {
            if (_data.AllHumanMonitoringSurveys == null) {
                LoadScope(SourceTableGroup.HumanMonitoringData);
                var allHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HumanMonitoringData);
                if (rawDataSourceIds?.Any() ?? false) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHumanMonitoringSurveys>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyId = r.GetString(RawHumanMonitoringSurveys.IdSurvey, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.HumanMonitoringSurveys, surveyId);
                                    if (valid) {
                                        int.TryParse(r.GetStringOrNull(RawHumanMonitoringSurveys.Year, fieldMap), out int year);
                                        var survey = new HumanMonitoringSurvey {
                                            Code = surveyId,
                                            Name = r.GetStringOrNull(RawHumanMonitoringSurveys.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawHumanMonitoringSurveys.Description, fieldMap),
                                            Location = r.GetStringOrNull(RawHumanMonitoringSurveys.Location, fieldMap),
                                            BodyWeightUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.BodyWeightUnit, fieldMap),
                                            AgeUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.AgeUnit, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawHumanMonitoringSurveys.StartDate, fieldMap) ?? (year > 0 ? new DateTime(year, 1, 1) : (DateTime?)null),
                                            EndDate = r.GetDateTimeOrNull(RawHumanMonitoringSurveys.EndDate, fieldMap) ?? (year > 0 ? new DateTime(year, 12, 31) : (DateTime?)null),
                                            NumberOfSurveyDays = r.GetInt32(RawHumanMonitoringSurveys.NumberOfSurveyDays, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawHumanMonitoringSurveys.IdPopulation, fieldMap),
                                            LipidConcentrationUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.LipidConcentrationUnit, fieldMap),
                                            CholestConcentrationUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.CholestConcentrationUnit, fieldMap),
                                            TriglycConcentrationUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.TriglycConcentrationUnit, fieldMap),
                                            CreatConcentrationUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.CreatConcentrationUnit, fieldMap)
                                        };
                                        allHumanMonitoringSurveys[survey.Code] = survey;
                                    }
                                }
                            }
                        }
                    }

                    // Add items by code from the scope where no matched items were found in the source
                    foreach (var code in GetCodesInScope(ScopingType.HumanMonitoringSurveys).Except(allHumanMonitoringSurveys.Keys, StringComparer.OrdinalIgnoreCase)) {
                        allHumanMonitoringSurveys[code] = new HumanMonitoringSurvey { Code = code };
                    }
                }
                _data.AllHumanMonitoringSurveys = allHumanMonitoringSurveys;
            }
            return _data.AllHumanMonitoringSurveys;
        }

        /// <summary>
        /// Gets all human monitoring survey individuals.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Individual> GetAllHumanMonitoringIndividuals() {
            if (_data.AllHumanMonitoringIndividuals == null) {
                var allHumanMonitoringIndividuals = new Dictionary<string, Individual>(StringComparer.OrdinalIgnoreCase);
                var allIndividualProperties = new Dictionary<string, IndividualProperty>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HumanMonitoringData);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllHumanMonitoringSurveys();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read individuals
                        var id = 0;
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividuals>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyCode = r.GetString(RawIndividuals.IdFoodSurvey, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.HumanMonitoringSurveys, surveyCode);
                                    if (valid) {
                                        var survey = _data.GetOrAddHumanMonitoringSurvey(surveyCode);
                                        var idIndividual = r.GetString(RawIndividuals.IdIndividual, fieldMap);
                                        var individual = new Individual(++id) {
                                            Code = idIndividual,
                                            BodyWeight = r.GetDouble(RawIndividuals.BodyWeight, fieldMap),
                                            //OriginalBodyWeight = r.GetDouble(RawIndividuals.BodyWeight, fieldMap),
                                            SamplingWeight = r.GetDoubleOrNull(RawIndividuals.SamplingWeight, fieldMap) ?? 1D,
                                            NumberOfDaysInSurvey = r.GetIntOrNull(RawIndividuals.NumberOfSurveyDays, fieldMap)
                                                ?? survey.NumberOfSurveyDays,
                                            CodeFoodSurvey = surveyCode,
                                        };
                                        allHumanMonitoringIndividuals.Add(individual.Code, individual);
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
                                        allIndividualProperties[propertyName] = new IndividualProperty {
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
                                    var valid = CheckLinkSelected(ScopingType.HumanMonitoringIndividuals, idIndividual);
                                    if (valid) {
                                        var propertyName = r.GetString(RawIndividualPropertyValues.PropertyName, fieldMap);
                                        var individual = allHumanMonitoringIndividuals[idIndividual];
                                        if (allIndividualProperties.TryGetValue(propertyName, out IndividualProperty individualProperty)) {
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
                        foreach (var property in allIndividualProperties) {
                            if (string.IsNullOrEmpty(property.Value.PropertyTypeString)) {
                                var individualPropertyValues = allHumanMonitoringIndividuals.Values
                                    .Where(r => r.IndividualPropertyValues.Any(c => c.IndividualProperty == property.Value))
                                    .Select(r => r.IndividualPropertyValues.First(c => c.IndividualProperty == property.Value))
                                    .ToList();
                                property.Value.PropertyTypeString = individualPropertyValues.All(ipv => ipv.IsNumeric()) ? IndividualPropertyType.Numeric.ToString() : IndividualPropertyType.Categorical.ToString();
                            }
                        }
                    }
                }

                _data.AllHumanMonitoringIndividualProperties = allIndividualProperties;
                _data.AllHumanMonitoringIndividuals = allHumanMonitoringIndividuals;
            }
            return _data.AllHumanMonitoringIndividuals;
        }

        /// <summary>
        /// Gets all individual properties.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> GetAllHumanMonitoringIndividualProperties() {
            if (_data.AllHumanMonitoringIndividualProperties == null) {
                GetAllHumanMonitoringIndividuals();
            }
            return _data.AllHumanMonitoringIndividualProperties;
        }

        /// <summary>
        /// Gets all human monitoring samples.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, HumanMonitoringSample> GetAllHumanMonitoringSamples() {
            if (_data.AllHumanMonitoringSamples == null) {
                var allHumanMonitoringSamples = new Dictionary<string, HumanMonitoringSample>(StringComparer.OrdinalIgnoreCase);
                var allAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(StringComparer.OrdinalIgnoreCase);
                var exposureEndpoints = new Dictionary<(string, string), HumanMonitoringSamplingMethod>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HumanMonitoringData);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();
                    var allIndividuals = GetAllHumanMonitoringIndividuals();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        fillAnalyticalMethods(rdm, allAnalyticalMethods, SourceTableGroup.HumanMonitoringData, ScopingType.HumanMonitoringAnalyticalMethods);

                        // Read samples
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHumanMonitoringSamples>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idIndividual = r.GetString(RawHumanMonitoringSamples.IdIndividual, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.HumanMonitoringIndividuals, idIndividual);
                                    if (valid) {
                                        var individual = allIndividuals[idIndividual];
                                        var compartment = r.GetStringOrNull(RawHumanMonitoringSamples.Compartment, fieldMap);
                                        var exposureRoute = r.GetStringOrNull(RawHumanMonitoringSamples.ExposureRoute, fieldMap);
                                        var sampleType = r.GetStringOrNull(RawHumanMonitoringSamples.SampleType, fieldMap);
                                        if (!exposureEndpoints.TryGetValue((exposureRoute, compartment), out HumanMonitoringSamplingMethod exposureEndpoint)) {
                                            exposureEndpoint = new HumanMonitoringSamplingMethod() {
                                                BiologicalMatrix = BiologicalMatrixConverter.FromString(compartment),
                                                ExposureRoute = exposureRoute,
                                                SampleTypeCode = sampleType
                                            };
                                            exposureEndpoints.Add((exposureRoute, compartment), exposureEndpoint);
                                        }
                                        var sample = new HumanMonitoringSample() {
                                            Code = r.GetString(RawHumanMonitoringSamples.IdSample, fieldMap),
                                            Individual = individual,
                                            DateSampling = r.GetDateTimeOrNull(RawHumanMonitoringSamples.DateSampling, fieldMap),
                                            DayOfSurvey = r.GetStringOrNull(RawHumanMonitoringSamples.DayOfSurvey, fieldMap),
                                            TimeOfSampling = r.GetStringOrNull(RawHumanMonitoringSamples.TimeOfSampling, fieldMap),
                                            SamplingMethod = exposureEndpoint,
                                            SpecificGravity = r.GetDoubleOrNull(RawHumanMonitoringSamples.SpecificGravity, fieldMap),
                                            SpecificGravityCorrectionFactor = r.GetDoubleOrNull(RawHumanMonitoringSamples.SpecificGravityCorrectionFactor, fieldMap),
                                            Name = r.GetStringOrNull(RawHumanMonitoringSamples.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawHumanMonitoringSamples.Description, fieldMap),
                                            LipidGrav = r.GetDoubleOrNull(RawHumanMonitoringSamples.LipidGrav, fieldMap),
                                            LipidEnz = r.GetDoubleOrNull(RawHumanMonitoringSamples.LipidEnz, fieldMap),
                                            Triglycerides = r.GetDoubleOrNull(RawHumanMonitoringSamples.Triglycerides, fieldMap),
                                            Cholesterol = r.GetDoubleOrNull(RawHumanMonitoringSamples.Cholesterol, fieldMap),
                                            Creatinine = r.GetDoubleOrNull(RawHumanMonitoringSamples.Creatinine, fieldMap),
                                            OsmoticConcentration = r.GetDoubleOrNull(RawHumanMonitoringSamples.OsmoticConcentration, fieldMap),
                                        };
                                        allHumanMonitoringSamples.Add(sample.Code, sample);
                                    }
                                }
                            }
                        }

                        // Read sample analyses
                        var sampleAnalyses = new Dictionary<string, SampleAnalysis>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHumanMonitoringSampleAnalyses>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSample = r.GetString(RawHumanMonitoringSampleAnalyses.IdSample, fieldMap);
                                    var idMethod = r.GetString(RawHumanMonitoringSampleAnalyses.IdAnalyticalMethod, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.HumanMonitoringSamples, idSample)
                                              & CheckLinkSelected(ScopingType.HumanMonitoringAnalyticalMethods, idMethod);
                                    if (valid) {
                                        var idAnalysisSample = r.GetString(RawHumanMonitoringSampleAnalyses.IdSampleAnalysis, fieldMap);
                                        var analyticalMethod = allAnalyticalMethods[idMethod];
                                        var sample = allHumanMonitoringSamples[idSample];
                                        var sampleAnalysis = new SampleAnalysis() {
                                            Code = idAnalysisSample,
                                            AnalyticalMethod = analyticalMethod,
                                            AnalysisDate = r.GetDateTimeOrNull(RawHumanMonitoringSampleAnalyses.DateAnalysis, fieldMap),
                                            Name = r.GetStringOrNull(RawHumanMonitoringSampleAnalyses.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawHumanMonitoringSampleAnalyses.Description, fieldMap),
                                        };
                                        sampleAnalyses.Add(sampleAnalysis.Code, sampleAnalysis);
                                        sample.SampleAnalyses.Add(sampleAnalysis);
                                    }
                                }
                            }
                        }

                        // Read sample analysis concentrations
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHumanMonitoringSampleConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSample = r.GetString(RawHumanMonitoringSampleConcentrations.IdAnalysisSample, fieldMap);
                                    var idSubstance = r.GetString(RawHumanMonitoringSampleConcentrations.IdCompound, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.HumanMonitoringSampleAnalyses, idSample)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var sample = sampleAnalyses[idSample];
                                        var substance = _data.GetOrAddSubstance(idSubstance);
                                        var resTypeString = r.GetStringOrNull(RawHumanMonitoringSampleConcentrations.ResType, fieldMap);
                                        var concentration = r.GetDoubleOrNull(RawHumanMonitoringSampleConcentrations.Concentration, fieldMap);
                                        if (string.IsNullOrEmpty(resTypeString)) {
                                            if (!concentration.HasValue || double.IsNaN(concentration.Value)) {
                                                resTypeString = "MV";
                                            } else {
                                                resTypeString = "VAL";
                                            }
                                        }
                                        var c = new ConcentrationPerSample {
                                            Sample = sample,
                                            Compound = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = concentration,
                                            ResTypeString = resTypeString,
                                        };
                                        sample.Concentrations[substance] = c;
                                    }
                                }
                            }
                        }

                    }
                }

                _data.AllHumanMonitoringSamples = allHumanMonitoringSamples;
                _data.AllHumanMonitoringAnalyticalMethods = allAnalyticalMethods;
                _data.HumanMonitoringSamplingMethods = new HashSet<HumanMonitoringSamplingMethod>(exposureEndpoints.Values);
            }
            return _data.AllHumanMonitoringSamples;
        }

        /// <summary>
        /// Returns all distinct analytical methods of all samples of the compiled data source
        /// and any methods for focal foods
        /// </summary>
        public IDictionary<string, AnalyticalMethod> GetAllHumanMonitoringAnalyticalMethods() {
            if (_data.AllHumanMonitoringAnalyticalMethods == null) {
                GetAllHumanMonitoringSamples();
            }
            return _data.AllHumanMonitoringAnalyticalMethods;
        }

        /// <summary>
        /// Gets all endpoints measured by the human monitoring surveys.
        /// </summary>
        /// <returns></returns>
        public ICollection<HumanMonitoringSamplingMethod> GetAllHumanMonitoringSamplingMethods() {
            if (_data.HumanMonitoringSamplingMethods == null) {
                GetAllHumanMonitoringSamples();
            }
            return _data.HumanMonitoringSamplingMethods;
        }

        private static void writeHumanMonitoringSurveyDataToCsv(string tempFolder, IEnumerable<HumanMonitoringSurvey> surveys) {
            if (!surveys?.Any() ?? true) {
                return;
            }

            var tdsv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.HumanMonitoringSurveys);
            var dtsv = tdsv.CreateDataTable();

            foreach (var survey in surveys) {
                var rowSv = dtsv.NewRow();

                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.IdSurvey, survey.Code);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.Name, survey.Name);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.Description, survey.Description);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.Location, survey.Location);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.BodyWeightUnit, survey.BodyWeightUnitString);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.AgeUnit, survey.AgeUnitString);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.IdPopulation, survey.IdPopulation);
                rowSv.WriteNonNullDateTime(RawHumanMonitoringSurveys.StartDate, survey.StartDate);
                rowSv.WriteNonNullDateTime(RawHumanMonitoringSurveys.EndDate, survey.EndDate);
                rowSv.WriteValue(RawHumanMonitoringSurveys.NumberOfSurveyDays, survey.NumberOfSurveyDays);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.LipidConcentrationUnit, survey.LipidConcentrationUnitString);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.TriglycConcentrationUnit, survey.TriglycConcentrationUnitString);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.CholestConcentrationUnit, survey.CholestConcentrationUnitString);
                rowSv.WriteNonEmptyString(RawHumanMonitoringSurveys.CreatConcentrationUnit, survey.CreatConcentrationUnitString);
                dtsv.Rows.Add(rowSv);
            }

            writeToCsv(tempFolder, tdsv, dtsv);
        }

        private static void writeHumanMonitoringSampleDataToCsv(string tempFolder, IEnumerable<HumanMonitoringSample> samples) {
            if (!samples?.Any() ?? true) {
                return;
            }

            var tds = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.HumanMonitoringSamples);
            var dts = tds.CreateDataTable();
            var tdsa = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.HumanMonitoringSampleAnalyses);
            var dtsa = tdsa.CreateDataTable();
            var tdsc = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.HumanMonitoringSampleConcentrations);
            var dtsc = tdsc.CreateDataTable();

            foreach (var s in samples) {
                var rowSample = dts.NewRow();
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.IdSample, s.Code);
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.IdIndividual, s.Individual.Code);
                rowSample.WriteNonNullDateTime(RawHumanMonitoringSamples.DateSampling, s.DateSampling);
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.DayOfSurvey, s.DayOfSurvey);
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.TimeOfSampling, s.TimeOfSampling);
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.Compartment, s.SamplingMethod?.BiologicalMatrix.ToString());
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.ExposureRoute, s.SamplingMethod?.ExposureRoute);
                rowSample.WriteNonEmptyString(RawHumanMonitoringSamples.SampleType, s.SamplingMethod?.SampleTypeCode);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.SpecificGravity, s.SpecificGravity);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.LipidEnz, s.LipidEnz);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.LipidGrav, s.LipidGrav);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.Cholesterol, s.Cholesterol);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.Creatinine, s.Creatinine);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.OsmoticConcentration, s.OsmoticConcentration);
                rowSample.WriteNonNullDouble(RawHumanMonitoringSamples.Triglycerides, s.Triglycerides);
                dts.Rows.Add(rowSample);

                foreach (var sa in s.SampleAnalyses) {
                    var rowsa = dtsa.NewRow();
                    rowsa.WriteNonEmptyString(RawHumanMonitoringSampleAnalyses.IdSampleAnalysis, sa.Code);
                    rowsa.WriteNonEmptyString(RawHumanMonitoringSampleAnalyses.IdSample, s.Code);
                    rowsa.WriteNonEmptyString(RawHumanMonitoringSampleAnalyses.IdAnalyticalMethod, sa.AnalyticalMethod?.Code);
                    rowsa.WriteNonNullDateTime(RawHumanMonitoringSampleAnalyses.DateAnalysis, sa.AnalysisDate);
                    dtsa.Rows.Add(rowsa);

                    foreach (var sc in sa.Concentrations) {
                        var rowsc = dtsc.NewRow();
                        rowsc.WriteNonEmptyString(RawHumanMonitoringSampleConcentrations.IdAnalysisSample, sa.Code);
                        rowsc.WriteNonEmptyString(RawHumanMonitoringSampleConcentrations.IdCompound, sc.Key.Code);
                        rowsc.WriteNonNullDouble(RawHumanMonitoringSampleConcentrations.Concentration, sc.Value.Concentration);
                        rowsc.WriteNonEmptyString(RawHumanMonitoringSampleConcentrations.ResType, sc.Value.ResType.ToString());
                        dtsc.Rows.Add(rowsc);
                    }
                }
            }

            writeToCsv(tempFolder, tds, dts);
            writeToCsv(tempFolder, tdsa, dtsa);
            writeToCsv(tempFolder, tdsc, dtsc);
        }
    }
}
