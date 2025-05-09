﻿using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using System.Data;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        #region SSD temporary data reading / conversion

        private const string _tempSqliteTableName = "samples";

        private const string _tempSsdTableCreateSql =
            @"CREATE TABLE samples (
                [labSampCode] nvarchar(30) NULL,
                [labSubSampCode] [nvarchar](4) NULL,
                [sampCountry] [nvarchar](2) NULL,
                [sampArea] [nvarchar](5) NULL,
                [prodCode] [varchar](50) NOT NULL,
                [prodProdMeth] [nvarchar](5) NULL,
                [sampStrategy] [nvarchar](50) NULL,
                [fieldTrialType] [nvarchar](50) NULL,
                [sampY] [int] NULL,
                [sampM] [int] NULL,
                [sampD] [int] NULL,
                [analysisY] [int] NULL,
                [analysisM] [int] NULL,
                [analysisD] [int] NULL,
                [paramCode] [nvarchar](50) NOT NULL,
                [resUnit] [nvarchar](5) NOT NULL,
                [resLOD] [float] NULL,
                [resLOQ] [float] NULL,
                [resVal] [float] NULL,
                [resType] [nvarchar](3) NULL);
              CREATE UNIQUE INDEX ix_primary_tempssd
              ON samples (labSampCode, labSubSampCode, sampCountry, sampY, sampM, sampD, prodCode, paramCode);";

        private const string _tempSsdSelectSql =
            @"SELECT labSampCode, labSubSampCode, sampCountry, sampArea, prodCode, prodProdMeth, sampStrategy, fieldTrialType,
                sampY, sampM, sampD, analysisY, analysisM, analysisD, paramCode, resUnit, resLOD, resLOQ, resVal, resType
            FROM samples ORDER BY labSampCode, labSubSampCode, sampCountry, sampY, sampM, sampD, prodCode, paramCode";

        private const string _tempTabulatedCreateSql =
            @"CREATE TABLE samples (
                [GUID] [nvarchar](50) NULL,
                [idCompound] [nvarchar](50) NOT NULL,
                [idFood] [nvarchar](50) NOT NULL,
                [Year] [nvarchar](50) NULL,
                [Month] [nvarchar](50) NULL,
                [SamplingType] [nvarchar](50) NULL,
                [Location] [nvarchar](50) NULL,
                [NumberOfSamples] [int] NOT NULL,
                [Concentration] [float] NOT NULL,
                [DateSampling] [nvarchar](10) NULL,
                [ConcentrationUnit] nvarchar(50) NULL);";

        private const string _tempTabulatedSelectSql =
            @"SELECT [GUID], [idCompound], [idFood], [Year], [Month], [SamplingType], [Location],
                     [NumberOfSamples], [Concentration], [DateSampling], [ConcentrationUnit]
              FROM samples ORDER BY [GUID], [Location], [Year], [Month], [DateSampling], [idFood], [idCompound]";

        private const string _tempTabulatedMethodSql =
            @"SELECT DISTINCT [idCompound], [ConcentrationUnit], [Concentration]
              FROM samples WHERE [Concentration] <= 0
              UNION
              SELECT [idCompound], [ConcentrationUnit], Min([Concentration])
              FROM samples WHERE [Concentration] > 0
              GROUP BY [idCompound], [ConcentrationUnit]";

        private enum SSDFields {
            labSampCode,
            labSubSampCode,
            sampCountry,
            sampArea,
            prodCode,
            prodProdMeth,
            sampStrategy,
            fieldTrialType,
            sampY,
            sampM,
            sampD,
            analysisY,
            analysisM,
            analysisD,
            paramCode,
            resUnit,
            resLOD,
            resLOQ,
            resVal,
            resType
        }

        private enum TabulatedFields {
            Guid,
            idCompound,
            idFood,
            Year,
            Month,
            SamplingType,
            Location,
            NumberOfSamples,
            Concentration,
            DateSampling,
            ConcentrationUnit
        }

        private enum TabulatedMethodFields {
            idCompound,
            ConcentrationUnit,
            Concentration
        }

        private const string _sep = "\a";

        private class SSDMethodRecord {
            public string CompoundCode { get; set; }
            public double? LOR { get { return LOQ ?? LOD; } }
            public double? LOD { get; set; }
            public double? LOQ { get; set; }
            public string ConcentrationUnit { get; set; }
            public override string ToString() {
                return string.Join(_sep, CompoundCode.ToLower(), LOQ, LOD, ConcentrationUnit);
            }
        }

        private class SSDSampleRecord {
            public string Code { get; set; }
            public int SubSampleCount { get; set; }
            public string Location { get; set; }
            public string Region { get; set; }
            public string FoodCode { get; set; }
            public string ProductionMethod { get; set; }
            public string ProgramStrategyCode { get; set; }
            public string FieldTrialType { get; set; }
            public DateTime? SamplingDate { get; set; }
            public DateTime? AnalysisDate { get; set; }
        }

        #endregion

        public ConcentrationsBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        /// <summary>
        /// Implements <see cref="RawDataSourceBulkCopierBase.tryCopyDataTable(IDataSourceReader, ProgressState)"/>.
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="progressState"></param>
        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {

            var hasSSDConcentrations = tryDoSsdConcentrationsBulkCopy(dataSourceReader, progressState);
            if (hasSSDConcentrations) {
                if (tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SampleProperties, true)) {
                    tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SamplePropertyValues, true);
                }
                registerTableGroup(SourceTableGroup.FocalFoods);
                registerTableGroup(SourceTableGroup.Concentrations);
            }

            var hasTabulatedConcentrations = tryDoTabulatedConcentrationsBulkCopy(dataSourceReader, progressState);
            if (hasTabulatedConcentrations && hasSSDConcentrations) {
                var message = "Cannot upload databases containing concentrations data in multiple formats.";
                throw new RawDataSourceBulkCopyException(message);
            } else if (hasTabulatedConcentrations) {
                registerTableGroup(SourceTableGroup.FocalFoods);
                registerTableGroup(SourceTableGroup.Concentrations);
            }

            progressState.Update("Processing sample-based concentrations", 66);
            var hasRelationalConcentrationData = dataSourceReader.HasDataForTableDefinition(
                McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.FoodSamples)
            );

            if (hasRelationalConcentrationData && (hasSSDConcentrations || hasTabulatedConcentrations)) {
                var message = "Cannot upload databases containing concentrations data in multiple formats.";
                throw new RawDataSourceBulkCopyException(message);
            } else if (hasRelationalConcentrationData) {
                if (tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SampleProperties)) {
                    tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodSamples);
                    tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SamplePropertyValues);
                } else {
                    tryDoBulkCopyWithDynamicProperties(
                        dataSourceReader,
                        RawDataSourceTableID.FoodSamples,
                        RawDataSourceTableID.SampleProperties,
                        RawDataSourceTableID.SamplePropertyValues
                    );
                }

                if (!_parsedDataTables.Contains(RawDataSourceTableID.AnalyticalMethods)) {
                    hasRelationalConcentrationData &= tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalyticalMethods);
                    hasRelationalConcentrationData &= tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalyticalMethodCompounds);
                }
                hasRelationalConcentrationData &= tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalysisSamples);
                hasRelationalConcentrationData &= tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConcentrationsPerSample);
                if (hasRelationalConcentrationData) {
                    registerTableGroup(SourceTableGroup.FocalFoods);
                    registerTableGroup(SourceTableGroup.Concentrations);
                } else {
                    const string message = "Cannot process incomplete concentration data.";
                    throw new RawDataSourceBulkCopyException(message);
                }
            }

            progressState.Update(100);
        }

        private bool tryDoTabulatedConcentrationsBulkCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            progressState.Update("Processing tabulated concentrations", 33);

            string sourceTableName = null;
            var sqliteDbFileName = Path.Combine(Path.GetTempPath(), $"_tempTabCopy{Guid.NewGuid():N}.sqlite");

            try {
                var tableDef = _tableDefinitions[RawDataSourceTableID.ConcentrationTabulated];
                using var tableReader = dataSourceReader.GetDataReaderByDefinition(tableDef, out sourceTableName);
                if (sourceTableName == null || tableReader == null) {
                    return false;
                }

                // Create the temp raw tabulated concentrations table in sqlite format
                using (var tmpDatabaseWriter = new SqLiteDataSourceWriter(sqliteDbFileName)) {
                    // Create the temp raw SSD concentrations table
                    progressState.Update("Preparing raw tabulated concentrations data", 5);
                    tmpDatabaseWriter.Open();
                    tmpDatabaseWriter.RunSql(_tempTabulatedCreateSql);
                    tmpDatabaseWriter.Write(tableReader, tableDef, _tempSqliteTableName, progressState);

                    if (progressState.CancellationToken.IsCancellationRequested) {
                        progressState.Update("Preparing raw tabulated data cancelled...");
                        return false;
                    }

                    // Prepare data tables
                    var concTables = createConcentrationTables();

                    var rawAnalyticalMethodsTable = concTables[RawDataSourceTableID.AnalyticalMethods];
                    var rawAnalyticalMethodCompoundsTable = concTables[RawDataSourceTableID.AnalyticalMethodCompounds];
                    var rawAnalysisSamplesTable = concTables[RawDataSourceTableID.AnalysisSamples];
                    var rawFoodSamplesTable = concTables[RawDataSourceTableID.FoodSamples];
                    var rawConcentrationsTable = concTables[RawDataSourceTableID.ConcentrationsPerSample];

                    // prepare analytical methods by running a query first
                    var methodIdDictionary = new Dictionary<string, string>();
                    var methodIdCounter = 0;

                    using (var command = tmpDatabaseWriter.CreateSQLiteCommand(_tempTabulatedMethodSql)) {
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                var methodCode = $"AM{methodIdCounter++}";
                                var concentration = reader.GetDouble(TabulatedMethodFields.Concentration, null);
                                var lor = concentration < 0
                                        ? -concentration
                                        : concentration == 0
                                            ? 1E-08
                                            : concentration / 10;

                                var idCompound = reader.GetString(TabulatedMethodFields.idCompound, null);
                                var concUnit = reader.GetStringOrNull(TabulatedMethodFields.ConcentrationUnit, null);

                                var methodKey = concentration > 0
                                              ? $"{idCompound}{_sep}{concUnit}"
                                              : $"{idCompound}{_sep}{concUnit}{_sep}{concentration}";

                                methodIdDictionary.Add(methodKey, methodCode);

                                // Add to analyticalMethods and analyticalMethodCompounds tables
                                var ram = rawAnalyticalMethodsTable.NewRow();
                                ram[nameof(RawAnalyticalMethods.IdAnalyticalMethod)]= methodCode;
                                ram[nameof(RawAnalyticalMethods.Description)] = concentration > 0
                                    ? $"{methodCode} from tabulated import positive concentration records"
                                    : $"{methodCode} from tabulated import censored value records";
                                rawAnalyticalMethodsTable.Rows.Add(ram);

                                var ramc = rawAnalyticalMethodCompoundsTable.NewRow();
                                ramc[nameof(RawAnalyticalMethodCompounds.IdAnalyticalMethod)] = methodCode;
                                ramc[nameof(RawAnalyticalMethodCompounds.IdCompound)] = idCompound;
                                ramc[nameof(RawAnalyticalMethodCompounds.LOD)] = lor;
                                ramc[nameof(RawAnalyticalMethodCompounds.LOQ)] = lor;
                                ramc[nameof(RawAnalyticalMethodCompounds.ConcentrationUnit)] = concUnit;
                                rawAnalyticalMethodCompoundsTable.Rows.Add(ramc);
                            }
                        }
                    }

                    using (var command = tmpDatabaseWriter.CreateSQLiteCommand(_tempTabulatedSelectSql)) {
                        using (var reader = command.ExecuteReader()) {
                            var sampleCodeDuplicates = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            var analysisSampleCount = 0;
                            string analysisSampleCode;

                            if (progressState.CancellationToken.IsCancellationRequested) {
                                progressState.Update("Processing raw tabulated data cancelled...");
                                return false;
                            }

                            progressState.Update("Processing raw tabulated data", 40);
                            var ntrans = 0;
                            var sampleCodeCounter = 0;
                            var sampleCodes = new HashSet<string>();
                            while (reader.Read()) {
                                if (++ntrans % 92551 == 0) {
                                    if (progressState.CancellationToken.IsCancellationRequested) {
                                        progressState.Update("Processing raw tabulated data cancelled...");
                                        return false;
                                    }
                                    progressState.Update($"Processing raw tabulated data: {ntrans / 1000}K records processed");
                                }

                                int[] mapper = null;
                                var foodCode = reader.GetString(TabulatedFields.idFood, mapper);
                                var compoundCode = reader.GetString(TabulatedFields.idCompound, mapper);
                                var location = reader.GetStringOrNull(TabulatedFields.Location, mapper) ?? string.Empty;
                                var year = reader.GetStringOrNull(TabulatedFields.Year, mapper);

                                // Construct samplingdate
                                bool hasSamplingDate = DateTime.TryParse(reader.GetStringOrNull(TabulatedFields.DateSampling, mapper), out var samplingDate);
                                if (!hasSamplingDate) {
                                    if (int.TryParse(year, out int yearInt)) {
                                        if (int.TryParse(reader.GetStringOrNull(TabulatedFields.Month, mapper), out int month)
                                                && month > 0 && month < 13) {
                                            samplingDate = new DateTime(yearInt, month, 1);
                                        } else {
                                            samplingDate = new DateTime(yearInt, 1, 1);
                                        }
                                        hasSamplingDate = true;
                                    }
                                }
                                var unit = reader.GetStringOrNull(TabulatedFields.ConcentrationUnit, mapper);
                                var concentration = reader.GetDouble(TabulatedFields.Concentration, mapper);
                                var lor = concentration < 0 ? -concentration : 0;

                                //number of samples from the record
                                var numberOfSamples = reader.GetIntOrNull(TabulatedFields.NumberOfSamples, mapper) ?? 1;
                                //create the key for the analytical method, based on the substance code,
                                //concentration unit and optionally the concentration (if it's a LOR)
                                var methodKey = concentration > 0
                                              ? $"{compoundCode}{_sep}{unit}"
                                              : $"{compoundCode}{_sep}{unit}{_sep}{concentration}";
                                //get the already added method based on the key
                                var methodCode = methodIdDictionary[methodKey];
                                var sampleCode = reader.GetStringOrNull(TabulatedFields.Guid, mapper);
                                var generateCode = string.IsNullOrEmpty(sampleCode) || sampleCodes.Contains(sampleCode);
                                if (generateCode) {
                                    sampleCode = $"{++sampleCodeCounter:X}";
                                }
                                var foodSampleCount = 0;

                                for (int i = 0; i < numberOfSamples; i++) {
                                    var foodSampleCode = $"{sampleCode}-{++foodSampleCount:X}";
                                    var rfs = rawFoodSamplesTable.NewRow();
                                    rfs[nameof(RawFoodSamples.IdFoodSample)] = foodSampleCode;
                                    rfs[nameof(RawFoodSamples.IdFood)] = foodCode;
                                    rfs[nameof(RawFoodSamples.Location)] = location;
                                    rfs[nameof(RawFoodSamples.DateSampling)] = samplingDate;
                                    rawFoodSamplesTable.Rows.Add(rfs);

                                    analysisSampleCode = $"{++analysisSampleCount:X}";
                                    var ras = rawAnalysisSamplesTable.NewRow();
                                    ras[nameof(RawAnalysisSamples.IdAnalysisSample)] = analysisSampleCode;
                                    ras[nameof(RawAnalysisSamples.IdFoodSample)] = foodSampleCode;
                                    ras[nameof(RawAnalysisSamples.IdAnalyticalMethod)] = methodCode;
                                    if (hasSamplingDate) {
                                        ras[nameof(RawAnalysisSamples.DateAnalysis)] = samplingDate;
                                    }
                                    rawAnalysisSamplesTable.Rows.Add(ras);

                                    //only add concentration data where the concentration is positive
                                    if (concentration > 0) {
                                        var rc = rawConcentrationsTable.NewRow();
                                        rc[nameof(RawConcentrationsPerSample.IdAnalysisSample)]= analysisSampleCode;
                                        rc[nameof(RawConcentrationsPerSample.IdCompound)] = compoundCode;
                                        rc[nameof(RawConcentrationsPerSample.Concentration)] = concentration;
                                        rc[nameof(RawConcentrationsPerSample.ResType)] = ResType.VAL.ToString();
                                        rawConcentrationsTable.Rows.Add(rc);
                                    }
                                }
                                sampleCodes.Add(sampleCode);
                            }
                        }
                    }

                    //Datatables are filled, now simply bulk copy to SQL Server tables
                    saveConcentrationTables(concTables, progressState);

                    return true;
                }
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            } finally {
                //Delete the temp raw tabulated concentrations table in sqlite format
                if (File.Exists(sqliteDbFileName)) {
                    try {
                        File.Delete(sqliteDbFileName);
                    } catch (Exception) {
                        progressState.Update($"File could not be deleted ({sqliteDbFileName}");
                    }
                }
            }
        }

        /// <summary>
        /// Copies the records of an SSD datasource into the destination MCRA tables for substances, analyticalMethods, samples and concentrations
        /// For this to work efficiently, the input data must be ordered, therefore it is first imported into a SQL server temp table containing an index
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        private bool tryDoSsdConcentrationsBulkCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            progressState.Update("Processing SSD concentrations");

            string sourceTableName = null;
            var sqliteDbFileName = Path.Combine(Path.GetTempPath(), $"_tempSSDCopy{Guid.NewGuid():N}.sqlite");
            try {
                var ssdTableDefinition = _tableDefinitions[RawDataSourceTableID.ConcentrationsSSD];
                using var ssdTableReader = dataSourceReader.GetDataReaderByDefinition(ssdTableDefinition, out sourceTableName);
                if (sourceTableName == null || ssdTableReader == null) {
                    return false;
                }

                // Create the temp raw SSD concentrations table in sqlite format
                using (var tmpDatabaseWriter = new SqLiteDataSourceWriter(sqliteDbFileName)) {
                    // Create the temp raw SSD concentrations table
                    progressState.Update("Preparing raw SSD data", 5);
                    tmpDatabaseWriter.Open();
                    tmpDatabaseWriter.RunSql(_tempSsdTableCreateSql);
                    tmpDatabaseWriter.Write(ssdTableReader, ssdTableDefinition, _tempSqliteTableName, progressState);

                    if (progressState.CancellationToken.IsCancellationRequested) {
                        progressState.Update("Preparing raw SSD data cancelled...");
                        return false;
                    }

                    // Prepare data tables
                    var concTables = createConcentrationTables();

                    var rawAnalyticalMethodsTable = concTables[RawDataSourceTableID.AnalyticalMethods];
                    var rawAnalyticalMethodCompoundsTable = concTables[RawDataSourceTableID.AnalyticalMethodCompounds];
                    var rawAnalysisSamplesTable = concTables[RawDataSourceTableID.AnalysisSamples];
                    var rawFoodSamplesTable = concTables[RawDataSourceTableID.FoodSamples];
                    var rawConcentrationsTable = concTables[RawDataSourceTableID.ConcentrationsPerSample];
                    var rawSamplePropertiesTable = concTables[RawDataSourceTableID.SampleProperties];
                    var rawSamplePropertyValuesTable = concTables[RawDataSourceTableID.SamplePropertyValues];

                    //fill the raw sample properties with predefined values
                    rawSamplePropertiesTable.Rows.Add(SamplingStrategyPropertyName, "Sampling strategy code");
                    rawSamplePropertiesTable.Rows.Add(FieldTrialTypePropertyName, "Field trial type");

                    using (var command = tmpDatabaseWriter.CreateSQLiteCommand(_tempSsdSelectSql)) {
                        using (var reader = command.ExecuteReader()) {

                            SSDSampleRecord currentSample = null;
                            var currentMethod = new List<SSDMethodRecord>();
                            var anMethodsByHash = new Dictionary<ulong, string>();
                            var anMethCounter = 0;
                            var currentSampleCode = string.Empty;
                            var currentProdCode = string.Empty;
                            var currentCountry = string.Empty;
                            DateTime? currentSampleDate = null;
                            var sampleCount = 0;
                            var uniqueSampleCode = string.Empty;
                            var sampleCodeDuplicates = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                            bool addSampleAndMethod() {
                                //add last sample and method:
                                //analytical method, if there is a current sample
                                if (currentSample != null) {
                                    // Get a hash for the current analytical method that has been built up so far, by creating a string
                                    // of the analyticalmethodcompound records in the current method,
                                    // the records are already sorted on compound code so similar lists will result in similar hashes
                                    var s = string.Join(_sep, currentMethod.Select(m => m.ToString()));
                                    var hash = ((ulong)s.Length << 32) | (uint)s.GetHashCode();

                                    if (!anMethodsByHash.TryGetValue(hash, out var cachedMethod)) {
                                        cachedMethod = "AM" + anMethCounter.ToString();
                                        anMethCounter++;
                                        anMethodsByHash[hash] = cachedMethod;

                                        // Add to analyticalMethods and analyticalMethodCompounds tables
                                        var ramRow = rawAnalyticalMethodsTable.NewRow();
                                        ramRow[nameof(RawAnalyticalMethods.IdAnalyticalMethod)] = cachedMethod;
                                        ramRow[nameof(RawAnalyticalMethods.Description)] = $"{cachedMethod} from SSD import";
                                        rawAnalyticalMethodsTable.Rows.Add(ramRow);

                                        foreach (var mc in currentMethod) {
                                            var ramcRow = rawAnalyticalMethodCompoundsTable.NewRow();
                                            ramcRow[nameof(RawAnalyticalMethodCompounds.IdAnalyticalMethod)] = cachedMethod;
                                            ramcRow[nameof(RawAnalyticalMethodCompounds.IdCompound)] = mc.CompoundCode;
                                            if (mc.LOD.HasValue) {
                                                ramcRow[nameof(RawAnalyticalMethodCompounds.LOD)] = mc.LOD;
                                            }
                                            if (mc.LOQ.HasValue) {
                                                ramcRow[nameof(RawAnalyticalMethodCompounds.LOQ)] = mc.LOQ;
                                            }
                                            ramcRow[nameof(RawAnalyticalMethodCompounds.ConcentrationUnit)] = mc.ConcentrationUnit;
                                            rawAnalyticalMethodCompoundsTable.Rows.Add(ramcRow);
                                        }
                                        currentMethod = [];
                                    }
                                    sampleCount++;

                                    var rasRow = rawAnalysisSamplesTable.NewRow();
                                    rasRow[nameof(RawAnalysisSamples.IdAnalysisSample)] = currentSample.Code;
                                    rasRow[nameof(RawAnalysisSamples.IdFoodSample)] = currentSample.Code;
                                    rasRow[nameof(RawAnalysisSamples.IdAnalyticalMethod)] = cachedMethod;
                                    if (currentSample.AnalysisDate.HasValue) {
                                        rasRow[nameof(RawAnalysisSamples.DateAnalysis)] = currentSample.AnalysisDate;
                                    }
                                    rawAnalysisSamplesTable.Rows.Add(rasRow);

                                    var rfsRow = rawFoodSamplesTable.NewRow();
                                    rfsRow[nameof(RawFoodSamples.IdFoodSample)] = currentSample.Code;
                                    rfsRow[nameof(RawFoodSamples.IdFood)] = currentSample.FoodCode;
                                    rfsRow[nameof(RawFoodSamples.Location)] = currentSample.Location;
                                    rfsRow[nameof(RawFoodSamples.Region)] = currentSample.Region;
                                    rfsRow[nameof(RawFoodSamples.ProductionMethod)] = currentSample.ProductionMethod;
                                    if (currentSample.SamplingDate.HasValue) {
                                        rfsRow[nameof(RawFoodSamples.DateSampling)] = currentSample.SamplingDate;
                                    }
                                    rawFoodSamplesTable.Rows.Add(rfsRow);

                                    //add sample property values (if any)
                                    if (!string.IsNullOrEmpty(currentSample.ProgramStrategyCode)) {
                                        var rspvRow = rawSamplePropertyValuesTable.NewRow();
                                        rspvRow[nameof(RawSamplePropertyValues.IdSample)] = currentSample.Code;
                                        rspvRow[nameof(RawSamplePropertyValues.PropertyName)] = SamplingStrategyPropertyName;
                                        rspvRow[nameof(RawSamplePropertyValues.TextValue)] = currentSample.ProgramStrategyCode;
                                        rawSamplePropertyValuesTable.Rows.Add(rspvRow);
                                    }
                                    if (!string.IsNullOrEmpty(currentSample.FieldTrialType)) {
                                        var rspvRow = rawSamplePropertyValuesTable.NewRow();
                                        rspvRow[nameof(RawSamplePropertyValues.IdSample)] = currentSample.Code;
                                        rspvRow[nameof(RawSamplePropertyValues.PropertyName)] = FieldTrialTypePropertyName;
                                        rspvRow[nameof(RawSamplePropertyValues.TextValue)] = currentSample.FieldTrialType;
                                        rawSamplePropertyValuesTable.Rows.Add(rspvRow);
                                    }
                                }
                                return true;
                            }

                            if (progressState.CancellationToken.IsCancellationRequested) {
                                progressState.Update("Processing raw SSD data cancelled...");
                                return false;
                            }

                            progressState.Update("Processing raw SSD data", 40);
                            var ntrans = 0;
                            while (reader.Read()) {
                                if (++ntrans % 92551 == 0) {
                                    if (progressState.CancellationToken.IsCancellationRequested) {
                                        progressState.Update("Processing raw SSD data cancelled...");
                                        return false;
                                    }
                                    progressState.Update($"Processing raw SSD data: {ntrans / 1000}K records processed");
                                }

                                int[] mapper = null;
                                var sampleCode = reader.GetStringOrNull(SSDFields.labSampCode, mapper) +
                                    (reader.IsDBNull(SSDFields.labSubSampCode, mapper)
                                        ? "" : "_" + reader.GetString(SSDFields.labSubSampCode, mapper));

                                var prodCode = reader.GetString(SSDFields.prodCode, mapper);
                                var paramCode = reader.GetString(SSDFields.paramCode, mapper);
                                var sampCountry = reader.GetStringOrNull(SSDFields.sampCountry, mapper) ?? string.Empty;
                                var sampArea = reader.GetStringOrNull(SSDFields.sampArea, mapper) ?? string.Empty;
                                var prodMeth = reader.GetStringOrNull(SSDFields.prodProdMeth, mapper) ?? string.Empty;

                                var sampleDate = FromSsdYmd(reader.GetIntOrNull(SSDFields.sampY, mapper), reader.GetIntOrNull(SSDFields.sampM, mapper), reader.GetIntOrNull(SSDFields.sampD, mapper));
                                var analysisDate = FromSsdYmd(reader.GetIntOrNull(SSDFields.analysisY, mapper), reader.GetIntOrNull(SSDFields.analysisM, mapper), reader.GetIntOrNull(SSDFields.analysisD, mapper));

                                var loq = reader.GetDoubleOrNull(SSDFields.resLOQ, mapper);
                                var lod = reader.GetDoubleOrNull(SSDFields.resLOD, mapper);
                                var unit = reader.GetString(SSDFields.resUnit, mapper);

                                if (currentSampleCode.Equals(sampleCode, StringComparison.OrdinalIgnoreCase)
                                    && currentProdCode.Equals(prodCode, StringComparison.OrdinalIgnoreCase)
                                    && currentCountry.Equals(sampCountry, StringComparison.OrdinalIgnoreCase)
                                    && currentSampleDate == sampleDate
                                ) {

                                    // Add current concentration record to current sample
                                    if (currentSample != null) {
                                        // Current analytical method
                                        // add current record to analytical method
                                        currentMethod.Add(new SSDMethodRecord {
                                            CompoundCode = paramCode,
                                            ConcentrationUnit = unit,
                                            LOD = lod,
                                            LOQ = loq,
                                        });
                                    }
                                } else {
                                    // Analytical method, if there is a current sample
                                    addSampleAndMethod();

                                    // Check whether sample code is equal to the previous code, if so, we have a
                                    // duplicate based on either prodCode, country, sampleDate and/or analysisDate
                                    // Now we need to create a new uniqueSampleCode: use the dictionary
                                    if (currentSampleCode.Equals(sampleCode, StringComparison.OrdinalIgnoreCase)) {
                                        sampleCodeDuplicates.TryGetValue(sampleCode, out var duplicate);
                                        sampleCodeDuplicates[sampleCode] = ++duplicate;
                                        // Create the unique sample code using the duplicate counter for the current sample code
                                        // Use a ` (backtick) quote to separate the counter value
                                        uniqueSampleCode = $"{sampleCode}:{duplicate}";
                                    } else {
                                        uniqueSampleCode = sampleCode;
                                    }

                                    currentSample = new SSDSampleRecord {
                                        Code = uniqueSampleCode,
                                        FoodCode = reader.GetString(SSDFields.prodCode, mapper),
                                        Location = reader.GetStringOrNull(SSDFields.sampCountry, mapper),
                                        Region = reader.GetStringOrNull(SSDFields.sampArea, mapper),
                                        ProductionMethod = reader.GetStringOrNull(SSDFields.prodProdMeth, mapper),
                                        ProgramStrategyCode = reader.GetStringOrNull(SSDFields.sampStrategy, mapper),
                                        FieldTrialType = reader.GetStringOrNull(SSDFields.fieldTrialType, mapper),
                                        SamplingDate = sampleDate,
                                        AnalysisDate = analysisDate
                                    };

                                    currentMethod = [
                                        new SSDMethodRecord {
                                            CompoundCode = paramCode,
                                            ConcentrationUnit = unit,
                                            LOD = lod,
                                            LOQ = loq,
                                        }
                                    ];

                                    // Set the current values to compare the next record
                                    currentSampleCode = sampleCode;
                                    currentProdCode = prodCode;
                                    currentCountry = sampCountry;
                                    currentSampleDate = sampleDate;
                                }

                                var resType = reader.GetString(SSDFields.resType, mapper);
                                var isValue = resType.Equals("VAL", StringComparison.OrdinalIgnoreCase);
                                var isLoq = resType.Equals("LOQ", StringComparison.OrdinalIgnoreCase);
                                var isLod = resType.Equals("LOD", StringComparison.OrdinalIgnoreCase);

                                // Only add if the ResType is one of the accepted formats
                                if (isValue || isLoq || isLod) {
                                    var resVal = reader.GetDoubleOrNull(SSDFields.resVal, mapper);
                                    if (isValue && (resVal ?? -1) < 0) {
                                        // If ResType == 'VAL' then we expect a concentration
                                        throw new Exception($"Missing positive for result specified with ResType 'VAL'");
                                    } else if (isLod && (lod ?? -1) <= 0) {
                                        // If ResType == 'LOD' then we expect a concentration
                                        throw new Exception($"Missing LOD for result specified with ResType 'LOD'");
                                    } else if (isLoq && (loq ?? -1) <= 0) {
                                        // If ResType == 'LOQ' then we expect a concentration
                                        throw new Exception($"Missing LOQ for result specified with ResType 'LOQ'");
                                    }

                                    //Only save Values and LOD values explicitly in table
                                    //LOQ values are implied when a concentration is not in the concentrationsPerSample data
                                    //they are taken from the AnalyticalMethodCompounds as LOQ by default
                                    if (!isLoq) {
                                        var rcRow = rawConcentrationsTable.NewRow();
                                        rcRow[nameof(RawConcentrationsPerSample.IdAnalysisSample)] = uniqueSampleCode;
                                        rcRow[nameof(RawConcentrationsPerSample.IdCompound)] = paramCode;
                                        if (resVal.HasValue) {
                                            rcRow[nameof(RawConcentrationsPerSample.Concentration)] = resVal.Value;
                                        }
                                        rcRow[nameof(RawConcentrationsPerSample.ResType)] = resType;
                                        rawConcentrationsTable.Rows.Add(rcRow);
                                    }
                                } else {
                                    // Specified ResType not known/supported
                                    throw new Exception($"Specified ResType '{resType}' not supported.");
                                }
                            }
                            // Add last sample and method:
                            // Analytical method, if there is a current sample
                            addSampleAndMethod();
                        }
                    }

                    //Datatables are filled, now simply bulk copy to SQL Server tables
                    saveConcentrationTables(concTables, progressState);

                    return true;
                }
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            } finally {
                //Delete the temp raw SSD concentrations table in sqlite format
                if (File.Exists(sqliteDbFileName)) {
                    try {
                        File.Delete(sqliteDbFileName);
                    } catch (Exception) {
                        progressState.Update($"File could not be deleted ({sqliteDbFileName}");
                    }
                }
            }
        }

        private static DateTime? FromSsdYmd(int? year, int? month, int? day) {
            if (!year.HasValue) {
                return null;
            }
            // Addeddays based on given day parameter: should be between 1 and 31
            // Subtract 1 to get days-to-add to first of month
            var addedDays = day.HasValue ? (day.Value < 1 ? 1 : (day.Value > 31 ? 31 : day.Value)) - 1 : 0;
            // Select year between 1900 and 3000
            // month between 1 and 12
            // Add addedDays to first of the month: an invalid day will still yield a valid DateTime
            return new DateTime(year < 1900 ? 1900 : (year > 3000 ? 3000 : year.Value),
                                month.HasValue ? (month < 1 ? 1 : (month > 12 ? 12 : month.Value)) : 1, 1).AddDays(addedDays);
        }

        private static readonly RawDataSourceTableID[] _rawDataSourceTableIDs = [
            RawDataSourceTableID.AnalyticalMethods,
            RawDataSourceTableID.AnalyticalMethodCompounds,
            RawDataSourceTableID.AnalysisSamples,
            RawDataSourceTableID.FoodSamples,
            RawDataSourceTableID.ConcentrationsPerSample,
            RawDataSourceTableID.SampleProperties,
            RawDataSourceTableID.SamplePropertyValues
        ];

        private const string SamplingStrategyPropertyName = "sampStrategy";
        private const string FieldTrialTypePropertyName = "FieldTrialType";

        private static IDictionary<RawDataSourceTableID, DataTable> createConcentrationTables() {
            // Create data tables as a dictionary
            var dict = _rawDataSourceTableIDs.Select(t => new KeyValuePair<RawDataSourceTableID, DataTable> (
                t, McraTableDefinitions.Instance.GetTableDefinition(t).CreateDataTable()
            )).ToDictionary();

            return dict;
        }

        private void saveConcentrationTables(IDictionary<RawDataSourceTableID, DataTable> tables, ProgressState progressState) {
            var progress = 75D;
            var progressIncrement = 20D / _rawDataSourceTableIDs.Length;

            //Datatables are filled, now simply bulk copy to SQL Server tables
            foreach (var tableId in _rawDataSourceTableIDs) {
                progressState.Update($"Copying data to {tableId} table", progress);
                if (tables[tableId].Rows.Count > 0) {
                    tryCopyDataTable(tables[tableId], tableId);
                }
                progress += progressIncrement;
            }
        }
    }
}
