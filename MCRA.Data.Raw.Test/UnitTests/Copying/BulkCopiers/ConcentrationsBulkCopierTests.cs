using System.Data;
using System.Runtime.Versioning;
using MCRA.Data.Raw.Converters;
using MCRA.Data.Raw.Copying;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;
using RTA = MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    [TestClass]
    public class ConcentrationsBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Test concentration data bulk copier. Copy relational data.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopyRelational() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Concentrations/Relational"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;

                var foodSampleFoods = tables["RawFoodSamples"].Rows
                    .OfType<DataRow>()
                    .Select(r => r["idFood"])
                    .GroupBy(r => r)
                    .ToDictionary(r => r.Key, r => r.Count());

                // Sample counts
                Assert.AreEqual(3, foodSampleFoods["APPLE"]);
                Assert.AreEqual(3, foodSampleFoods["BANANAS"]);

                var samplePropertiesTableDef = getTableDefinition(RawDataSourceTableID.SampleProperties);
                var additionalSampleProperties = getDistinctColumnValues<string>(
                    tables[samplePropertiesTableDef.TargetDataTable],
                    nameof(RawSampleProperties.Name)
                ).ToArray();
                CollectionAssert.AreEquivalent(additionalSampleProperties, new[] { "Season", "SeasonType" });

                var samplePropertyValuesTableDef = getTableDefinition(RawDataSourceTableID.SamplePropertyValues);
                var additionalSamplePropertyValues = getDistinctColumnValues<string>(
                    tables[samplePropertyValuesTableDef.TargetDataTable],
                    nameof(RawSamplePropertyValues.TextValue)
                ).ToArray();
                CollectionAssert.AreEquivalent(additionalSamplePropertyValues, new[] { "Winter", "Summer", "Import", "Export" });

            }
        }

        /// <summary>
        /// Test copying of SSD concentration data.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopySsd() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new ExcelFileReader(TestUtils.GetResource("Concentrations/ConcentrationsSSD.xls"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;
                var VALCount = getColumnValues<string>(tables["RawConcentrationsPerSample"], nameof(RawConcentrationsPerSample.ResType))
                    .Count(c => c == ResType.VAL.ToString());
                var LODLOQCount = getColumnValues<string>(tables["RawConcentrationsPerSample"], nameof(RawConcentrationsPerSample.ResType))
                    .Count(c => c != ResType.VAL.ToString());
                Assert.AreEqual(5, tables["RawAnalyticalMethods"].Rows.Count);
                Assert.AreEqual(12, tables["RawAnalyticalMethodCompounds"].Rows.Count);
                Assert.AreEqual(20, tables["RawFoodSamples"].Rows.Count);
                Assert.AreEqual(20, tables["RawAnalysisSamples"].Rows.Count);
                Assert.AreEqual(32, VALCount);
                Assert.AreEqual(6, LODLOQCount);

                var foodSampleFoods = tables["RawFoodSamples"].Rows
                    .OfType<DataRow>()
                    .Select(r => r["idFood"])
                    .GroupBy(r => r)
                    .ToDictionary(r => r.Key, r => r.Count());

                // Sample counts
                Assert.AreEqual(5, foodSampleFoods["APPLE"]);
                Assert.AreEqual(5, foodSampleFoods["BANANAS"]);
                Assert.AreEqual(10, foodSampleFoods["PINEAPPLE"]);

                var locations = getDistinctColumnValues<string>(tables["RawFoodSamples"], nameof(RawFoodSamples.Location)).ToArray();
                CollectionAssert.AreEquivalent(locations, new[] { "NL", "DE", null });

                var regions = getDistinctColumnValues<string>(tables["RawFoodSamples"], nameof(RawFoodSamples.Region)).ToArray();
                CollectionAssert.AreEquivalent(regions, new[] { "NL1", "NL2", "DE1", "DE2", null });

                var productionMethods = getDistinctColumnValues<string>(tables["RawFoodSamples"], nameof(RawFoodSamples.ProductionMethod)).ToArray();
                CollectionAssert.AreEquivalent(productionMethods, new[] { "PD07A", "PD06A", null });
            }
        }

        /// <summary>
        /// Test bulkcopying of SSD data with additional sample properties specified
        /// in separate tables.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestFail_MissingConcentration() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Concentrations/SSD-MissingConcentration"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                Assert.ThrowsExactly<RawDataSourceBulkCopyException>(() => bulkCopier.TryCopy(reader, new ProgressState()));
            }
        }

        /// <summary>
        /// Test bulkcopying of SSD data with multiple sample analyses for some samples,
        /// specified via the sampAnId field.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestCopySSDSampAnId() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Concentrations/SSD-sampAnId"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                // Get generated raw tables from writer
                var tables = dataSourceWriter.DataTables;
                
                var rawFoodSamples = getRawDataRecords<RTA.RawFoodSample>(tables["RawFoodSamples"]);
                Assert.AreEqual(20, rawFoodSamples.Count);

                var rawSampleAnalyses = getRawDataRecords<RTA.RawAnalysisSample>(tables["RawAnalysisSamples"]);
                Assert.AreEqual(22, rawSampleAnalyses.Count);

                var rawSampleConcentrations = getRawDataRecords<RTA.RawConcentrationsPerSample>(tables["RawConcentrationsPerSample"]);

                // First sample has two sample analyses
                Assert.AreEqual(
                    expected: 2,
                    actual: rawSampleAnalyses.Where(r => r.idFoodSample == "FS01").Count()
                );

                // First sample has two sample analyses
                CollectionAssert.AreEquivalent(
                    expected: new [] { "SA1", "SA2" },
                    actual: rawSampleAnalyses
                        .Where(r => r.idFoodSample == "FS01")
                        .Select(r => r.idAnalysisSample)
                        .ToArray()
                );

                // Sample analysis IDs renamed for SA1 and SA2 of FS02
                CollectionAssert.AreEquivalent(
                    expected: new[] { "SA1:1", "SA2:1" },
                    actual: rawSampleAnalyses
                        .Where(r => r.idFoodSample == "FS02")
                        .Select(r => r.idAnalysisSample)
                        .ToArray()
                );

                // When not specified, sample analysis ID is equal to food sample
                Assert.AreEqual(
                    expected: "FS03",
                    actual: rawSampleAnalyses.Single(r => r.idFoodSample == "FS03").idAnalysisSample
                );

                // Assert sample concentration record counts
                var concentrationRecordCounts = rawSampleConcentrations
                    .GroupBy(r => r.idAnalysisSample)
                    .ToDictionary(r => r.Key, r => r.Count());
                Assert.AreEqual(concentrationRecordCounts["SA1"], 3);
                Assert.AreEqual(concentrationRecordCounts["SA2"], 2);
                Assert.AreEqual(concentrationRecordCounts["SA1:1"], 3);
                Assert.AreEqual(concentrationRecordCounts["SA2:1"], 3);
            }
        }

        /// <summary>
        /// Test bulkcopying of SSD data with multiple sample analyses for some samples,
        /// specified via the sampAnId and/or anPortSeq field.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestCopySSDAnPortSeq() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Concentrations/SSD-anPortSeq"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                // Get generated raw tables from writer
                var tables = dataSourceWriter.DataTables;

                var rawFoodSamples = getRawDataRecords<RTA.RawFoodSample>(tables["RawFoodSamples"]);
                Assert.AreEqual(4, rawFoodSamples.Count);

                var rawSampleAnalyses = getRawDataRecords<RTA.RawAnalysisSample>(tables["RawAnalysisSamples"]);
                Assert.AreEqual(7, rawSampleAnalyses.Count);

                var rawSampleConcentrations = getRawDataRecords<RTA.RawConcentrationsPerSample>(tables["RawConcentrationsPerSample"]);

                // First sample has three sample analyses
                CollectionAssert.AreEquivalent(
                    expected: new[] { "SA1:1", "SA1:2", "SA2" },
                    actual: rawSampleAnalyses
                        .Where(r => r.idFoodSample == "FS01")
                        .Select(r => r.idAnalysisSample)
                        .ToArray()
                );

                // Sample analysis IDs of FS02 based on sample ID and anPortSeq
                CollectionAssert.AreEquivalent(
                    expected: new[] { "FS02:1", "FS02:2" },
                    actual: rawSampleAnalyses
                        .Where(r => r.idFoodSample == "FS02")
                        .Select(r => r.idAnalysisSample)
                        .ToArray()
                );

                // When only anPortSeq is specified, sample analysis ID is equal to food sample and anPortSeq
                Assert.AreEqual(
                    expected: "FS03:1",
                    actual: rawSampleAnalyses.Single(r => r.idFoodSample == "FS03").idAnalysisSample
                );

                // When anPortSeq and sampAnId not specified, sample analysis ID is equal to food sample
                Assert.AreEqual(
                    expected: "FS04",
                    actual: rawSampleAnalyses.Single(r => r.idFoodSample == "FS04").idAnalysisSample
                );

                // Assert sample concentration record counts
                var concentrationRecordCounts = rawSampleConcentrations
                    .GroupBy(r => r.idAnalysisSample)
                    .ToDictionary(r => r.Key, r => r.Count());
                Assert.AreEqual(concentrationRecordCounts["SA1:1"], 3);
                Assert.AreEqual(concentrationRecordCounts["SA1:2"], 3);
                Assert.AreEqual(concentrationRecordCounts["SA2"], 2);
            }
        }

        /// <summary>
        /// Test bulkcopying of SSD data with additional sample properties specified
        /// in separate tables.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopySsdWithAdditionalProperties() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Concentrations/SSD-Complete"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;
                var VALCount = getColumnValues<string>(tables["RawConcentrationsPerSample"], nameof(RawConcentrationsPerSample.ResType))
                    .Count(c => c == ResType.VAL.ToString());
                var LODLOQCount = getColumnValues<string>(tables["RawConcentrationsPerSample"], nameof(RawConcentrationsPerSample.ResType))
                    .Count(c => c != ResType.VAL.ToString());
                Assert.AreEqual(5, tables["RawAnalyticalMethods"].Rows.Count);
                Assert.AreEqual(12, tables["RawAnalyticalMethodCompounds"].Rows.Count);
                Assert.AreEqual(20, tables["RawFoodSamples"].Rows.Count);
                Assert.AreEqual(20, tables["RawAnalysisSamples"].Rows.Count);
                Assert.AreEqual(32, VALCount);
                Assert.AreEqual(6, LODLOQCount);
                Assert.AreEqual(3, tables["RawSampleProperties"].Rows.Count);
                Assert.AreEqual(23, tables["RawSamplePropertyValues"].Rows.Count);

                var properties = getDistinctColumnValues<string>(tables["RawSampleProperties"], nameof(RawSampleProperties.Name)).Order();
                Assert.AreEqual("FieldTrialType|sampStrategy|Season", string.Join('|', properties));

                var propertyValues = getDistinctColumnValues<string>(tables["RawSamplePropertyValues"], nameof(RawSamplePropertyValues.TextValue)).Order();
                Assert.AreEqual("stgyA|stgyB|stgyC|Summer|trialA|trialB|Winter", string.Join('|', propertyValues));

                var samplePropertyValues = tables["RawSamplePropertyValues"].Rows
                    .OfType<DataRow>()
                    .Select(r => (idSample: r["idSample"], idProperty: r["PropertyName"], value: r["TextValue"] ?? r["DoubleValue"]))
                    .GroupBy(r => r.idSample)
                    .ToDictionary(r => r.Key, r => r.ToDictionary(s => s.idProperty, s => s.value));
                Assert.AreEqual("Winter", samplePropertyValues["1"]["Season"]);
                Assert.AreEqual("Winter", samplePropertyValues["2"]["Season"]);
                Assert.AreEqual("Summer", samplePropertyValues["6"]["Season"]);
                Assert.AreEqual("Summer", samplePropertyValues["7"]["Season"]);
            }
        }

        /// <summary>
        /// Test import of SSD concentration data with additional entity code conversion.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopySsdWithEntityRecoding() {
            var substanceCodeConversions = new EntityCodeConversionsCollection() {
                IdEntity = "Compounds",
                ConversionTuples = [
                    new ("CompoundA", "SubstanceA"),
                    new ("CompoundX", "SubstanceX")
                ]
            };
            var foodCodeConversions = new EntityCodeConversionsCollection() {
                IdEntity = "Foods",
                ConversionTuples = [
                    new ("APPLE", "XXX_APPLE_XXX"),
                ]
            };

            var dataTableDataSourceWriter = new DataTableDataSourceWriter();
            var dataSourceWriter = new RecodingDataSourceWriter(
                dataTableDataSourceWriter,
                substanceCodeConversions,
                foodCodeConversions
            );
            using (var reader = new ExcelFileReader(TestUtils.GetResource("Concentrations/ConcentrationsSSD.xls"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataTableDataSourceWriter.DataTables;

                var substancesAnalyticalMethodCompounds = tables["RawAnalyticalMethodCompounds"].Rows
                    .OfType<DataRow>().Select(r => r["idCompound"].ToString()).Distinct().ToList();

                Assert.IsTrue(substancesAnalyticalMethodCompounds.Contains("SubstanceA"));
                Assert.IsFalse(substancesAnalyticalMethodCompounds.Contains("CompoundA"));

                var foodSampleFoods = tables["RawFoodSamples"].Rows
                    .OfType<DataRow>().Select(r => r["idFood"]).Distinct().ToList();

                Assert.IsTrue(foodSampleFoods.Contains("XXX_APPLE_XXX"));
                Assert.IsFalse(foodSampleFoods.Contains("APPLE"));
            }
        }

        /// <summary>
        /// Test import of SSD concentration data with additional entity code conversion.
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopySsdWithEntityRecodingFromXmlConfig() {
            var recodings = EntityCodeConversionConfiguration
                .FromXmlFile(TestUtils.GetResource("Concentrations/ConcentrationsRecodeConfig.xml"));

            var dataTableDataSourceWriter = new DataTableDataSourceWriter();
            var dataSourceWriter = new RecodingDataSourceWriter(
                dataTableDataSourceWriter,
                recodings.EntityCodeConversions
            );
            using (var reader = new ExcelFileReader(TestUtils.GetResource("Concentrations/ConcentrationsSSD.xls"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataTableDataSourceWriter.DataTables;

                var substancesAnalyticalMethodCompounds = tables["RawAnalyticalMethodCompounds"].Rows
                    .OfType<DataRow>().Select(r => r["idCompound"].ToString()).Distinct().ToList();

                Assert.IsTrue(substancesAnalyticalMethodCompounds.Contains("SubstanceA"));
                Assert.IsFalse(substancesAnalyticalMethodCompounds.Contains("CompoundA"));

                var foodSampleFoods = tables["RawFoodSamples"].Rows
                    .OfType<DataRow>().Select(r => r["idFood"]).Distinct().ToList();

                Assert.IsTrue(foodSampleFoods.Contains("XXX_APPLE_XXX"));
                Assert.IsFalse(foodSampleFoods.Contains("APPLE"));
            }
        }

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopyTabulated
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopyTabulatedConcentration() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new ExcelFileReader(TestUtils.GetResource("Concentrations/ConcentrationsTabulated.xls"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;
                Assert.AreEqual(10, tables["RawAnalyticalMethods"].Rows.Count);
                Assert.AreEqual(10, tables["RawAnalyticalMethodCompounds"].Rows.Count);
                Assert.AreEqual(30, tables["RawFoodSamples"].Rows.Count);
                Assert.AreEqual(30, tables["RawAnalysisSamples"].Rows.Count);
                Assert.AreEqual(15, tables["RawConcentrationsPerSample"].Rows.Count);

                var foodSampleFoods = tables["RawFoodSamples"].Rows
                    .OfType<DataRow>()
                    .Select(r => r["idFood"])
                    .GroupBy(r => r)
                    .ToDictionary(r => r.Key, r => r.Count());

                Assert.AreEqual(15, foodSampleFoods["APPLE"]);
                Assert.AreEqual(9, foodSampleFoods["BANANAS"]);
                Assert.AreEqual(6, foodSampleFoods["PINEAPPLE"]);

                var lods = tables["RawAnalyticalMethodCompounds"].Rows
                    .OfType<DataRow>()
                    .ToLookup(r => r["idCompound"], r => r[ResType.LOD.ToString()]);
                CollectionAssert.AreEquivalent(new double[] { 0.01, 0.01, 0.05 }, lods["CompoundA"].ToArray());
                CollectionAssert.AreEquivalent(new double[] { 0.01, 0.05 }, lods["CompoundB"].ToArray());
                CollectionAssert.AreEquivalent(new double[] { 0.03, 0.05 }, lods["CompoundC"].ToArray());
                CollectionAssert.AreEquivalent(new double[] { 1E-08, 0.02 }, lods["CompoundD"].ToArray());
                CollectionAssert.AreEquivalent(new double[] { 1E-08 }, lods["CompoundE"].ToArray());
            }
        }

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopyTabulated
        /// </summary>
        [TestMethod]
        [SupportedOSPlatform("windows")]
        public void ConcentrationDataBulkCopier_TestBulkCopyConcentrations() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new AccessDataFileReader(TestUtils.GetResource("DataGroupsTests.mdb"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;
                Assert.AreEqual(5, tables["RawAnalyticalMethods"].Rows.Count);
                Assert.AreEqual(12, tables["RawAnalyticalMethodCompounds"].Rows.Count);
                Assert.AreEqual(20, tables["RawFoodSamples"].Rows.Count);
                Assert.AreEqual(20, tables["RawAnalysisSamples"].Rows.Count);
                Assert.AreEqual(32, tables["RawConcentrationsPerSample"].Rows.Count);
            }
        }

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopyTabulated
        /// </summary>
        [TestMethod]
        [SupportedOSPlatform("windows")]
        public void ConcentrationDataBulkCopier_TestBulkCopyConcentrationsWithEntityRecoding() {
            var config = new EntityCodeConversionConfiguration() {
                EntityCodeConversions = [
                    new() {
                        IdEntity = "Compounds",
                        ConversionTuples = [
                            new ("CompoundA", "SubstanceA"),
                            new ("CompoundX", "SubstanceX")
                        ]
                    },
                    new() {
                        IdEntity = "Foods",
                        ConversionTuples = [
                            new ("APPLE", "XXX_APPLE_XXX"),
                        ]
                    }
                ]
            };
            var configXml = XmlSerialization.ToXml(config);

            var dataTableDataSourceWriter = new DataTableDataSourceWriter();
            var dataSourceWriter = new RecodingDataSourceWriter(
                dataTableDataSourceWriter,
                config.EntityCodeConversions
            );

            using (var reader = new AccessDataFileReader(TestUtils.GetResource("DataGroupsTests.mdb"))) {
                reader.Open();
                var bulkCopier = new ConcentrationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataTableDataSourceWriter.DataTables;

                var substancesAnalyticalMethodCompounds = tables["RawAnalyticalMethodCompounds"].Rows
                    .OfType<DataRow>().Select(r => r["idCompound"].ToString()).Distinct().ToList();

                Assert.IsTrue(substancesAnalyticalMethodCompounds.Contains("SubstanceA"));
                Assert.IsFalse(substancesAnalyticalMethodCompounds.Contains("CompoundA"));

                var foodSampleFoods = tables["RawFoodSamples"].Rows
                    .OfType<DataRow>().Select(r => r["idFood"]).Distinct().ToList();

                Assert.IsTrue(foodSampleFoods.Contains("XXX_APPLE_XXX"));
                Assert.IsFalse(foodSampleFoods.Contains("APPLE"));
            }
        }
    }
}
