using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.DataSourceReading.Attributes;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.EuHbmDataCopiers {

    public class EuHbmImportDataCopier : RawDataSourceBulkCopierBase {

        #region Supported codebook versions

        private class CodebookVersion {
            public string Id { get; set; }
            public Version Version { get; set; }

            public CodebookVersion(string id, Version version) {
                Id = id;
                Version = version;
            }
        }

        private static readonly Dictionary<string, CodebookVersion> _supportedCodebookVersions
            = new(StringComparer.OrdinalIgnoreCase) {
                { "PARC", new CodebookVersion("PARC", new Version(1, 9)) }, // not an official version (pre 2.0 version)
                { "BasicCodebook_v2.0", new CodebookVersion("BasicCodebook_v2.0", new Version(2, 0)) },
                { "BasicCodebook_v2.1", new CodebookVersion("BasicCodebook_v2.1", new Version(2, 1)) },
                { "BasicCodebook_v2.2", new CodebookVersion("BasicCodebook_v2.2", new Version(2, 2)) },
                { "BasicCodebook_v2.3", new CodebookVersion("BasicCodebook_v2.3", new Version(2, 3)) },
                { "BasicCodebook_v2.4", new CodebookVersion("BasicCodebook_v2.4", new Version(2, 4)) }
            };

        #endregion

        #region Helper classes

        private readonly List<string> _substancesIgnoreListCodeBook2_2 = new() {
                "chol",
                "trigl",
                "sg",
                "lipid",
                "lipid_enz",
                "crt",
                "osm"
            };

        [AcceptedName("SAMPLE")]
        public class EuHbmImportSampleRecord {

            private static readonly Dictionary<string, (string biologicalMatrix, string samplingType)> _matrixMapping
                = new() {
                    { "BWB", ("Blood", "Whole blood") },
                    { "BP", ("BloodPlasma", "Plasma") },
                    { "BS", ("BloodSerum", "Serum") },
                    { "CBWB", ("Cordblood", string.Empty) },
                    { "CBP", ("CordBloodPlasma", "Plasma") },
                    { "CBS", ("CordBloodSerum", "Serum") },
                    { "US", ("Urine", "Spot") },
                    { "UD", ("Urine", "24h") },
                    { "UM", ("Urine", "Morning") },
                    { "SA", ("Saliva", string.Empty) },
                    { "SEM", ("Semen", string.Empty) },
                    { "EBC", ("Breath", "Condensate") },
                    { "RBC", ("RedBloodCells", string.Empty) },
                    { "BM", ("BreastMilk", string.Empty) },
                    { "ADI", ("BodyFat", string.Empty) },
                    { "BWBG", ("WholeBlood", "Whole blood") }, // Deprecated since v2.4
                    { "BPG", ("BloodPlasma", "Plasma") }, // Deprecated since v2.4
                    { "BSG", ("BloodSerum", "Serum") }, // Deprecated since v2.4
                    { "CBWBG", ("CordBlood", "Cord blood") }, // Deprecated since v2.4
                    { "CBSG", ("CordBloodSerum", "Serum") }, // Deprecated since v2.4
                    { "CBPG", ("CordBloodPlasma", "Plasma") }, // Deprecated since v2.4
                    { "H", ("Hair", string.Empty) },
                    { "ATN", ("ToeNails", string.Empty) }, // Deprecated since v2.4
                    { "BTN", ("BigToeNails", string.Empty) }, // Deprecated since v2.4
                    { "DW", ("OuterSkin", "Dermal wipes") },
                    { "AF", ("AmnioticFluid", string.Empty) },
                    { "PLT", ("PlacentaTissue", string.Empty) },
            };

            [AcceptedName("id_sample")]
            public string IdSample { get; set; }

            [AcceptedName("id_subject")]
            public string IdSubject { get; set; }

            [AcceptedName("id_timepoint")]
            public string IdTimepoint { get; set; }

            [AcceptedName("matrix")]
            public string Matrix { get; set; }

            [AcceptedName("samplingyear")]
            public int? SamplingYear { get; set; }

            [AcceptedName("samplingmonth")]
            public int? SamplingMonth { get; set; }

            [AcceptedName("samplingday")]
            public int? SamplingDay { get; set; }

            [AcceptedName("samplingtime")]
            public string SamplingTime { get; set; }

            [AcceptedName("analysisyear")]
            public int? AnalysisYear { get; set; }

            [AcceptedName("analysismonth")]
            public int? AnalysisMonth { get; set; }

            [AcceptedName("analysisday")]
            public int? AnalysisDay { get; set; }

            /// <summary>
            /// Season of sampling.
            /// </summary>
            [AcceptedName("samplingseason")]
            public int? SamplingSeason { get; set; }

            /// <summary>
            /// Method applied for lipid assessment in the sample.
            /// </summary>
            [AcceptedName("lipidassessment")]
            public string LipidAssessmentMethod { get; set; }

            /// <summary>
            /// Cholesterol in sample.
            /// </summary>
            [AcceptedName("chol")]
            public double? Cholesterol { get; set; }

            /// <summary>
            /// Triglycerides in sample.
            /// </summary>
            [AcceptedName("trigl")]
            public double? Triglycerides { get; set; }

            /// <summary>
            /// Lipids un sample.
            /// </summary>
            [AcceptedName("lipid")]
            public double? Lipids { get; set; }

            /// <summary>
            /// Lipids un sample.
            /// </summary>
            [AcceptedName("lipid_enz")]
            public double? LipidEnz { get; set; }

            /// <summary>
            /// Urine density of the sample.
            /// </summary>
            [AcceptedName("density")]
            public double? Density { get; set; }

            /// <summary>
            /// Concentration of creatinine in urine of the sample.
            /// </summary>
            [AcceptedName("crt")]
            public double? Creatinine { get; set; }

            /// <summary>
            /// Osmotic concentration of urine of the sample.
            /// </summary>
            [AcceptedName("osm")]
            public double? OsmoticConcentration { get; set; }

            /// <summary>
            /// Volume of the urine in sample.
            /// </summary>
            [AcceptedName("uvolume")]
            public double? UrineVolume { get; set; }

            /// <summary>
            /// Specific gravity of urine of the sample.
            /// </summary>
            [AcceptedName("sg")]
            public double? SpecificGravity { get; set; }

            /// <summary>
            /// Gets the biological matrix (derived from matrix code).
            /// </summary>
            /// <returns></returns>
            public string GetBiologicalMatrix() {
                if (_matrixMapping.TryGetValue(Matrix, out var result)) {
                    return result.biologicalMatrix;
                }
                return "Undefined";
            }

            /// <summary>
            /// Gets the sampling type/method (derived from matrix code). E.g., 24h or pooled.
            /// </summary>
            /// <returns></returns>
            public string GetSampleType() {
                if (_matrixMapping.TryGetValue(Matrix, out var result)) {
                    return result.samplingType;
                }
                return "Undefined";
            }
        }

        [AcceptedName("TIMEPOINT")]
        public class EuHbmImportTimepointRecord {
            [AcceptedName("id_timepoint")]
            public string IdTimepoint { get; set; }
            [AcceptedName("timepoint_description")]
            public string Description { get; set; }
        }

        [AcceptedName("SUBJECTUNIQUE")]
        public class EuHbmImportSubjectUniqueRecord {
            [AcceptedName("id_subject")]
            public string IdSubject { get; set; }
            [AcceptedName("id_participant")]
            public string IdParticipant { get; set; }
            [AcceptedName("relation")]
            public string Relation { get; set; }
            [AcceptedName("sex")]
            public string Sex { get; set; }
            [AcceptedName("age_birth_m")]
            public int? AgeMother { get; set; }
            [AcceptedName("smoking_m")]
            public string SmokingStatusMother { get; set; }
            [AcceptedName("case_control")]
            public string CaseControl { get; set; }
            [AcceptedName("control_type")]
            public string ControlType { get; set; }
            [AcceptedName("job_task")]
            public int? JobTask { get; set; }
        }

        [AcceptedName("SUBJECTREPEATED")]
        [AcceptedName("SUBJECTTIMEPOINT")]
        public class EuHbmImportSubjectTimepointRecord {
            [AcceptedName("id_subject")]
            public string IdSubject { get; set; }
            [AcceptedName("id_timepoint")]
            [AcceptedName("id_group")]
            public string IdTimepoint { get; set; }
            [AcceptedName("country")]
            public string Country { get; set; }
            [AcceptedName("nuts1")]
            public string Nuts1 { get; set; }
            [AcceptedName("nuts2")]
            public string Nuts2 { get; set; }
            [AcceptedName("nuts3")]
            public string Nuts3 { get; set; }
            [AcceptedName("subdivision")]
            public string Subdivision { get; set; }
            [AcceptedName("height")]
            public double? Height { get; set; }
            [AcceptedName("weight")]
            public double? Weight { get; set; }
            [AcceptedName("ageyears")]
            public int? Age { get; set; }
            [AcceptedName("isced_raw")]
            public int? Isced { get; set; }
            [AcceptedName("isced_m_raw")]
            public int? IscedMother { get; set; }
            [AcceptedName("isced_f_raw")]
            public int? IscedFather { get; set; }
            [AcceptedName("isced_hh_raw")]
            public int? IscedHousehold { get; set; }
            [AcceptedName("smoking")]
            public string SmokingStatus { get; set; }
        }

        public class EuHbmConcentrationRecord {
            [AcceptedName("id_sample")]
            public string IdSample { get; set; }
            [IgnoreField]
            public string IdSubstance { get; set; }
            public double? Concentration { get; set; }
            public double? Lod { get; set; }
            public double? Loq { get; set; }
        }

        public class EuHbmSampleInfoRecord {

            /// <summary>
            /// Sample id, used to match with concentration records
            /// </summary>
            [AcceptedName("id_sample")]
            public string IdSample { get; set; }

            /// <summary>
            /// Cholesterol in sample.
            /// </summary>
            [AcceptedName("chol")]
            public double? Cholesterol { get; set; }

            /// <summary>
            /// Triglycerides in sample.
            /// </summary>
            [AcceptedName("trigl")]
            public double? Triglycerides { get; set; }

            /// <summary>
            /// Lipids un sample.
            /// </summary>
            [AcceptedName("lipid")]
            public double? Lipids { get; set; }

            /// <summary>
            /// Lipids un sample.
            /// </summary>
            [AcceptedName("lipid_enz")]
            public double? LipidEnz { get; set; }

            /// <summary>
            /// Urine density of the sample.
            /// </summary>
            [AcceptedName("density")]
            public double? Density { get; set; }

            /// <summary>
            /// Concentration of creatinine in urine of the sample.
            /// </summary>
            [AcceptedName("crt")]
            public double? Creatinine { get; set; }

            /// <summary>
            /// Osmotic concentration of urine of the sample.
            /// </summary>
            [AcceptedName("osm")]
            public double? OsmoticConcentration { get; set; }

            /// <summary>
            /// Volume of the urine in sample.
            /// </summary>
            [AcceptedName("uvolume")]
            public double? UrineVolume { get; set; }

            /// <summary>
            /// Specific gravity of urine of the sample.
            /// </summary>
            [AcceptedName("sg")]
            public double? SpecificGravity { get; set; }
        }

        #endregion

        public EuHbmImportDataCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            // If the data source reader contains the right tables, then we assume it to
            // be a EU HBM import file and we will try to parse it.
            var tableNames = dataSourceReader.GetTableNames().ToHashSet();
            if (tableNames.Contains("STUDYINFO") && tableNames.Contains("SAMPLE")) {

                // Read study info
                var (studyInfo, codebookVersion) = readStudyInfo(dataSourceReader);

                // Get study ID/Name/Description
                if (!studyInfo.TryGetValue("Study ID", out var surveyCode)) {
                    throw new Exception("Study ID not specified in STUDYINFO sheet.");
                }
                if (!studyInfo.TryGetValue("Study Name", out var surveyName)) {
                    surveyName = surveyCode;
                }
                if (!studyInfo.TryGetValue("Study Description", out var studyDescription)) {
                    studyDescription = string.Empty;
                }
                if (!studyInfo.TryGetValue("Country", out var country)) {
                    country = string.Empty;
                }

                // Read the individuals
                var subjectUniqueRecords = readSubjectUniqueRecords(dataSourceReader);
                var subjectRepeatedRecords = readSubjectRepeatedRecords(dataSourceReader)
                    .ToLookup(r => r.IdSubject);

                // Read all time points
                var codeBookTimepoints = readTimepointRecords(dataSourceReader);
                var timepoints = codeBookTimepoints.Select(c => new RawHumanMonitoringTimepoint {
                     idSurvey = surveyCode,
                     idTimepoint = c.IdTimepoint,
                     Name = c.Description?[..Math.Min(c.Description.Length, 100)],
                     Description = c.Description?[..Math.Min(c.Description.Length, 200)]
                });

                // Create the survey
                var survey = new RawHumanMonitoringSurvey() {
                    idSurvey = surveyCode,
                    Name = surveyName,
                    Description = studyDescription,
                    AgeUnit = "Y",
                    BodyWeightUnit = "kg",
                    Location = country,
                    NumberOfSurveyDays = subjectRepeatedRecords.First().Count(),
                    LipidConcentrationUnit = ConcentrationUnit.mgPerdL.ToString(),
                    TriglycConcentrationUnit = ConcentrationUnit.mgPerdL.ToString(),
                    CholestConcentrationUnit = ConcentrationUnit.mgPerdL.ToString(),
                    CreatConcentrationUnit = ConcentrationUnit.mgPerdL.ToString()
                };
                var surveys = new List<RawHumanMonitoringSurvey>() { survey };
                var ageMotherProperty = !subjectUniqueRecords.All(c => c.AgeMother == null);
                var smokingMotherProperty = !subjectUniqueRecords.All(c => string.IsNullOrEmpty(c.SmokingStatusMother));
                var jobTaskProperty = !subjectUniqueRecords.All(c => c.JobTask == null);
                var genderProperty = !subjectUniqueRecords.All(c => string.IsNullOrEmpty(c.Sex));
                var caseControlProperty = !subjectUniqueRecords.Any(c => string.IsNullOrEmpty(c.CaseControl));
                var controlTypeProperty = !subjectUniqueRecords.All(c => string.IsNullOrEmpty(c.ControlType));
                var ageProperty = !subjectRepeatedRecords.SelectMany(c => c).All(c => c.Age == null);
                var iscedProperty = !subjectRepeatedRecords.SelectMany(c => c).All(c => c.Isced == null);
                var iscedMotherProperty = !subjectRepeatedRecords.SelectMany(c => c).All(c => c.IscedMother == null);
                var iscedFatherProperty = !subjectRepeatedRecords.SelectMany(c => c).All(c => c.IscedFather == null);
                var iscedHouseholdProperty = !subjectRepeatedRecords.SelectMany(c => c).All(c => c.IscedHousehold == null);
                var smokingProperty = !subjectRepeatedRecords.SelectMany(c => c).All(c => string.IsNullOrEmpty(c.SmokingStatus));
                // Add individual properties
                var individualProperties = new List<RawIndividualProperty>();
                if (genderProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "Gender",
                        Name = "Gender",
                        Description = "Gender",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Gender
                    });
                }
                if (ageProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "Age",
                        Name = "Age",
                        Description = "Age",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Integer
                    });
                }
                if (ageMotherProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "AgeMother",
                        Name = "Age of mother at birth",
                        Description = "Age of mother at birth",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Integer
                    });
                }
                if (smokingMotherProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "SmokingStatusMother",
                        Name = "Smoking status of mother",
                        Description = "Smoking status of mother during pregnancy",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Boolean
                    });
                }
                if (caseControlProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "CaseControl",
                        Name = "Case control study",
                        Description = "Indicates whether the subject belongs to the control or the case group",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Boolean
                    });
                }
                if (controlTypeProperty && caseControlProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "ControlType",
                        Name = "Control type",
                        Description = "Indicates whether subject is a  within company (1) or outwith company control(2)",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Categorical
                    });
                }
                if (jobTaskProperty && caseControlProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "JobTask",
                        Name = "Job task",
                        Description = "Description of the work task performed by exposed workers",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.JobTask
                    });
                }
                if (iscedProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "Isced",
                        Name = "ISCED",
                        Description = "Education level (raw)",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Isced
                    });
                }
                if (iscedMotherProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "IscedMother",
                        Name = "ISCED of mother",
                        Description = "Education level of mother (raw)",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Isced
                    });
                }
                if (iscedFatherProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "IscedFather",
                        Name = "ISCED of father",
                        Description = "Education level of father (raw)",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Isced
                    });
                }
                if (iscedHouseholdProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "IscedHousehold",
                        Name = "ISCED of household",
                        Description = "Education level of household (raw)",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Isced
                    });
                }
                if (smokingProperty) {
                    individualProperties.Add(new RawIndividualProperty() {
                        idIndividualProperty = "SmokingStatus",
                        Name = "Smoking status",
                        Description = "Smoking status of subject at sampling",
                        PropertyLevel = PropertyLevelType.Individual,
                        Type = IndividualPropertyType.Boolean
                    });
                }
                var individualPropertyValues = new List<RawIndividualPropertyValue>();

                // Create individuals records
                var individuals = new List<RawIndividual>();
                foreach (var subject in subjectUniqueRecords) {
                    var repeated = subjectRepeatedRecords.Contains(subject.IdSubject)
                        ? subjectRepeatedRecords[subject.IdSubject]
                        : null;
                    var bw = repeated?.Where(r => r.Weight > 0).Select(r => r.Weight).Average();
                    var subjectAge = repeated?.Select(r => r.Age).Average();
                    var subjectIsced = repeated?.FirstOrDefault().Isced;
                    var subjectIscedMother = repeated?.FirstOrDefault().IscedMother;
                    var subjectIscedFather = repeated?.FirstOrDefault().IscedFather;
                    var subjectIscedHousehold = repeated?.FirstOrDefault().IscedHousehold;
                    var subjectSmokingStatus = repeated?.FirstOrDefault().SmokingStatus;
                    // Create and add individual
                    var individual = new RawIndividual {
                        idIndividual = subject.IdSubject,
                        idFoodSurvey = surveyCode,
                        NumberOfSurveyDays = repeated?.Count() ?? 0,
                        BodyWeight = bw,
                    };
                    individuals.Add(individual);

                    // Add individual properties
                    if (!string.IsNullOrEmpty(subject.Sex)) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "Gender",
                            TextValue = subject.Sex,
                        });
                    }
                    if (subject.AgeMother.HasValue) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "AgeMother",
                            DoubleValue = subject.AgeMother,
                        });
                    }
                    if (!string.IsNullOrEmpty(subject.SmokingStatusMother)) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "SmokingStatusMother",
                            TextValue = subject.SmokingStatusMother,
                        });
                    }
                    if (!string.IsNullOrEmpty(subject.CaseControl)) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "CaseControl",
                            TextValue = subject.CaseControl,
                        });
                    }
                    if (!string.IsNullOrEmpty(subject.ControlType)) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "ControlType",
                            TextValue = subject.ControlType,
                        });
                    }
                    if (subject.JobTask.HasValue) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "JobTask",
                            TextValue = ((JobTaskType)subject.JobTask).GetDisplayName(),
                        });
                    }
                    if (subjectAge != null) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "Age",
                            DoubleValue = subjectAge,
                        });
                    }
                    if (subjectIsced != null) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "Isced",
                            TextValue = ((IscedType)subjectIsced).GetDisplayName(),
                        });
                    }
                    if (subjectIscedMother != null) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "IscedMother",
                            TextValue = ((IscedType)subjectIscedMother).GetDisplayName(),
                        });
                    }
                    if (subjectIscedFather != null) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "IscedFather",
                            TextValue = ((IscedType)subjectIscedFather).GetDisplayName(),
                        });
                    }
                    if (subjectIscedHousehold != null) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "IscedHousehold",
                            TextValue = ((IscedType)subjectIscedHousehold).GetDisplayName(),
                        });
                    }
                    if (!string.IsNullOrEmpty(subjectSmokingStatus)) {
                        individualPropertyValues.Add(new RawIndividualPropertyValue() {
                            idIndividual = subject.IdSubject,
                            PropertyName = "SmokingStatus",
                            TextValue = subjectSmokingStatus,
                        });
                    }
                    // NOTE: here you can add other individual properties (mapped from codebook)
                }

                // Derive location from individuals
                var locations = subjectRepeatedRecords.SelectMany(r => r.Select(s => s.Country)).Distinct().ToList();
                if (string.IsNullOrEmpty(survey.Location) && locations.Count == 1) {
                    survey.Location = locations.First();
                }

                // Read the sample records
                var sampleRecords = readSampleRecords(dataSourceReader).ToDictionary(r => r.IdSample);

                //Note in version[2,2] concentrations were moved to other sheet
                var samplesDictionary = sampleRecords.Values
                    .Select(r => new RawHumanMonitoringSample() {
                        idSample = r.IdSample,
                        idIndividual = r.IdSubject,
                        Compartment = r.GetBiologicalMatrix(),
                        SampleType = r.GetSampleType(),
                        SpecificGravity = r.SpecificGravity,
                        DateSampling = r.SamplingYear.HasValue
                            ? new DateTime(r.SamplingYear.Value, r.SamplingMonth ?? 1, r.SamplingDay ?? 1)
                            : null,
                        DayOfSurvey = r.IdTimepoint,
                        LipidGrav = r.Lipids,
                        LipidEnz = r.LipidEnz,
                        Cholesterol = r.Cholesterol,
                        Creatinine = r.Creatinine,
                        Triglycerides = r.Triglycerides,
                        OsmoticConcentration = r.OsmoticConcentration,
                        UrineVolume = r.UrineVolume
                    })
                    .ToDictionary(c => c.idSample);

                // Derive survey start-date and end-date from samples
                var sampleDates = samplesDictionary
                    .Where(r => r.Value.DateSampling != null)
                    .Select(r => r.Value.DateSampling)
                    .ToList();

                // Derive survey start date and end date based on sample dates
                if (sampleDates.Any()) {
                    survey.StartDate = sampleDates.Min();
                    survey.EndDate = sampleDates.Max();
                }

                // Create the MCRA raw table record collections for the samples
                var substances = new Dictionary<string, RawCompound>(StringComparer.OrdinalIgnoreCase);
                var analyticalMethods = new List<RawAnalyticalMethod>();
                var analyticalMethodSubstances = new List<RawAnalyticalMethodCompound>();
                var sampleAnalyses = new List<RawHumanMonitoringSampleAnalysis>();
                var sampleConcentrations = new List<RawHumanMonitoringSampleConcentration>();

                // Read measurements from the per-MATRIX data tables / sheets
                var measurementTables = new List<string>();
                var prefix = string.Empty;
                if (codebookVersion.Version <= new Version(2, 1)) {
                    prefix = "DATA_";
                    measurementTables = dataSourceReader
                        .GetTableNames()
                        .Where(r => r.StartsWith(prefix))
                        .ToList();
                } else if (codebookVersion.Version >= new Version(2, 2)) {
                    prefix = "SAMPLETIMEPOINT_";
                    measurementTables = dataSourceReader
                        .GetTableNames()
                        .Where(r => r.StartsWith(prefix))
                        .ToList();
                }

                var counter = 0;
                foreach (var measurementTable in measurementTables) {
                    // Matrix code is sheet name minus the prefix
                    var matrixCode = measurementTable[prefix.Length..];

                    // Extract the substance codes from the header names
                    List<string> substanceCodes;
                    using (var rdr = dataSourceReader.GetDataReaderByName(measurementTable)) {
                        rdr.Read();
                        var headers = new List<string>();
                        for (int i = 0; i < rdr.FieldCount; i++) {
                            headers.Add(rdr.GetString(i) ?? "");
                        }
                        substanceCodes = headers
                            .Where(r => r.EndsWith("_loq"))
                            .Select(r => r[..^4])
                            .ToList();
                    }

                    // From version 2.2, "chol", "trigl", "sg", "lipid", "lipid_enz", "crt", "osm"
                    // are included in the DATA sheets instead of the SAMPLE sheet.
                    if (codebookVersion.Version >= new Version(2, 2)) {

                        // Remove "chol", "trigl", "sg", "lipid", "lipid_enz", "crt", "osm" from the list of substance codes
                        // These will not be included as substance
                        substanceCodes = substanceCodes
                            .Where(c => !_substancesIgnoreListCodeBook2_2.Contains(c))
                            .ToList();

                        // Get the sample info records containing the values of "chol", "trigl", "sg", "lipid", "lipid_enz", "crt", "osm" per sample
                        var tableDef = TableDefinitionExtensions.FromType(typeof(EuHbmSampleInfoRecord));
                        tableDef.Aliases.Add(measurementTable);
                        var sampleInfoRecords = dataSourceReader
                            .ReadDataTable<EuHbmSampleInfoRecord>(tableDef)
                            .ToList();

                        // Merge the sample info with the (main) sample records
                        foreach (var record in sampleInfoRecords) {
                            if (!samplesDictionary.TryGetValue(record.IdSample, out var sample)) {
                                throw new Exception($"Found reference to non-existing sample '{record.IdSample}' in table {measurementTable}");
                            }
                            sample.Cholesterol = record.Cholesterol;
                            sample.Triglycerides = record.Triglycerides;
                            sample.SpecificGravity = record.SpecificGravity;
                            sample.LipidGrav = record.Lipids;
                            sample.LipidEnz = record.LipidEnz;
                            sample.Creatinine = record.Creatinine;
                            sample.OsmoticConcentration = record.OsmoticConcentration;
                            sample.UrineVolume = record.UrineVolume;
                        }
                    }

                    // Parse the concentrations per substance
                    var concentrations = new List<EuHbmConcentrationRecord>();
                    foreach (var substanceCode in substanceCodes) {

                        // Get or add the substance record
                        if (!substances.TryGetValue(substanceCode, out var substance)) {
                            substance = new RawCompound() {
                                idCompound = substanceCode,
                                Name = substanceCode,
                            };
                            substances.Add(substanceCode, substance);
                        }

                        // Create a table definition for reading the substance concentration records from the data sheet
                        var tableDef = TableDefinitionExtensions.FromType(typeof(EuHbmConcentrationRecord));
                        tableDef.Aliases.Add(measurementTable);
                        tableDef.FindColumnDefinitionByAlias("Concentration").Aliases.Add(substanceCode);
                        tableDef.FindColumnDefinitionByAlias("Loq").Aliases.Add($"{substanceCode}_loq");
                        tableDef.FindColumnDefinitionByAlias("Lod").Aliases.Add($"{substanceCode}_lod");
                        var substanceConcentrations = dataSourceReader.ReadDataTable<EuHbmConcentrationRecord>(tableDef)
                            .Where(r => r.Concentration.HasValue)
                            .ToList();

                        foreach (var concentration in substanceConcentrations) {
                            concentration.IdSubstance = substanceCode;
                        }
                        concentrations.AddRange(substanceConcentrations);
                    }

                    // Group the measurements by sample
                    var measurementsBySample = concentrations
                        .AsParallel()
                        .GroupBy(r => r.IdSample)
                        .Select(gr => new {
                            IdSample = gr.Key,
                            AnalyticalMethodKey = $"{matrixCode}_"
                                + string.Join("-", gr.OrderBy(r => r.IdSubstance).Select(r => $"{r.IdSubstance}_{r.Lod}_{r.Loq}")),
                            SampleConcentrations = gr.ToList(),
                        })
                        .ToList();

                    // Group the sample-measurements by analytical method key
                    var samplesByMethodKey = measurementsBySample
                        .GroupBy(r => r.AnalyticalMethodKey);

                    // Add the records for each group of sample-analyses with the same analytical method
                    foreach (var group in samplesByMethodKey) {

                        // Create the analytical method
                        var idMethod = $"AM-{counter}-{matrixCode}-generated";
                        var method = new RawAnalyticalMethod() {
                            idAnalyticalMethod = idMethod,
                            Name = $"Analytical method {counter} for {matrixCode} (generated)",
                            Description = $"Analytical method for {matrixCode} generated by MCRA from EU HBM import format."
                        };
                        analyticalMethods.Add(method);

                        // Create the analytical method substances
                        var methodSubstances = group.First().SampleConcentrations
                            .Select(r => new RawAnalyticalMethodCompound() {
                                idAnalyticalMethod = idMethod,
                                idCompound = r.IdSubstance,
                                LOD = r.Lod,
                                LOQ = r.Loq,
                                ConcentrationUnit = getConcentrationUnit(matrixCode).GetShortDisplayName()
                            })
                            .ToList();
                        analyticalMethodSubstances.AddRange(methodSubstances);

                        // Create the sample analyses
                        foreach (var sample in group) {
                            // Create the sample analysis
                            var idSampleAnalysis = sample.IdSample;
                            var sampleAnalysis = new RawHumanMonitoringSampleAnalysis() {
                                idSample = sample.IdSample,
                                idSampleAnalysis = idSampleAnalysis,
                                idAnalyticalMethod = idMethod,
                            };
                            sampleAnalyses.Add(sampleAnalysis);

                            // Set analysis date
                            if (sampleRecords[sample.IdSample].AnalysisYear.HasValue) {
                                sampleAnalysis.DateAnalysis = new DateTime(
                                    sampleRecords[sample.IdSample].AnalysisYear.Value,
                                    sampleRecords[sample.IdSample].AnalysisMonth ?? 1,
                                    sampleRecords[sample.IdSample].AnalysisDay ?? 1
                                );
                            }

                            // Create the concentrations
                            foreach (var sampleConcentration in sample.SampleConcentrations) {
                                if (!sampleConcentration.Concentration.HasValue || sampleConcentration.Concentration == -10) {
                                    // Missing value
                                    var concentration = new RawHumanMonitoringSampleConcentration() {
                                        idAnalysisSample = idSampleAnalysis,
                                        idCompound = sampleConcentration.IdSubstance,
                                        ResType = "MV"
                                    };
                                    sampleConcentrations.Add(concentration);
                                } else if (sampleConcentration.Concentration.Value >= 0) {
                                    // Positive measurement
                                    var concentration = new RawHumanMonitoringSampleConcentration() {
                                        idAnalysisSample = idSampleAnalysis,
                                        idCompound = sampleConcentration.IdSubstance,
                                        Concentration = sampleConcentration.Concentration.Value,
                                        ResType = "VAL"
                                    };
                                    sampleConcentrations.Add(concentration);
                                } else if (sampleConcentration.Concentration.Value == -1) {
                                    // LOD measurement
                                    var concentration = new RawHumanMonitoringSampleConcentration() {
                                        idAnalysisSample = idSampleAnalysis,
                                        idCompound = sampleConcentration.IdSubstance,
                                        ResType = "LOD"
                                    };
                                    sampleConcentrations.Add(concentration);
                                } else if (sampleConcentration.Concentration.Value == -2 || sampleConcentration.Concentration.Value == -3) {
                                    // LOD measurement
                                    var concentration = new RawHumanMonitoringSampleConcentration() {
                                        idAnalysisSample = idSampleAnalysis,
                                        idCompound = sampleConcentration.IdSubstance,
                                        ResType = "LOQ"
                                    };
                                    sampleConcentrations.Add(concentration);
                                }
                            }
                        }
                        counter++;
                    }
                }
                // Copy all data tables to the database
                var hasSubstances = tryCopyDataTable(substances.Values.ToDataTable(), RawDataSourceTableID.Compounds);
                var hasSurveys = tryCopyDataTable(surveys.ToDataTable(), RawDataSourceTableID.HumanMonitoringSurveys);
                var hasTimepoints = tryCopyDataTable(timepoints.ToDataTable(), RawDataSourceTableID.HumanMonitoringTimepoints);
                var hasIndividuals = tryCopyDataTable(individuals.ToDataTable(), RawDataSourceTableID.Individuals);
                var hasIndividualProperties = tryCopyDataTable(individualProperties.ToDataTable(), RawDataSourceTableID.IndividualProperties);
                var hasIndividualPropertyValues = tryCopyDataTable(individualPropertyValues.ToDataTable(), RawDataSourceTableID.IndividualPropertyValues);
                var hasSamples = tryCopyDataTable(samplesDictionary.Values.ToDataTable(), RawDataSourceTableID.HumanMonitoringSamples);
                var hasAnalyticalMethods = tryCopyDataTable(analyticalMethods.ToDataTable(), RawDataSourceTableID.AnalyticalMethods);
                var hasAnalyticalMethodSubstances = tryCopyDataTable(analyticalMethodSubstances.ToDataTable(), RawDataSourceTableID.AnalyticalMethodCompounds);
                var hasSampleAnalyses = tryCopyDataTable(sampleAnalyses.ToDataTable(), RawDataSourceTableID.HumanMonitoringSampleAnalyses);
                var hasSampleConcentrations = tryCopyDataTable(sampleConcentrations.ToDataTable(), RawDataSourceTableID.HumanMonitoringSampleConcentrations);

                // Register the table groups
                registerTableGroup(SourceTableGroup.Compounds);
                registerTableGroup(SourceTableGroup.HumanMonitoringData);
            }
        }

        private static (Dictionary<string, string> StudyInfo, CodebookVersion CodebookVersion) readStudyInfo(IDataSourceReader reader) {
            var studyInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var dataReader = reader.GetDataReaderByName("STUDYINFO")) {
                while (dataReader.Read()) {
                    if (!dataReader.IsDBNull(0) && !dataReader.IsDBNull(1)) {
                        var key = dataReader.GetValue(0).ToString();
                        if (key.StartsWith("Study ID", StringComparison.OrdinalIgnoreCase)) {
                            key = "Study ID";
                        }
                        if (key.StartsWith("Country", StringComparison.OrdinalIgnoreCase)) {
                            key = "Country";
                        }
                        var value = dataReader.GetValue(1).ToString();
                        studyInfo.Add(key, value);
                    }
                }
            }
            // Get codebook reference field
            if (!studyInfo.TryGetValue("Codebook Reference", out var codebookReference)) {
                throw new Exception($"Codebook reference not specified.");
            }

            // Check codebook reference and version
            if (!_supportedCodebookVersions.TryGetValue(codebookReference, out var codebookVersion)) {
                throw new Exception($"Codebook reference {codebookReference} not supported.");
            }

            return (StudyInfo: studyInfo, CodebookVersion: codebookVersion);
        }

        private List<EuHbmImportSampleRecord> readSampleRecords(IDataSourceReader reader) {
            var tableDef = TableDefinitionExtensions.FromType(typeof(EuHbmImportSampleRecord));
            using (var dataReader = reader.GetDataReaderByDefinition(tableDef)) {
                var records = reader.ReadDataTable<EuHbmImportSampleRecord>(tableDef);
                return records;
            }
        }

        private List<EuHbmImportSubjectUniqueRecord> readSubjectUniqueRecords(IDataSourceReader reader) {
            var tableDef = TableDefinitionExtensions.FromType(typeof(EuHbmImportSubjectUniqueRecord));
            using (var dataReader = reader.GetDataReaderByDefinition(tableDef)) {
                var records = reader.ReadDataTable<EuHbmImportSubjectUniqueRecord>(tableDef);
                return records;
            }
        }

        private List<EuHbmImportSubjectTimepointRecord> readSubjectRepeatedRecords(IDataSourceReader reader) {
            var tableDef = TableDefinitionExtensions.FromType(typeof(EuHbmImportSubjectTimepointRecord));
            using (var dataReader = reader.GetDataReaderByDefinition(tableDef)) {
                var records = reader.ReadDataTable<EuHbmImportSubjectTimepointRecord>(tableDef);
                return records;
            }
        }

        private List<EuHbmImportTimepointRecord> readTimepointRecords(IDataSourceReader reader) {
            var tableDef = TableDefinitionExtensions.FromType(typeof(EuHbmImportTimepointRecord));
            using (var dataReader = reader.GetDataReaderByDefinition(tableDef)) {
                var records = reader.ReadDataTable<EuHbmImportTimepointRecord>(tableDef);
                return records;
            }
        }

        private ConcentrationUnit getConcentrationUnit(string matrix) {
            var ugPerLMatrices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "BWB", "BP", "BS",
                "CBWB", "CBP", "CBS",
                "US", "UD", "UM",
                "SA", "SEM", "EBC", "RBC", "BM",
                "ADI", "AF"
            };
            var ugPergMatrices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "BWBG", "BPG", "BSG",
                "CBWBG", "CBPG", "CBSG",
                "BMG"
            };
            var ngPergMatrices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "H", "ATN", "BTN",
                "PLT"
            };
            var ugPercm2GMatrices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "DW"
            };

            if (ugPerLMatrices.Contains(matrix)) {
                return ConcentrationUnit.ugPerL;
            }
            if (ugPergMatrices.Contains(matrix)) {
                return ConcentrationUnit.ugPerg; ;
            }
            if (ngPergMatrices.Contains(matrix)) {
                return ConcentrationUnit.ngPerg;
            }
            if (ugPercm2GMatrices.Contains(matrix)) {
                throw new NotImplementedException("Reading of ug/cm2 matrices not yet implemented.");
            }
            throw new ArgumentException($"Unknown matrix {matrix}.");
        }
    }
}
