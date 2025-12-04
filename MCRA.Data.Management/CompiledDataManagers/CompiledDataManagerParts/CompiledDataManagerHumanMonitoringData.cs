using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

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
                if (rawDataSourceIds?.Count > 0) {
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
                                            BodyWeightUnit = r.GetEnum(RawHumanMonitoringSurveys.BodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                            AgeUnitString = r.GetStringOrNull(RawHumanMonitoringSurveys.AgeUnit, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawHumanMonitoringSurveys.StartDate, fieldMap) ?? (year > 0 ? new DateTime(year, 1, 1) : null),
                                            EndDate = r.GetDateTimeOrNull(RawHumanMonitoringSurveys.EndDate, fieldMap) ?? (year > 0 ? new DateTime(year, 12, 31) : null),
                                            NumberOfSurveyDays = r.GetInt32(RawHumanMonitoringSurveys.NumberOfSurveyDays, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawHumanMonitoringSurveys.IdPopulation, fieldMap),
                                            LipidConcentrationUnit = r.GetEnum(RawHumanMonitoringSurveys.LipidConcentrationUnit, fieldMap, ConcentrationUnit.mgPerdL),
                                            CholestConcentrationUnit = r.GetEnum(RawHumanMonitoringSurveys.CholestConcentrationUnit, fieldMap, ConcentrationUnit.mgPerdL),
                                            TriglycConcentrationUnit = r.GetEnum(RawHumanMonitoringSurveys.TriglycConcentrationUnit, fieldMap, ConcentrationUnit.mgPerdL),
                                            CreatConcentrationUnit = r.GetEnum(RawHumanMonitoringSurveys.CreatConcentrationUnit, fieldMap, ConcentrationUnit.mgPerdL)
                                        };
                                        allHumanMonitoringSurveys[survey.Code] = survey;
                                    }
                                }
                            }

                            // Read time points
                            using (var r = rdm.OpenDataReader<RawHumanMonitoringTimepoints>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var surveyId = r.GetString(RawHumanMonitoringTimepoints.IdSurvey, fieldMap);
                                    if (allHumanMonitoringSurveys.ContainsKey(surveyId)) {
                                        var timepoint = new HumanMonitoringTimepoint {
                                            Code = r.GetString(RawHumanMonitoringTimepoints.IdTimepoint, fieldMap),
                                            Name = r.GetStringOrNull(RawHumanMonitoringTimepoints.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawHumanMonitoringTimepoints.Description, fieldMap),
                                        };
                                        allHumanMonitoringSurveys[surveyId].Timepoints.Add(timepoint);
                                    }
                                }

                                // Check for valid time points, if none were added then populate the time points based on the number of days in the survey
                                foreach (var survey in allHumanMonitoringSurveys) {
                                    if (!survey.Value.Timepoints.Any()) {
                                        survey.Value.Timepoints = Enumerable
                                            .Range(1, survey.Value.NumberOfSurveyDays)
                                            .Select(i => new HumanMonitoringTimepoint {
                                                Code = $"{i}",
                                                Name = $"Survey day {i}"
                                            })
                                            .ToHashSet();
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
                GetAllHumanMonitoringSurveys();
                var collectionNrOfDaysInSurvey = _data.AllHumanMonitoringSurveys.Values
                    .ToDictionary(s => s.Code, s => s.NumberOfSurveyDays, StringComparer.OrdinalIgnoreCase);
                _data.AllHumanMonitoringIndividuals = GetIndividuals(
                    SourceTableGroup.HumanMonitoringData,
                    ScopingType.HumanMonitoringSurveys,
                    _data.GetOrAddHumanMonitoringSurvey,
                    collectionNrOfDaysInSurvey
                    );
                _data.AllHumanMonitoringIndividualProperties = GetIndividualProperties(
                    SourceTableGroup.HumanMonitoringData,
                    ScopingType.HumanMonitoringIndividuals,
                    _data.AllHumanMonitoringIndividuals
                    );
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
                var exposureEndpoints = new Dictionary<(string, string, string), HumanMonitoringSamplingMethod>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HumanMonitoringData);
                if (rawDataSourceIds?.Count > 0) {
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
                                        if (!exposureEndpoints.TryGetValue((exposureRoute, compartment, sampleType), out HumanMonitoringSamplingMethod exposureEndpoint)) {
                                            exposureEndpoint = new HumanMonitoringSamplingMethod() {
                                                BiologicalMatrix = BiologicalMatrixConverter.FromString(compartment),
                                                ExposureRoute = exposureRoute,
                                                SampleTypeCode = sampleType
                                            };
                                            exposureEndpoints.Add((exposureRoute, compartment, sampleType), exposureEndpoint);
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
                                            UrineVolume = r.GetDoubleOrNull(RawHumanMonitoringSamples.UrineVolume, fieldMap),
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
                                        var isMissing = !concentration.HasValue || double.IsNaN(concentration.Value);
                                        var resType = ResTypeConverter.FromString(
                                            resTypeString,
                                            isMissing ? ResType.MV : ResType.VAL
                                        );
                                        var c = new ConcentrationPerSample {
                                            Sample = sample,
                                            Compound = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = concentration,
                                            ResType = resType,
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
    }
}
