using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Text;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposuresByCompound, CoExposure
    /// </summary>
    [TestClass]
    public class CoExposureTotalDistributionSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize co-exposure target exposures chronic, test CoExposureTotalDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureTotalDistributionSection_TestSummarizeTargetExposuresChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var pointsOfDeparture = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances);
            var exposures = MockTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);

            var section = new CoExposureTotalDistributionSection();
            section.Summarize(exposures, null, substances);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize co-exposure target exposures acute, test CoExposureTotalDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureTotalDistributionSection_TestSummarizeTargetExposuresAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var pointsOfDeparture = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances);
            var exposures = MockTargetExposuresGenerator.MockIndividualDayExposures(individualDays, substances, random);

            var section = new CoExposureTotalDistributionSection();
            section.Summarize(null, exposures, substances);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize co-exposure dietary exposures acute, test CoExposureTotalDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureTotalDistributionSection_TestSummarizeDietaryExposuresAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new CoExposureTotalDistributionSection();
            section.Summarize(exposures, substances, ExposureType.Acute);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize co-exposure dietary exposures chronic, test CoExposureTotalDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureTotalDistributionSection_TestSummarizeDietaryExposuresChronic() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new CoExposureTotalDistributionSection();
            section.Summarize(exposures, substances, ExposureType.Chronic);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }


        [TestMethod, TestCategory("Sandbox Tests")]
        public void ExposurePatternsAlgorithmCompareOutputTest() {
            //var data = createData(50, 50);
            var data = createData(100,20000);
            var result01 = getExposurePatternFrequencies1(data.records.ToList(), data.compounds).ToArray();

            var result02 = getExposurePatternFrequencies2(data.records.ToList(), data.compounds).ToArray();

            Assert.AreEqual(result02.Length, result01.Length);

            var sb01 = new StringBuilder();
            var sb02 = new StringBuilder();
            for (int i = 0; i < result01.Length; i++) {
                //sb01.AppendLine($"{i:000000}: freq {result01[i].Frequency:0000} perc {result01[i].Percentage:00.00} subst {result01[i].Substances}");
                //sb02.AppendLine($"{i:000000}: freq {result02[i].Frequency:0000} perc {result02[i].Percentage:00.00} subst {result02[i].Substances}");
                sb01.AppendLine($"{i:00},f{result01[i].Frequency},p{result01[i].Percentage:00.0},{result01[i].Substances}");
                sb02.AppendLine($"{i:00},f{result02[i].Frequency},p{result02[i].Percentage:00.0},{result02[i].Substances}");
            }
            var s01 = sb01.ToString();
            var s02 = sb02.ToString();

            Assert.AreEqual(s01, s02);
        }

        [TestMethod, TestCategory("Sandbox Tests")]
        public void ExposurePatternsAlgorithmCompareSpeedTest() {
            var data = createData(150, 20000);

            var sw = Stopwatch.StartNew();

            var result01 = getExposurePatternFrequencies1(data.records.ToList(), data.compounds).ToArray();
            sw.Stop();
            Debug.WriteLine($"algorithm 01: {sw.Elapsed}");
            Assert.IsNotNull(result01);
            sw.Reset();
            sw.Start();
            var result02 = getExposurePatternFrequencies2(data.records.ToList(), data.compounds).ToArray();
            sw.Stop();
            Debug.WriteLine($"algorithm 02: {sw.Elapsed}");
            Assert.AreEqual(result02.Length, result01.Length);
        }

        [TestMethod, TestCategory("Sandbox Tests")]
        public void ExposurePatternsAlgorithm2Test() {
            var data = createData(8, 8);

            var result = getExposurePatternFrequencies2(data.records.ToList(), data.compounds).ToArray();
            Assert.AreEqual(8, result.Length);

            var sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++) {
                sb.AppendLine($"{i:00},f{result[i].Frequency},p{result[i].Percentage:00.0},{result[i].Substances}");
            }
            const string check =
                "00,f8,p50.0,C00\r\n"
                + "01,f8,p50.0,C01\r\n"
                + "02,f8,p50.0,C02\r\n"
                + "03,f4,p25.0,C00, C01\r\n"
                + "04,f4,p25.0,C00, C02\r\n"
                + "05,f4,p25.0,C01, C02\r\n"
                + "06,f2,p12.5,\r\n"
                + "07,f2,p12.5,C00, C01, C02\r\n";

            var s01 = sb.ToString();
            Assert.AreEqual(check, s01);
        }

        private (List<DetailCoExposureRecord> records, ICollection<Compound> compounds) createData(int compoundCount, int listCount) {
            var compounds = new Compound[compoundCount];
            for (int i = 0; i < compoundCount; i++) {
                compounds[i] = new Compound($"C{i:X2}");
            }
            var records = new DetailCoExposureRecord[listCount];

            for (int i = 0; i < listCount; i++) {
                var bp = new BitPattern32(new[] { (uint)i });
                records[i] = new DetailCoExposureRecord {
                    Binary = bp,
                    Frequency = 2, //i % 10,
                    NumberOfSubstances = bp.NumberOfSetBits,
                    Index = bp.IndicesOfSetBits.ToArray(),
                    Row = i
                };
            };
            return (records.ToList(), compounds);
        }

        private (List<DetailCoExposureRecord> records, ICollection<Compound> compounds) createData() {
            var compounds = new Compound[40];
            for (int i = 0; i < 40; i++) {
                compounds[i] = new Compound($"C{i:X2}");
            }

            var list = new string[] {
                "0000000000000000000000000100000000000000",
                "0000000000000000000000000100000000000000",
                "0000000000000000000000000100000000000000",
                "0000000000000000000000000000000000010000",
                "0000000000000000000000000000000000010000",
                "0000000000000000000000000000000000010000",
                "0000000010000000000000000000000000010000",
                "1000000010000000000000000000000000010000",
                "0000000010000001000000000000000000010000",
                "0000000000000001000000001000000000000000",
                "0000000000000001000000001100000000000000",
                "1000000000000001000000001100000000000000",
                "0100000000000001000000001100000000000000",
                "1100000000000001000000001100000000000000",
                "1000000000000001000000001100010000000000",
                "0100000000000001000000001100011000000000",
                "1100000000000001000000001100000100000000",
                "0100000000000001000000001100011000010000",
                "1100000000000001000000001100000100001000",
                "0000000010100001000000000000000000010000",
                "0000000010010001000000000000000000010000",
                "0000000010000001000000001100000000010000",
                "0000000000000000000000000000000000000000",
            };
            var records = new DetailCoExposureRecord[list.Length];

            for (int i = 0; i < list.Length; i++) {
                var bp = new BitPattern32(list[i]);
                records[i] = new DetailCoExposureRecord {
                    Binary = bp,
                    Frequency = 2, //i % 10,
                    NumberOfSubstances = bp.NumberOfSetBits,
                    Index = bp.IndicesOfSetBits.ToArray(),
                    Row = i
                };
            };
            return (records.ToList(), compounds);
        }

        //Algorithms comparison
        //for bitwise grouping
        /// <summary>
        /// Calculate the contribution of patterns containing the combination of patterns
        /// </summary>
        /// <param name="groupedExposurePatterns"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        private List<FullCoExposureRecord> getExposurePatternFrequencies1(List<DetailCoExposureRecord> groupedExposurePatterns, ICollection<Compound> substances) {
            var groupedExposurePatternsCount = groupedExposurePatterns.Sum(c => c.Frequency);
            var results = new FullCoExposureRecord[groupedExposurePatterns.Count];
            var orderedRecords = groupedExposurePatterns.OrderBy(p => p.NumberOfSubstances).ToArray();

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1000 };

            Parallel.ForEach(orderedRecords, parallelOptions, group => {
                //only higher numbers are relevant for binary patterns
                var sets = orderedRecords.Where(c => c.NumberOfSubstances > group.NumberOfSubstances).ToList();
                var frequency = group.Frequency;
                if (group.NumberOfSubstances > 0) {
                    foreach (var ix in group.Index) {
                        sets.RemoveAll(c => !c.Binary.Get(ix));
                        if (sets.Count == 0) {
                            break;
                        }
                    }
                    frequency += sets.Sum(c => c.Frequency);
                }
                results[group.Row] = new FullCoExposureRecord() {
                    Frequency = frequency,
                    Substances = getCompoundNames(group.Index, substances),
                    NumberOfSubstances = group.NumberOfSubstances,
                    Percentage = 100D * frequency / groupedExposurePatternsCount,
                };
            });
            return results.OrderByDescending(c => c.Percentage).ThenBy(c => c.Substances).ToList();
        }

        //Algorithms comparison
        //for bitwise grouping
        /// <summary>
        /// Calculate the contribution of patterns containing the combination of patterns
        /// </summary>
        /// <param name="groupedExposurePatterns"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        private List<FullCoExposureRecord> getExposurePatternFrequencies2(List<DetailCoExposureRecord> groupedExposurePatterns, ICollection<Compound> substances) {
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1000 };

            var groupedExposurePatternsCount = groupedExposurePatterns.Sum(c => c.Frequency);
            var results = new FullCoExposureRecord[groupedExposurePatterns.Count];

            //create an array of arrays grouped by number of substances, ordered descending by number of substances
            var groupedRecords = groupedExposurePatterns
                .GroupBy(p => p.NumberOfSubstances)
                .OrderBy(g => g.Key)
                .Select(g => g.ToArray())
                .ToArray();

            //next steps, fill all records with one order lower number of substances and sum frequencies
            Parallel.For(0, groupedRecords.Length, parallelOptions, i => {
                var editGroup = groupedRecords[i];
                //iterate over rest of groups, this can be done parallel
                Parallel.For(0, editGroup.Length, parallelOptions, j => {
                    var editItem = editGroup[j];
                    var frequency = editItem.Frequency;
                    //loop over all sets with length > editgroup
                    if(editItem.NumberOfSubstances > 0) {
                        for (int l = i + 1; l < groupedRecords.Length; l++) {
                            //loop over individual items
                            frequency += groupedRecords[l]
                                .Where(r => editItem.Binary.IsSubSetOf(r.Binary))
                                .Sum(r => r.Frequency);
                        }
                    }
                    results[editItem.Row] = new FullCoExposureRecord() {
                        Frequency = frequency,
                        Substances = getCompoundNames(editItem.Index, substances),
                        NumberOfSubstances = editItem.NumberOfSubstances,
                        Percentage = 100D * frequency / groupedExposurePatternsCount
                    };
                });
            });

            return results.OrderByDescending(c => c.Percentage).ThenBy(c => c.Substances).ToList();
        }

        //Algorithms comparison
        //for bitwise grouping
        /// <summary>
        /// Calculate the contribution of patterns containing the combination of patterns
        /// </summary>
        /// <param name="groupedExposurePatterns"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        private List<FullCoExposureRecord> getExposurePatternFrequencies4(List<DetailCoExposureRecord> groupedExposurePatterns, ICollection<Compound> substances) {
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1000 };

            var groupedExposurePatternsCount = groupedExposurePatterns.Sum(c => c.Frequency);
            var results = new FullCoExposureRecord[groupedExposurePatterns.Count];
            var orderedRecords = groupedExposurePatterns.OrderBy(p => p.NumberOfSubstances).ToArray();

            Parallel.ForEach(orderedRecords, parallelOptions, group => {
                //only higher numbers are relevant for binary patterns
                var frequency = group.Frequency;
                if (group.NumberOfSubstances > 0) {
                    var sets = groupedExposurePatterns.Where(c => c.NumberOfSubstances > group.NumberOfSubstances).ToList();
                    frequency += sets
                        .Where(s => group.Binary.IsSubSetOf(s.Binary))
                        .Sum(c => c.Frequency);
                }
                results[group.Row] = new FullCoExposureRecord() {
                    Frequency = frequency,
                    Substances = getCompoundNames(group.Index, substances),
                    NumberOfSubstances = group.NumberOfSubstances,
                    Percentage = 100D * frequency / groupedExposurePatternsCount,
                };
            });
            return results.OrderByDescending(c => c.Percentage).ThenBy(c => c.Substances).ToList();
        }

        //Algorithms comparison
        //for bitwise grouping
        /// <summary>
        /// Calculate the contribution of patterns containing the combination of patterns
        /// </summary>
        /// <param name="groupedExposurePatterns"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        private List<FullCoExposureRecord> getExposurePatternFrequencies3(List<DetailCoExposureRecord> groupedExposurePatterns, ICollection<Compound> substances) {
            var groupedExposurePatternsCount = groupedExposurePatterns.Sum(c => c.Frequency);
            var results = new FullCoExposureRecord[groupedExposurePatterns.Count];
            var orderedRecords = groupedExposurePatterns.OrderBy(p => p.NumberOfSubstances).ToArray();

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1000 };
            Parallel.For(0, orderedRecords.Length, parallelOptions, recordIdx => {
                var baseRec = orderedRecords[recordIdx];
                var frequency = baseRec.Frequency;
                //only loop through records higher than index, because we sorted on number of substances already
                if (baseRec.NumberOfSubstances > 0) {
                    for (int i = recordIdx + 1; i < orderedRecords.Length; i++) {
                        var compareRec = orderedRecords[i];
                        if (baseRec.NumberOfSubstances < compareRec.NumberOfSubstances
                            && baseRec.Binary.IsSubSetOf(compareRec.Binary)) {
                            frequency += compareRec.Frequency;
                        }
                    }
                }
                results[baseRec.Row] = new FullCoExposureRecord() {
                    Frequency = frequency,
                    Substances = getCompoundNames(baseRec.Index, substances),
                    NumberOfSubstances = baseRec.NumberOfSubstances,
                    Percentage = 100D * frequency / groupedExposurePatternsCount,
                };
            });
            return results.OrderByDescending(c => c.Percentage).ThenBy(c => c.Substances).ToList();
        }

        private static string getCompoundNames(int[] index, ICollection<Compound> selectedCompounds) {
            var result = string.Join(", ", index.Select(i => selectedCompounds.ElementAt(i).Name));
            return result;
        }
    }
}
